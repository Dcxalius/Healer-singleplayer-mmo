using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Resources;
using Project_1.GameObjects.Entities.GroundEffect;
using Project_1.GameObjects.Entities.Players;
using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Spells.Buff;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Particles;
using Project_1.Textures;
using Project_1.Tiles;
using Project_1.UI.HUD;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Project_1.Textures.AnimatedTextures;
using static Project_1.GameObjects.Unit.Equipment;
using System.Diagnostics;
using Project_1.Items;
using Project_1.UI.UIElements.Guild;
using System.Security.Cryptography;

namespace Project_1.GameObjects.Entities
{
    internal abstract class Entity : MovingObject
    {
        public bool Selected => ObjectManager.Player.Target == this;
        public virtual Entity Target { get => target; }
        protected Entity target = null;

        protected NamePlate namePlate;

        #region UnitData
        protected UnitData UnitData => unitData;
        UnitData unitData;

        public bool HasDestination => destination.HasDestination;
        public Color RelationColor => unitData.RelationData.RelationColor();
        public Relation.RelationToPlayer RelationToPlayer => unitData.RelationData.ToPlayer;
        protected Relation Relation => unitData.RelationData;
        public string Name => unitData.Name;
        public string Class => unitData.ClassData.Name;
        public int CurrentLevel => unitData.Level.CurrentLevel;
        public bool Alive => unitData.Health.CurrentHealth > 0;
        public bool FullHealth => unitData.Health.MaxHealth == unitData.Health.CurrentHealth;
        public float MaxHealth => unitData.Health.MaxHealth;
        public float CurrentHealth => unitData.Health.CurrentHealth;

        public Equipment Equipment => unitData.Equipment;
        public Level Level => unitData.Level;
        public Resource.ResourceType ResourceType => unitData.Resource.Type;
        public float MaxResource => unitData.Resource.MaxValue;
        public float CurrentResource => unitData.Resource.Value;

        public Resource Resource => unitData.Resource;

        public Color ResourceColor => unitData.Resource.ResourceColor;

        public override float MaxSpeed => unitData.MovementData.MaxSpeed;

        public PairReport PrimaryStatReport => unitData.BaseStats.TotalPrimaryStats.NewReport;
        #endregion

        float timeSinceLastAttack = 0;

        protected Corpse corpse;
        ParticleBase bloodsplatter;

        SelectRing selectRing;
        Shadow shadow;


        protected Destination destination;

        public Entity(UnitData aUnitData) : base(new RandomAnimatedTexture(aUnitData.GfxPath, new Point(32), 0, TimeSpan.FromMilliseconds(500)), aUnitData.Position)
        {
            unitData = aUnitData;
            bloodsplatter = new ParticleBase((1000d, 2000d), ParticleBase.OpacityType.Fading, ParticleBase.ColorType.Static, new Color[] { Color.Red }, new Point(1));
            shadow = new Shadow();
            selectRing = new SelectRing();
            spellCast = new SpellCast(this);
            buffList = new BuffList();
            destination = new Destination(this);
            aggroTablesIAmOn = new List<NonFriendly>();

            velocity = unitData.Velocity;
            momentum = UnitData.Momentum;
            CreateNamePlate();
            corpse = new Corpse(unitData.CorpseGfxPath, unitData.LootTable);
        }


        #region Update
        public override void Update()
        {
            if (AmIDead()) return;
            TargetAliveCheck();
            if (unitData.BaseStats.CheckIfResourceRegened()) RefreshPlates();
            
            Movement();
            AttackTarget();

            shadow.UpdatePosition(Position, Size);
            selectRing.UpdatePosition(Position, Size);
            spellCast.UpdateSpellChannel();
            buffList.Update(this);

            MoveNamePlate();

            //HUDManager.RefreshPlateBox(this);
        }

        bool AmIDead()
        {
            if (!Alive)
            {
                Death();
                return true;
            }
            return false;
        }

        protected virtual void Death()
        {
            for (int i = 0; i < aggroTablesIAmOn.Count; i++)
            {
                aggroTablesIAmOn[i].RemoveFromAggroTable(this);
            }

            ObjectManager.RemoveEntity(this);
            RemoveNamePlate();
            if (corpse != null) corpse.SpawnCorpe(Position);

        }
        void TargetAliveCheck()
        {
            if (Target == null) return;
            if (Target.Alive) return;

            RemoveTarget();

        }

        public void Movement()
        {
            destination.Update();
            WorldSpace oldPosition = Position;
            velocity += destination.GetVelocity(unitData.Attack.Range, unitData.MovementData.Speed);
            base.Update();
            CheckForCollisions(oldPosition);
        }


