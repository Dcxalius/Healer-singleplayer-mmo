﻿using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Resources;
using Project_1.GameObjects.Entities.GroundEffect;
using Project_1.GameObjects.Entities.Players;
using Project_1.GameObjects.Entities.Temp;
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

namespace Project_1.GameObjects.Entities
{
    internal abstract class Entity : MovingObject
    {
        public bool Selected => ObjectManager.Player.Target == this;
        public virtual Entity Target { get => target; set => target = value; }
        protected Entity target = null;
        public virtual bool InCombat => aggroTablesIAmOn.Count > 0;

        #region UnitData
        protected UnitData UnitData => unitData;
        UnitData unitData;

        public bool HasDestination => destination.HasDestination;
        public Color RelationColor => unitData.RelationData.RelationColor();
        public Relation.RelationToPlayer RelationToPlayer => unitData.RelationData.ToPlayer;
        public string Name => unitData.Name;
        public bool Alive => unitData.HealthData.CurrentHealth > 0;
        public bool FullHealth => unitData.HealthData.MaxHealth == unitData.HealthData.CurrentHealth;
        public float MaxHealth => unitData.HealthData.MaxHealth;
        public float CurrentHealth => unitData.HealthData.CurrentHealth;

        public Resource.ResourceType ResourceType => unitData.Resource.Type;
        public float MaxResource => unitData.Resource.MaxValue;
        public float CurrentResource => unitData.Resource.Value;

        public Resource Resource => unitData.Resource;

        public Color ResourceColor => unitData.Resource.ResourceColor;

        public override float MaxSpeed => unitData.MovementData.MaxSpeed;
        #endregion

        List<NonFriendly> aggroTablesIAmOn;


        float timeSinceLastAttack = 0;




        protected Corpse corpse;
        ParticleBase bloodsplatter;

        public bool OffGlobalCooldown => spellCast.OffGlobalCooldown;
        public double RatioOfGlobalCooldownDone => spellCast.RatioOfGlobalCooldownDone;
        SpellCast spellCast;

        SelectRing selectRing;
        Shadow shadow;

        BuffList buffList;

        protected Destination destination;

        public Entity(Texture aTexture, WorldSpace aStartingPos, Corpse aCorpse = null) : base(aTexture, aStartingPos)
        {
            corpse = aCorpse;
            unitData = ObjectFactory.GetData(GetType().Name);
            bloodsplatter = new ParticleBase((1000d, 2000d), ParticleBase.OpacityType.Fading, ParticleBase.ColorType.Static, new Color[] { Color.Red }, new Point(1));
            shadow = new Shadow();
            selectRing = new SelectRing();
            spellCast = new SpellCast(this);
            buffList = new BuffList();
            destination = new Destination(this);
            aggroTablesIAmOn = new List<NonFriendly>();
        }


        public override void Update()
        {
            if (Dead()) return;
            TargetAliveCheck();
            unitData.Update();
            Movement();
            AttackTarget();

            shadow.UpdatePosition(Position, Size);
            selectRing.UpdatePosition(Position, Size);
            spellCast.UpdateSpellChannel();
            buffList.Update(this);
        }

        public void Movement()
        {
            destination.Update();
            WorldSpace oldPosition = Position;
            velocity += destination.GetVelocity(unitData.AttackData.Range, unitData.MovementData.Speed);
            base.Update();
            CheckForCollisions(oldPosition);
        }

        public bool StartCast(Spell aSpell) => spellCast.StartCast(aSpell);

        public void AddBuff(Buff aBuff) => buffList.AddBuff(aBuff, this); 

        public List<Buff> GetAllBuffs() => buffList.GetAllBuffs();

        void TargetAliveCheck()
        {
            if (Target == null) return;
            if (Target.Alive) return;

            Target = null;
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

            return true;
        }

        

        public void ServerTick() //"Server tick"
        {
            if (!InCombat)
            {
                unitData.Tick();
            }
        }

       

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

