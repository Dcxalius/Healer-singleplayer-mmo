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
using Project_1.Textures.AnimatedTextures;
using static Project_1.GameObjects.Unit.Equipment;
using System.Diagnostics;
using Project_1.Items;
using Project_1.GameObjects.Unit.Classes;
using System.Threading.Tasks.Dataflow;
using Project_1.GameObjects.FloatingTexts;
using Project_1.GameObjects.Entities.Corspes;

namespace Project_1.GameObjects.Entities
{
    internal abstract class Entity : WorldObject
    {
        public bool Selected => ObjectManager.Player.Target == this;
        public virtual Entity Target { get => target; }
        protected Entity target = null;

        public override WorldSpace FeetSize { get => new WorldSpace(Size.X, Size.Y / 2); }

        public override Rectangle WorldRectangle
        {
            get
            {
                Point pos = FeetPosition.ToPoint() - new Point((int)(FeetSize.X / 2), (int)(FeetSize.Y / 2));
                return new Rectangle(pos, FeetSize.ToPoint());
            }
        }

        protected NamePlate namePlate;

        #region UnitData
        protected UnitData UnitData => unitData;
        UnitData unitData;

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

        public Equipment Equipment => unitData.Equipment;
        public Level Level => unitData.Level;
        public Resource.ResourceType ResourceType => unitData.Resource.Type;
        public float MaxResource => unitData.Resource.MaxValue;
        public float CurrentResource => unitData.Resource.Value;

        public Resource Resource => unitData.Resource;

        public Color ResourceColor => unitData.Resource.ResourceColor;

        public override float MaxSpeed => unitData.MovementData.MaxSpeed;

        public PairReport PrimaryStatReport => unitData.BaseStats.StatReport;
        #endregion

        float timeSinceLastAttack = 0;

        ParticleBase bloodsplatter;
        bool namePlateRequiresUpdate;

        public Entity(UnitData aUnitData) : base(new RandomAnimatedTexture(aUnitData.GfxPath, new Point(32), 0, TimeSpan.FromMilliseconds(500)), aUnitData.Position)
        {
            unitData = aUnitData;
            unitData.Equipment.SetOwner(this);
            unitData.BaseStats.SetOwner(this);
            unitData.Destination.SetOwner(this);
            bloodsplatter = new ParticleBase((1000d, 2000d), ParticleBase.OpacityType.Fading, ParticleBase.ColorType.Static, new Color[] { Color.Red }, new Point(1));
            namePlateRequiresUpdate = false;
            
            spellCast = new SpellCast(this);
            buffList = new BuffList();
            aggroTablesIAmOn = new List<NonFriendly>();

            velocity = unitData.Velocity;
            momentum = UnitData.Momentum;

            groundEffects.Add(new Shadow());
            groundEffects.Add(new SelectRing());
            CreateNamePlate();
        }

        public void Delete()
        {
            RemoveNamePlate();

        }



        #region Update
        public override void Update()
        {
            if (AmIDead()) return;
            TargetAliveCheck();
            if (unitData.BaseStats.CheckIfResourceRegened()) FlagForRefresh();
            
            MoveNamePlate();
            Movement();
            AttackTarget();
            spellCast.UpdateSpellChannel();
            buffList.Update(this);


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
            Corpse c = new Corpse(unitData.CorpseGfxPath, unitData.LootTable, FeetPosition);

        }
        void TargetAliveCheck()
        {
            if (Target == null) return;
            if (Target.Alive) return;

            RemoveTarget();

        }

        public void Movement()
        {
            Destination.Update();

            float minAttackRange = GetMinAttackRange();
            
            velocity += Destination.GetVelocity(minAttackRange, unitData.MovementData.Speed, new WorldSpace(FeetSize));
            base.Update(); //TODO: This shouldnt be here
            CheckForCollisions();

            unitData.Position = FeetPosition;
            unitData.Momentum = momentum;
            unitData.Velocity = velocity;
        }

        float GetMinAttackRange()
        {
            float minAttackRange;
            if (unitData.AttackData.OffHandAttack != null && unitData.AttackData.MainHandAttack != null)
            {
                minAttackRange = Math.Min(unitData.AttackData.MainHandAttack.Range, unitData.AttackData.OffHandAttack.Range);
            }
            else if (unitData.AttackData.MainHandAttack == null)
            {
                minAttackRange = unitData.AttackData.OffHandAttack.Range;
            }
            else
            {
                minAttackRange = unitData.AttackData.MainHandAttack.Range;
            }
            return minAttackRange;
        }