        void CheckForCollisions(WorldSpace aOldPosition)
        {

            List<Rectangle> resultingCollisions = TileManager.CollisionsWithUnwalkable(WorldRectangle);

            if (resultingCollisions.Count != 0)
            {
                for (int i = 0; i < resultingCollisions.Count; i++)
                {

                    WorldSpace collisionDir = WorldSpace.Normalize((WorldSpace)(resultingCollisions[i].Center - WorldRectangle.Center).ToVector2());

                    if (Math.Abs(collisionDir.X) > Math.Abs(collisionDir.Y))
                    {
                        velocity.X = 0;
                        momentum.X = 0;
                        Position = new WorldSpace(aOldPosition.X, Position.Y);
                    }
                    if (Math.Abs(collisionDir.X) < Math.Abs(collisionDir.Y))
                    {
                        velocity.Y = 0;
                        momentum.Y = 0;
                        Position = new WorldSpace(Position.X, aOldPosition.Y);
                    }
                }
            }

        }


        void AttackTarget()
        {
            if (target == null) return;

            if (unitData.Attack.SecondsPerAttack > timeSinceLastAttack)
            {
                timeSinceLastAttack += (float)TimeManager.SecondsSinceLastFrame;
                return;
            }
            AttackIfInRange();
        }

        void AttackIfInRange()
        {

            if (CheckForRelation() && (target.FeetPosition - FeetPosition).ToVector2().Length() < unitData.Attack.Range)
            {
                timeSinceLastAttack = 0;
                float damage = unitData.Attack.GetAttackDamage;

                if (damage <= 0) return;

                target.TakeDamage(this, damage);
                if (target.unitData.Health.CurrentHealth <= 0)
                {
                    target = null;
                }
            }
        }

        protected abstract bool CheckForRelation();
        

        public void ServerTick() //"Server tick"
        {
            if (!InCombat)
            {
                unitData.Tick();
                RefreshPlates();
            }
        }
        #endregion

        #region Target
        public void SetTarget(Entity aEntity)
        {
            target = aEntity;
            HUDManager.SetNewTarget(this, target);
        }

        public void RemoveTarget()
        {
            target = null;
            HUDManager.SetNewTarget(this, null);
        }

        #endregion

        #region Spells
        public bool OffGlobalCooldown => spellCast.OffGlobalCooldown;
        public double RatioOfGlobalCooldownDone => spellCast.RatioOfGlobalCooldownDone;
        SpellCast spellCast;
        BuffList buffList;

        public bool StartCast(Spell aSpell) => spellCast.StartCast(aSpell);

        public void AddBuff(Buff aBuff) => buffList.AddBuff(aBuff, this); 

        public List<Buff> GetAllBuffs() => buffList.GetAllBuffs();
        #endregion

        #region AggroTable
        public virtual bool InCombat => aggroTablesIAmOn.Count > 0;
        List<NonFriendly> aggroTablesIAmOn;

        public void AddedToAggroTable(NonFriendly aNonfriendly)
        {
            if (aggroTablesIAmOn.Contains(aNonfriendly))
            {
                DebugManager.Print(GetType(), aNonfriendly + " tried to add me to a table I thought I was on.");
                return;
            }
            aggroTablesIAmOn.Add(aNonfriendly);
        }

        public void RemovedFromAggroTable(NonFriendly aNonfriendly)
        {
            if (!aggroTablesIAmOn.Contains(aNonfriendly))
            {
                DebugManager.Print(GetType(), aNonfriendly + " tried to remove me from a table I didn't know I was on.");
                return;
            }
            aggroTablesIAmOn.Remove(aNonfriendly);
        }
        #endregion

        #region ValueChanges
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
            RefreshPlates();
            return true;
        }

        public virtual void TakeDamage(Entity aAttacker, float aDamageTaken)
        {
            int beforeDamageTaken = (int)Math.Round(unitData.Health.CurrentHealth);
            unitData.Health.CurrentHealth -= aDamageTaken;
            int afterDamageTaken = (int)Math.Round(unitData.Health.CurrentHealth);
            WorldSpace dirOfFlyingStuff = (FeetPosition - aAttacker.FeetPosition);
            dirOfFlyingStuff.Normalize();
            FloatingText floatingText = new FloatingText((beforeDamageTaken - afterDamageTaken).ToString(), Color.Red, FeetPosition, dirOfFlyingStuff); //TODO: Change to handle attacker and this being in the same place
            ObjectManager.SpawnFloatingText(floatingText);


            ParticleMovement bloodMovement = new ParticleMovement(dirOfFlyingStuff, WorldSpace.Zero, 0.9f);
            //DebugManager.Print(GetType(), ((aDamageTaken / MaxHealth) * 100).ToString());
            ParticleManager.SpawnParticle(bloodsplatter, WorldRectangle, this, bloodMovement, (int)Math.Max(1, (aDamageTaken / MaxHealth) * 100));
            RefreshPlates(); //TODO: Check death here?
        }

