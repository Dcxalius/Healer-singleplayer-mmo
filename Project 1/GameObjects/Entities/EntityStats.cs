using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Entities.Corspes;
using Project_1.GameObjects.FloatingTexts;
using Project_1.GameObjects.Spawners;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Classes;
using Project_1.GameObjects.Unit.Resources;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Items;
using Project_1.Particles;
using Project_1.UI.HUD;
using Project_1.UI.HUD.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.GameObjects.Unit.Equipment;

namespace Project_1.GameObjects.Entities
{
    internal partial class Entity
    {
        protected UnitData UnitData => unitData;
        UnitData unitData;

        protected NamePlate namePlate;
        public UnitType UnitType => unitData.UnitType;

        public virtual Color MinimapColor => Color.White;

        public bool HasDestination => unitData.Destination.HasDestination;
        public Destination Destination => unitData.Destination;
        public Color RelationColor => unitData.RelationData.RelationColor();
        public Relation.RelationToPlayer RelationToPlayer => unitData.RelationData.ToPlayer;
        protected Relation Relation => unitData.RelationData;
        public string Name => unitData.Name;
        public string Class => unitData.ClassData.Name;
        public virtual ClassData ClassData => unitData.ClassData;
        public int CurrentLevel => unitData.Level.CurrentLevel;
        public bool Alive => unitData.Health.CurrentHealth > 0;
        public bool FullHealth => unitData.Health.MaxHealth == unitData.Health.CurrentHealth;
        public float MaxHealth => unitData.Health.MaxHealth;
        public float CurrentHealth => unitData.Health.CurrentHealth;

        public SecondaryStats SecondaryStats => unitData.SecondaryStats;
        public int DefenseSkill => unitData.DefenseSkill;
        public WeaponSkill WeaponSkill => unitData.WeaponSkill;
        public bool IsDualWielding => Equipment.IsDualWielding;
        public Equipment Equipment => unitData.Equipment;
        public Level Level => unitData.Level;
        public Resource.ResourceType ResourceType => unitData.Resource.Type;
        public float MaxResource => unitData.Resource.MaxValue;
        public float CurrentResource => unitData.Resource.Value;
        public Resource Resource => unitData.Resource;

        public Color ResourceColor => unitData.Resource.ResourceColor;

        public override float MaxSpeed => unitData.MovementData.MaxSpeed;

        public PairReport PrimaryStatReport => unitData.BaseStats.StatReport;


        bool namePlateRequiresUpdate;
        ParticleBase bloodsplatter;


        bool AmIDead()
        {
            if (!Alive)
            {
                Death();
                return true;
            }
            return false;
        }


        void TargetAliveCheck()
        {
            if (Target == null) return;
            if (Target.Alive) return;

            RemoveTarget();

        }
        protected virtual void Death()
        {
            for (int i = 0; i < aggroTablesIAmOn.Count; i++)
            {
                aggroTablesIAmOn[i].RemoveFromAggroTable(this);
            }

            ObjectManager.RemoveEntity(this);
            RemoveNamePlate();
            Corpse c = new Corpse(unitData.CorpseGfxPath, unitData.LootTable, FeetPosition);

        }


        public bool ResourceGain(Entity aEntity, float aValue, Resource.ResourceType aResourceType)
        {
            if (aResourceType != ResourceType) return false;

            if (MaxResource == CurrentResource) return false;

            float value = aValue;

            if (MaxResource < CurrentResource + value)
            {
                value = MaxResource - CurrentResource;
            }

            for (int i = 0; i < aggroTablesIAmOn.Count; i++)
            {
                aggroTablesIAmOn[i].AddToAggroTable(aEntity, value);
            }

            unitData.Resource.Value += value;
            FlagForRefresh();
            return true;
        }

        public virtual bool TakeHealing(Entity aHealer, float aHealingTaken)
        {
            if (FullHealth) return false;

            float value = CalculateHealing(aHealingTaken);
            for (int i = 0; i < aggroTablesIAmOn.Count; i++)
            {
                aggroTablesIAmOn[i].AddToAggroTable(aHealer, value);
            }

            WorldSpace dir = GetDirOfFloatingText(aHealer.FeetPosition);

            SpawnFlyingText(aHealingTaken.ToString(), dir, Color.White);//TODO: Change color to green once text border has been implemented
            FlagForRefresh();
            return true;
        }

        float CalculateHealing(float aHealingTaken)
        {
            float value = aHealingTaken;
            if (CurrentHealth + value > MaxHealth) value = MaxHealth - CurrentHealth;

            int beforeHealingTaken = (int)Math.Round(unitData.Health.CurrentHealth);
            unitData.Health.CurrentHealth += value;
            int afterHealingTaken = (int)Math.Round(unitData.Health.CurrentHealth);

            return beforeHealingTaken - afterHealingTaken;
        }