        void CheckForCollisions() //TODO: Rework this?
        {

            List<(Rectangle, Rectangle)> resultingCollisions = TileManager.CollisionsWithUnwalkable(this);

            if (resultingCollisions.Count != 0)
            {
                for (int i = 0; i < resultingCollisions.Count; i++)
                {
                    //TODO: Ponder how to make it not jump
                    //Related to the fact that when colliding with corners it uses feetpos rather than the border of feet
                    
                    if (FeetPosition.X - resultingCollisions[i].Item1.Left < 0)
                    {
                        FeetPosition = new WorldSpace(MathF.Round(FeetPosition.X), FeetPosition.Y) - new WorldSpace(resultingCollisions[i].Item1.Width, 0);
                        velocity.X = 0;
                        momentum.X = 0;
                        //DebugManager.Print(GetType(), (FeetPosition.X - resultingCollisions[i].Item1.Left).ToString());
                    }
                    if (FeetPosition.X - resultingCollisions[i].Item1.Right > 0)
                    {
                        FeetPosition = new WorldSpace(MathF.Round(FeetPosition.X), FeetPosition.Y) + new WorldSpace(resultingCollisions[i].Item1.Width, 0);
                        velocity.X = 0;
                        momentum.X = 0;
                        //DebugManager.Print(GetType(), (FeetPosition.X - resultingCollisions[i].Item1.Right).ToString());

                    }
                    if (FeetPosition.Y - resultingCollisions[i].Item1.Top < 0)
                    {
                        FeetPosition = new WorldSpace(FeetPosition.X, MathF.Round(FeetPosition.Y)) - new WorldSpace(0, resultingCollisions[i].Item1.Height);
                        velocity.Y = 0;
                        momentum.Y = 0;
                        //DebugManager.Print(GetType(), (FeetPosition.Y - resultingCollisions[i].Item1.Top).ToString());
                    }
                    if (FeetPosition.Y - resultingCollisions[i].Item1.Bottom > 0)
                    {
                        FeetPosition = new WorldSpace(FeetPosition.X, MathF.Round(FeetPosition.Y)) + new WorldSpace(0, resultingCollisions[i].Item1.Height);
                        velocity.Y = 0;
                        momentum.Y = 0;
                        //DebugManager.Print(GetType(), (FeetPosition.Y - resultingCollisions[i].Item1.Bottom).ToString());
                    }
                }
            }

        }


        void AttackTarget()
        {
            if (target == null) return;
            if (!CheckForRelation()) return;

            AttackData a = unitData.AttackData;
            if ((target.FeetPosition - FeetPosition).ToVector2().Length() >= GetMinAttackRange() - Size.X / 2 - target.Size.X / 2) return;
            
            CheckAttackSpeed(ref unitData.NextAvailableMainHandAttack, a.MainHandAttack);
            if (target == null) return;
            CheckAttackSpeed(ref unitData.NextAvailableOffHandAttack, a.OffHandAttack);
        }

        void CheckAttackSpeed(ref TimeSpan aLockoutTime, Attack aAttack)
        {
            if (aAttack == null) return;
            if (aLockoutTime > TimeManager.TotalFrameTimeAsTimeSpan) return;

            aLockoutTime = TimeManager.TotalFrameTimeAsTimeSpan + TimeSpan.FromSeconds(aAttack.SecondsPerAttack);
            AttackIfInRange(aAttack);
        }

        void AttackIfInRange(Attack aAttack)
        {

            timeSinceLastAttack = 0;
            float damage = aAttack.GetAttackDamage;

            if (damage <= 0) return;
            //TODO: Calculate misses
            //TODO: Calculate mitigation (dodge, parry, block)
            //TODO: Calculate crit
            //TODO: Proc onhits
            target.TakeDamage(this, damage);
            TargetAliveCheck();
        }

        protected abstract bool CheckForRelation();
        

        public void ServerTick() //"Server tick"
        {
            if (!InCombat)
            {
                unitData.Tick();
                FlagForRefresh();
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
            
            SpawnFlyingText(aHealingTaken, dir, Color.White);//TODO: Change color to green once text border has been implemented
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

        public virtual void TakeDamage(Entity aAttacker, float aDamageTaken)
        {
            float damageTaken = CalculateDamage(aAttacker, aDamageTaken);
            WorldSpace dir = GetDirOfFloatingText(aAttacker.FeetPosition);
            SpawnFlyingText(damageTaken, dir, Color.Red);

            ParticleMovement bloodMovement = new ParticleMovement(dir, WorldSpace.Zero, 0.9f);
            ParticleManager.SpawnParticle(bloodsplatter, WorldRectangle, this, bloodMovement, (int)Math.Max(1, Math.Min((damageTaken / MaxHealth) * 100, 100)));
            FlagForRefresh(); //TODO: Check death here?
        }

        void SpawnFlyingText(float aHealthChangeValue, WorldSpace aDirOfFlyingStuff, Color aTextColor)
        {
            FloatingText floatingText = new FloatingText(aHealthChangeValue.ToString(), aTextColor, FeetPosition, aDirOfFlyingStuff);
            FloatingTextManager.AddFloatingText(floatingText);

        }

        float CalculateDamage(Entity aAttacker, float aDamageTaken)
        {
            float damageAfterArmor = (1f - unitData.BaseStats.TotalArmor.GetGetReductionPercentage(aAttacker.CurrentLevel)) * aDamageTaken; //TODO: Check if this gives right values
            float healthBeforeHit = unitData.Health.CurrentHealth;
            unitData.Health.CurrentHealth -= damageAfterArmor;
            float healthAfterHit = unitData.Health.CurrentHealth;
            float damageVisiblyTaken = MathF.Round(healthBeforeHit - healthAfterHit, 1);
            return damageVisiblyTaken;
        }

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
            HUDManager.RefreshCharacterWindowExpBar(this as Friendly);
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

        protected void FlagForRefresh() => namePlateRequiresUpdate = true;

        public virtual void RefreshPlates()
        {
            if (!namePlateRequiresUpdate) return;
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
                
                ClickedOn(aClickEvent);

                return true;
            }
            return false;
        }

        protected virtual void ClickedOn(ClickEvent aClickEvent)
        {
            if (aClickEvent.NoModifiers())
            {
                ObjectManager.Player.SetTarget(this);
            }

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

        #endregion
    }
}