        public virtual bool TakeHealing(Entity aHealer, float aHealingTaken)
        {
            float value = aHealingTaken;

            if (FullHealth) return false;
            if (CurrentHealth + value > MaxHealth) value = MaxHealth - CurrentHealth;

            int beforeHealingTaken = (int)Math.Round(unitData.Health.CurrentHealth);
            unitData.Health.CurrentHealth += value;
            int afterHealingTaken = (int)Math.Round(unitData.Health.CurrentHealth);
            WorldSpace ws = (FeetPosition - aHealer.FeetPosition);
            ws.Normalize();
            FloatingText floatingText = new FloatingText((beforeHealingTaken - afterHealingTaken).ToString(), Color.White, FeetPosition, ws); //TODO: Change color to green once text border has been implemented ALSO Change to handle attacker and this being in the same place
            ObjectManager.SpawnFloatingText(floatingText);
            for (int i = 0; i < aggroTablesIAmOn.Count; i++)
            {
                aggroTablesIAmOn[i].AddToAggroTable(aHealer, value);
            }
            RefreshPlates();
            return true;
        }

        public abstract void ExpToParty(int aExpAmount);

        public void GainExperience(int aExpAmount)
        {
            unitData.GainExp(aExpAmount);
            HUDManager.RefreshCharacterWindowExpBar(this);
        }
        protected void CreateNamePlate()
        {

            namePlate = new NamePlate(this);
            HUDManager.AddNamePlate(this, namePlate);

        }

        protected void RemoveNamePlate()
        {
            namePlate = null;
            HUDManager.RemoveNamePlate(this);
        }

        protected virtual void RefreshPlates()
        {
            HUDManager.RefreshPlates(this);

        }

        protected virtual void MoveNamePlate()
        {
            namePlate.Reposition(this);
        }

        #endregion

        #region Click
        public override bool Click(ClickEvent aClickEvent)
        {
            if (Camera.Camera.WorldRectToScreenRect(WorldRectangle).Contains(aClickEvent.AbsolutePos.ToPoint()))
            {
                if (aClickEvent.NoModifiers())
                {
                    ObjectManager.Player.SetTarget(this);
                }
                ClickedOn(aClickEvent);

                return true;
            }
            return false;
        }

        protected virtual void ClickedOn(ClickEvent aClickEvent)
        {
            if (aClickEvent.NoModifiers() && aClickEvent.ButtonPressed == InputManager.ClickType.Right)
            {
                ObjectManager.Player.Party.IssueTargetOrder(this);
            }
        }
        #endregion

        #region Equipment
        public Item EquipInParticularSlot(Items.SubTypes.Equipment aEquipment, Slot aSlot)
        {
            Item item = Equipment.EquipInParticularSlot(aEquipment, aSlot);
            unitData.BaseStats.RefreshEquipmentStats(Equipment.EquipmentStats);
            HUDManager.RefreshCharacterWindowStats(unitData.BaseStats.TotalPrimaryStats.NewReport);
            RefreshPlates();

            return item;
        }
        public Item Equip(Items.SubTypes.Equipment aEquipment)
        {
            Item item = Equipment.Equip(aEquipment);
            unitData.BaseStats.RefreshEquipmentStats(Equipment.EquipmentStats);
            HUDManager.RefreshCharacterWindowStats(unitData.BaseStats.TotalPrimaryStats.NewReport);
            RefreshPlates();

            return item;
        }

        public (Item, Item) EquipTwoHander(Items.SubTypes.Equipment aEquipment)
        {
            (Item, Item) returnable = Equipment.EquipTwoHander(aEquipment);
            unitData.BaseStats.RefreshEquipmentStats(Equipment.EquipmentStats);
            HUDManager.RefreshCharacterWindowStats(unitData.BaseStats.TotalPrimaryStats.NewReport);
            RefreshPlates();

            return returnable;
        }

        #endregion

        

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch aBatch)
        {
            shadow.Draw(aBatch, this);
            selectRing.Draw(aBatch, this);
            base.Draw(aBatch);
        }
    }
}