        public virtual void RecieveAttack(HitTable.HitResult aHitResult, Entity aAttacker, Unit.Attack aAttack, Damage aDamageTaken)
        {
            string resultString = "";
            Color resultColor = Color.White;
            if (aHitResult <= HitTable.HitResult.Parry)
            {
                switch (aHitResult)
                {
                    case HitTable.HitResult.Miss:
                        resultString = "Miss";
                        resultColor = Color.Gray;
                        break;
                    case HitTable.HitResult.Dodge:
                        resultString = "Dodge";
                        resultColor = Color.DarkGray;
                        //TODO: Trigger dodge event
                        break;
                    case HitTable.HitResult.Parry:
                        resultString = "Parry";
                        resultColor = Color.DarkSlateGray;
                        //TODO: Trigger parry event
                        break;
                }

                SpawnFlyingText(resultString, GetDirOfFloatingText(aAttacker.FeetPosition), resultColor);
                return;
            }
            Damage premitigation = new Damage(aDamageTaken);
            switch (aHitResult)
            {
                case HitTable.HitResult.Glancing:
                    Debug.Assert(UnitType != UnitType.Player);
                    aDamageTaken.ApplyGlancingBlowDamage(aAttacker, aAttack, this);
                    resultColor = Color.DimGray;
                    break;
                case HitTable.HitResult.Block:
                    aDamageTaken.ApplyBlocked(aAttacker, this);
                    resultColor = Color.LightGray;
                    //TODO: Trigger block event
                    if (!aDamageTaken.ContainsDamage)
                    {
                        resultString = "Blocked";
                        SpawnFlyingText(resultString, GetDirOfFloatingText(aAttacker.FeetPosition), resultColor);
                        return;
                    }
                    break;
                case HitTable.HitResult.Crit:
                    aDamageTaken.ApplyCriticalStrike(aAttacker, this);
                    resultColor = Color.Yellow;
                    break;
                case HitTable.HitResult.Crushing:
                    aDamageTaken.ApplyCrushingDamage(aAttacker, this);
                    resultColor = Color.Orange;
                    break;
                case HitTable.HitResult.Hit:
                    resultColor = Color.Red; //TODO: Instead of just using text color, have the text color depend on the damage type and glancing/blocked/crit/crushing/hit change the border color
                    break;
                default:
                    break;
            }
            aDamageTaken.ApplyDamageReduction(aAttacker, this, aAttack);

            if (aDamageTaken.Sum <= 0) return;

            for (int i = 0; i < aDamageTaken.Count; i++)
            {
                //TODO: When different damage types are implemented, show different colors for different damage types
                // For example, physical damage could be red, fire damage orange, frost damage blue, etc.
                // And introduce a offset to the floating text position so that multiple damage types don't overlap
                unitData.Health.CurrentHealth -= aDamageTaken[aDamageTaken.Types[i]];

                WorldSpace dir = GetDirOfFloatingText(aAttacker.FeetPosition);
                SpawnFlyingText(resultString, dir, resultColor);
            }

            ParticleMovement bloodMovement = new ParticleMovement(GetDirOfFloatingText(aAttacker.FeetPosition), WorldSpace.Zero, 0.9f);
            ParticleManager.SpawnParticle(bloodsplatter, WorldRectangle, this, bloodMovement, (int)Math.Max(1, Math.Min((aDamageTaken.Sum / MaxHealth) * 100, 100)));
            FlagForRefresh(); //TODO: Check death here?
        }

        void SpawnFlyingText(string aHealthChangeValue, WorldSpace aDirOfFlyingStuff, Color aTextColor) => FloatingTextManager.AddFloatingText(new FloatingText(aHealthChangeValue, aTextColor, FeetPosition, aDirOfFlyingStuff));


        WorldSpace GetDirOfFloatingText(WorldSpace aFeetPosOfTriggerer)
        {
            WorldSpace dirOfFlyingStuff = (FeetPosition - aFeetPosOfTriggerer);
            if (dirOfFlyingStuff == WorldSpace.Zero)
            {
                dirOfFlyingStuff.Y = 1;
            }
            dirOfFlyingStuff.Normalize();
            return dirOfFlyingStuff;
        }



        public abstract void ExpToParty(int aExpAmount);

        public void GainExperience(int aExpAmount)
        {
            unitData.GainExp(aExpAmount);

            if (!(this is Friendly)) return;
            HUDManager.windowHandler.RefreshCharacterWindowExpBar(this as Friendly);
        }


        protected void CreateNamePlate()
        {
            namePlate = new NamePlate(this);
            HUDManager.namePlateHandler.AddNamePlate(this, namePlate);

        }

        protected void RemoveNamePlate()
        {
            namePlate = null;
            HUDManager.namePlateHandler.RemoveNamePlate(this);
        }

        protected void FlagForRefresh() => namePlateRequiresUpdate = true;

        public virtual void RefreshPlates()
        {
            if (!namePlateRequiresUpdate) return;
            HUDManager.plateBoxHandler.RefreshPlates(this);
        }

        protected virtual void MoveNamePlate()
        {
            namePlate.Reposition(this);
        }

        public Item EquipInParticularSlot(Items.SubTypes.Equipment aEquipment, Slot aSlot)
        {
            Item item = Equipment.EquipInParticularSlot(aEquipment, aSlot);
            unitData.BaseStats.RefreshEquipmentStats(Equipment.EquipmentStats);
            FlagForRefresh();
            return item;
        }
        public Item Equip(Items.SubTypes.Equipment aEquipment)
        {
            Item item = Equipment.Equip(aEquipment);
            unitData.BaseStats.RefreshEquipmentStats(Equipment.EquipmentStats);
            FlagForRefresh();

            return item;
        }

        public (Item, Item) EquipTwoHander(Items.SubTypes.Equipment aEquipment)
        {
            (Item, Item) returnable = Equipment.EquipTwoHander(aEquipment);
            unitData.BaseStats.RefreshEquipmentStats(Equipment.EquipmentStats);
            FlagForRefresh();

            return returnable;
        }
    }
}
