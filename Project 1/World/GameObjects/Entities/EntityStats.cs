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
using Project_1.Messaging;
using Project_1.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.GameObjects.Unit.Equipment;
using Project_1.GameObjects.Entities.Friendlies;
using System.Globalization;
using Project_1.Managers;

namespace Project_1.GameObjects.Entities
{
    internal partial class Entity
    {
        public int GetTalentRank(int aTalentId) => learntTalents.SingleOrDefault(x => x.id == aTalentId).rank;
        (int id, int rank)[] learntTalents;

        protected UnitData UnitData => unitData;
        UnitData unitData;

        protected bool HasNamePlate => hasNamePlate;
        bool hasNamePlate;
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
        public double MaxHealth => unitData.Health.MaxHealth;
        public double CurrentHealth
        {
            get => unitData.Health.CurrentHealth;
            set
            {
                unitData.Health.CurrentHealth = value;
                FlagForRefresh();
            }
        }

        public SecondaryStats SecondaryStats => unitData.SecondaryStats;
        internal void RefreshSecondaryStats()
        {
            ThreadAffinity.AssertSimThread();
            unitData.SecondaryStats.Refresh(unitData);
        }
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

        internal EntityUiSnapshot BuildUiSnapshot()
        {
            return new EntityUiSnapshot(
                RenderId,
                Name,
                Class,
                RelationToPlayer.ToRelationToPlayerKind(),
                RelationColor,
                CurrentLevel,
                CurrentHealth,
                MaxHealth,
                CurrentResource,
                MaxResource,
                ResourceColor,
                FeetPosition,
                WorldRectangle.Height);
        }


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
            ThreadAffinity.AssertSimThread();
            Events.Clear();
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
            ThreadAffinity.AssertSimThread();
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
            ThreadAffinity.AssertSimThread();
            if (FullHealth) return false;

            double value = CalculateHealing(aHealingTaken);
            for (int i = 0; i < aggroTablesIAmOn.Count; i++)
            {
                aggroTablesIAmOn[i].AddToAggroTable(aHealer, (float)(value / aggroTablesIAmOn.Count));
            }

            WorldSpace dir = GetDirOfFloatingText(aHealer.FeetPosition);

            SpawnFlyingText(FormatHealthDelta(value), dir, Color.Green, Color.Black, 1f);//TODO: Change color to green once text border has been implemented
            FlagForRefresh();
            return true;
        }

        double CalculateHealing(double aHealingTaken)
        {
            return ApplyHealthDelta(aHealingTaken);
        }

        double ApplyHealthDelta(double aDelta)
        {
            double before = unitData.Health.CurrentHealth;
            double after = before + aDelta;
            if (after < 0) after = 0;
            if (after > MaxHealth) after = MaxHealth;
            unitData.Health.CurrentHealth = after;
            return after - before;
        }

        string FormatHealthDelta(double aDelta)
        {
            double abs = Math.Abs(aDelta);
            const double epsilon = 0.005d;
            if (abs < epsilon) return "0";
            double rounded = Math.Round(abs);
            if (Math.Abs(abs - rounded) < epsilon) return rounded.ToString(CultureInfo.InvariantCulture);
            return abs.ToString("0.##", CultureInfo.InvariantCulture);
        }


        public abstract void ExpToParty(int aExpAmount);

        public void GainExperience(int aExpAmount)
        {
            ThreadAffinity.AssertSimThread();
            unitData.GainExp(aExpAmount);

            if (!(this is Friendly)) return;
            MailboxManager.PublishUiEvent(new ExperienceRefreshed(RenderId, RelationToPlayer.ToRelationToPlayerKind(), CurrentLevel, Level.Experience));
        }


        protected void CreateNamePlate()
        {
            ThreadAffinity.AssertSimThread();
            hasNamePlate = true;
            MailboxManager.PublishUiEvent(new NamePlateAdded(BuildUiSnapshot()));

        }

        protected void RemoveNamePlate()
        {
            ThreadAffinity.AssertSimThread();
            hasNamePlate = false;
            MailboxManager.PublishUiEvent(new NamePlateRemoved(RenderId));
        }

        protected void FlagForRefresh() => namePlateRequiresUpdate = true;

        public virtual void RefreshPlates()
        {
            ThreadAffinity.AssertSimThread();
            if (!namePlateRequiresUpdate) return;
            MailboxManager.PublishUiEvent(new PlateRefreshRequested(BuildUiSnapshot()));
        }

        public Item EquipInParticularSlot(Items.SubTypes.Equipment aEquipment, Slot aSlot)
        {
            ThreadAffinity.AssertSimThread();
            Item item = Equipment.EquipInParticularSlot(aEquipment, aSlot);
            unitData.BaseStats.RefreshEquipmentStats(Equipment.EquipmentStats);
            FlagForRefresh();
            return item;
        }
        public Item Equip(Items.SubTypes.Equipment aEquipment)
        {
            ThreadAffinity.AssertSimThread();
            Item item = Equipment.Equip(aEquipment);
            unitData.BaseStats.RefreshEquipmentStats(Equipment.EquipmentStats);
            FlagForRefresh();

            return item;
        }

        public (Item, Item) EquipTwoHander(Items.SubTypes.Equipment aEquipment)
        {
            ThreadAffinity.AssertSimThread();
            (Item, Item) returnable = Equipment.EquipTwoHander(aEquipment);
            unitData.BaseStats.RefreshEquipmentStats(Equipment.EquipmentStats);
            FlagForRefresh();

            return returnable;
        }
    }
}