        public virtual void TakeDamage(Entity aAttacker, float aDamageTaken)
        {
            unitData.HealthData.CurrentHealth -= aDamageTaken;
            WorldSpace dirOfFlyingStuff = (FeetPosition - aAttacker.FeetPosition);
            dirOfFlyingStuff.Normalize();
            FloatingText floatingText = new FloatingText(aDamageTaken.ToString(), Color.Red, FeetPosition, dirOfFlyingStuff); //TODO: Change to handle attacker and this being in the same place
            ObjectManager.SpawnFloatingText(floatingText);


            ParticleMovement bloodMovement = new ParticleMovement(dirOfFlyingStuff, WorldSpace.Zero, 0.9f);
            DebugManager.Print(GetType(), ((aDamageTaken / MaxHealth) * 100).ToString());
            ParticleManager.SpawnParticle(bloodsplatter, WorldRectangle, this, bloodMovement, (int)Math.Max(1, (aDamageTaken / MaxHealth) * 100));
        }

        public virtual bool TakeHealing(Entity aHealer, float aHealingTaken)
        {
            float value = aHealingTaken;

            if (FullHealth) return false;
            if (CurrentHealth + value > MaxHealth) value = MaxHealth - CurrentHealth;

            unitData.HealthData.CurrentHealth += value;
            WorldSpace ws = (FeetPosition - aHealer.FeetPosition);
            ws.Normalize();
            FloatingText floatingText = new FloatingText(value.ToString(), Color.White, FeetPosition, ws); //TODO: Change color to green once text border has been implemented ALSO Change to handle attacker and this being in the same place
            ObjectManager.SpawnFloatingText(floatingText);
            for (int i = 0; i < aggroTablesIAmOn.Count; i++)
            {
                aggroTablesIAmOn[i].AddToAggroTable(aHealer, value);
            }

            return true;
        }

        public override bool Click(ClickEvent aClickEvent)
        {
            if (Camera.Camera.WorldRectToScreenRect(WorldRectangle).Contains(aClickEvent.AbsolutePos.ToPoint()))
            {
                if (aClickEvent.NoModifiers())
                {
                    ObjectManager.Player.Target = this;
                    HUDManager.SetNewTarget();
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
                ObjectManager.Player.IssueTargetOrder(this);
            }
        }




        protected virtual bool Dead()
        {
            if (!Alive)
            {
                for (int i = 0;  i < aggroTablesIAmOn.Count; i++)
                {
                    aggroTablesIAmOn[i].RemoveFromAggroTable(this);
                }
                
                ObjectManager.RemoveEntity(this);
                if (corpse != null) corpse.SpawnCorpe(Position); //Make this spawn a default corpse if corpse is null
                return true;
            }
            return false;
        }

        void AttackTarget()
        {
            if (target == null) return;

            if (unitData.AttackData.SecondsPerAttack > timeSinceLastAttack)
            {
                timeSinceLastAttack += (float)TimeManager.SecondsSinceLastFrame;
                return;
            }
            AttackIfInRange();
        }

        void AttackIfInRange()
        {

            if (CheckForRelation() && (target.FeetPosition - FeetPosition).ToVector2().Length() < unitData.AttackData.Range)
            {
                timeSinceLastAttack = 0;
                target.TakeDamage(this, unitData.AttackData.Damage);
                if (target.unitData.HealthData.CurrentHealth <= 0)
                {
                    target = null;
                }
            }
        }

        bool CheckForRelation()
        {
            if (target.RelationToPlayer == Relation.RelationToPlayer.Self && RelationToPlayer == Relation.RelationToPlayer.Friendly)
            {
                return false;
            }
            if (target.RelationToPlayer != RelationToPlayer)
            {
                return true;
            }

            return false;
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

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch aBatch)
        {
            shadow.Draw(aBatch, this);
            selectRing.Draw(aBatch, this);
            base.Draw(aBatch);
        }
    }
}
