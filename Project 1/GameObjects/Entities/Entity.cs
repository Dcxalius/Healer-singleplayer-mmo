using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Entities.Players;
using Project_1.GameObjects.Entities.Resources;
using Project_1.GameObjects.Spells;
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
        public enum BehaviourWhenAttacked
        {
            Flee,
            Retaliate,
            RetaliateButFleeWhenLow
        }

        protected UnitData UnitData { get => unitData; }
        public bool HasDestination { get => destinations.Count > 0; }
        public Color RelationColor { get => unitData.RelationColor(); }
        public UnitData.RelationToPlayer Relation { get => unitData.Relation; }
        public virtual Entity Target { get => target; set => target = value; }

        public string Name { get => unitData.Name; }
        public bool Alive { get => unitData.CurrentHealth > 0; }
        public bool FullHealth { get => unitData.MaxHealth == unitData.CurrentHealth; }
        public float MaxHealth { get => unitData.MaxHealth; }
        public float CurrentHealth { get => unitData.CurrentHealth; }

        public Resource.ResourceType ResourceType { get => unitData.Resource.Type; }
        public float MaxResource { get => unitData.MaxResource; }
        public float CurrentResource { get => unitData.CurrentResource; }

        public Color ResourceColor { get => unitData.ResourceColor; }

        public override float MaxSpeed { get => unitData.MaxSpeed; }

        Rectangle shadowPos;

        List<NonFriendly> aggroTablesIAmOn = new List<NonFriendly>();

        static Texture SelectTexture = new Texture(new GfxPath(GfxType.Object, "SelectRing"));
        static Texture ShadowTexture = new Texture(new GfxPath(GfxType.Object, "Shadow"));

        List<WorldSpace> destinations = new List<WorldSpace>();

        protected Entity target = null;
        bool selected = false;

        float timeSinceLastAttack = 0;

        UnitData unitData;

        protected Corpse corpse;

        List<Buff> buffs;

        ParticleBase bloodsplatter;

        public virtual bool InCombat { get => inCombat; }
        bool inCombat;

        public bool OffGlobalCooldown { get => lastCastSpell + globalCooldown < TimeManager.TotalFrameTime; }
        public double RatioOfGlobalCooldownDone { get => Math.Min((TimeManager.TotalFrameTime - lastCastSpell) / globalCooldown, 1); }
        const double globalCooldown = 1500;
        double lastCastSpell;
        Spell channeledSpell;
        WorldSpace channeledSpellStartPosition;
        Entity channelTarget;
        double startCastTime;

        public Entity(Texture aTexture, WorldSpace aStartingPos, Corpse aCorpse = null) : base(aTexture, aStartingPos)
        {
            inCombat = false;
            buffs = new List<Buff>();
            corpse = aCorpse;
            shadowPos = new Rectangle((Position + new Vector2(size.X / 2, size.Y)).ToPoint(), size);
            unitData = ObjectFactory.GetData(GetType().Name);
            bloodsplatter = new ParticleBase((1000d, 2000d), ParticleBase.OpacityType.Fading, ParticleBase.ColorType.Static, new Color[] { Color.Red }, new Point(1));
        }


        public override void Update()
        {
            Walk();
            Vector2 oldPosition = Position;
            base.Update();
            CheckForCollisions(oldPosition);
            UpdateSpellChannel();
            unitData.Update();
            AttackTarget();
            UpdateBuffs();
            Death();
        }

        public bool ResourceGain(Entity aEntity, float aValue, Resource.ResourceType aResourceType)
        {
            if (aResourceType != ResourceType) return false;

            if (MaxResource == CurrentResource) return false;

            float value = aValue;

            if (MaxResource < CurrentHealth + value)
            {
                value = MaxResource - CurrentHealth;
            }

            for (int i = 0; i < aggroTablesIAmOn.Count; i++)
            {
                aggroTablesIAmOn[i].AddToAggroTable(aEntity, value);
            }

            unitData.CurrentResource += value;

            return true;
        }

        bool CastSpeedCheck()
        {
            const float graceSpeedWindow = 0.1f;
            if (momentum.ToVector2().Length() < graceSpeedWindow)
            {
                return false;
            }
            return true;
        }

        void UpdateSpellChannel()
        {
            if (channeledSpell == null) return;
            if (CastSpeedCheck())
            {
                CancelChannel();
                
                return;
            }

            if (FinishChannel()) return;

            HUDManager.UpdateChannelSpell((float)((TimeManager.TotalFrameTime - startCastTime) / channeledSpell.CastTime));
        }

        void CancelChannel()
        {
            HUDManager.CancelChannel();
            channelTarget = null;
            channeledSpell = null;
            channeledSpellStartPosition = WorldSpace.Zero;
        }

        bool FinishChannel()
        {
            if (channeledSpell.CastTime < TimeManager.TotalFrameTime - startCastTime)
            {
                const float graceWidth = 5;
                if (channeledSpell.CastDistance + graceWidth < (channelTarget.FeetPos - FeetPos).ToVector2().Length())
                {
                    CancelChannel();
                    return true;

                }

                CastSpell(channeledSpell);

                HUDManager.FinishChannel();
                channeledSpell = null;
                channelTarget = null;
                channeledSpellStartPosition = WorldSpace.Zero;
                return true;
            }
            return false;
        }

        bool StartChannel(Spell aSpell)
        {
            if (channeledSpell != null) return false;
            if (aSpell == null) return false;
            if (aSpell.CastTime == 0) return false;
            if (!aSpell.OffCooldown) return false;
            if (CastSpeedCheck()) return false;
            lastCastSpell = TimeManager.TotalFrameTime;
            channeledSpellStartPosition = FeetPos;
            channeledSpell = aSpell;
            startCastTime = TimeManager.TotalFrameTime;
            channelTarget = Target;
            if (channelTarget == null) { channelTarget = this; }
            HUDManager.ChannelSpell(channeledSpell);
            HUDManager.UpdateChannelSpell(0);
            return true;
        }

        public bool StartCast(Spell aSpell)
        {
            if (aSpell == null) return false;
            //if (!spellBook.HasSpell(aSpell)) return false;
            if (!UnitData.Resource.isCastable(aSpell.ResourceCost)) return false;
            if (!OffGlobalCooldown) return false;
            if (!aSpell.OffCooldown) return false;

            if (Target != null)
            {
                float d = (Target.FeetPos - FeetPos).ToVector2().Length();
                if (d > aSpell.CastDistance) return false;
            }


            if (aSpell.CastTime > 0)
            {
                ObjectManager.Player.StartChannel(aSpell);
                return true;
            }
            lastCastSpell = TimeManager.TotalFrameTime;
            return CastSpell(aSpell);
        }

        bool CastSpell(Spell aSpell)
        {

            if (!aSpell.Trigger(Target)) return false;
            UnitData.Resource.CastSpell(aSpell.ResourceCost);

            return true;
        }

        public void ServerTick() //"Server tick"
        {
            if (!InCombat)
            {
                unitData.HealthRegen();
            }
        }

        void UpdateBuffs()
        {
            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                if (buffs[i].IsOver)
                {
                    buffs.RemoveAt(i);
                    continue;
                }
                buffs[i].Update(this);
            }
        }

        public void AddBuff(Buff aBuff)
        {
            for (int i = 0; i < buffs.Count; i++)
            {
                if (buffs[i] == aBuff)
                {
                    buffs[i].Recast();
                    return;
                }
            }

            buffs.Add(aBuff);
            HUDManager.AddBuff(buffs.Last(), this);
        }

        public List<Buff> GetAllBuffs()
        {
            return buffs;
        }

        public void AddedToAggroTable(NonFriendly aNonfriendly)
        {
            if (aggroTablesIAmOn.Contains(aNonfriendly))
            {
                DebugManager.Print(GetType(), aNonfriendly + " tried to add me to a table I thought I was on.");
                return;
            }
            inCombat = true;
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
            Combat();
        }
        void Combat()
        {
            if (inCombat)
            {
                if (aggroTablesIAmOn.Count == 0)
                {
                    inCombat = false;
                }
            }
        }

        public void Select()
        {
            selected = true;
        }

        public void Deselect()
        {
            selected = false;
        }

        public virtual void TakeDamage(Entity aAttacker, float aDamageTaken)
        {
            unitData.CurrentHealth -= aDamageTaken;
            WorldSpace dirOfFlyingStuff = (FeetPos - aAttacker.FeetPos);
            dirOfFlyingStuff.Normalize();
            FloatingText floatingText = new FloatingText(aDamageTaken.ToString(), Color.Red, FeetPos, dirOfFlyingStuff); //TODO: Change to handle attacker and this being in the same place
            ObjectManager.SpawnFloatingText(floatingText);


            ParticleMovement bloodMovement = new ParticleMovement(dirOfFlyingStuff, WorldSpace.Zero, 0.9f);
            ParticleManager.SpawnParticle(bloodsplatter, WorldRectangle, this, bloodMovement, 10);
        }

        public virtual bool TakeHealing(Entity aHealer, float aHealingTaken)
        {
            float value = aHealingTaken;

            if (FullHealth) return false;
            if (CurrentHealth + value > MaxHealth) value = MaxHealth - CurrentHealth;

            unitData.CurrentHealth += value;
            WorldSpace ws = (FeetPos - aHealer.FeetPos);
            ws.Normalize();
            FloatingText floatingText = new FloatingText(value.ToString(), Color.White, FeetPos, ws); //TODO: Change color to green once text border has been implemented ALSO Change to handle attacker and this being in the same place
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

        WorldSpace GetDirVectorToNextDestination(WorldSpace aDestination, out float aLenghtTillDestination)
        {
            WorldSpace dirV = aDestination - FeetPos;
            aLenghtTillDestination = dirV.ToVector2().Length();
            dirV.Normalize();
            return dirV;
        }

        void Walk()
        {
            if (momentum.ToVector2().Length() < 0.1f)
            {
                momentum = WorldSpace.Zero;
            }

            if (destinations.Count == 0 && target == null)
            {
                return;
            }

            if (target != null)
            {
                OverwriteDestination(target.FeetPos);

            }

            WalkToDestination();
        }

        void WalkToDestination()
        {

            float length = 0;
            WorldSpace directionToWalk = GetDirVectorToNextDestination(destinations[0], out length);

            if (target == null)
            {
                if (length < momentum.ToVector2().Length() * 10f) //TODO: Find a good factor
                {
                    destinations.RemoveAt(0);

                    return;
                }

            }
            else
            {
                if (length < unitData.AttackRange)
                {
                    destinations.RemoveAt(0);
                    momentum = momentum / 1.6f;

                    return;
                }
            }

            velocity += directionToWalk * unitData.Speed * (float)TimeManager.SecondsSinceLastFrame;
        }

        protected void OverwriteDestination(WorldSpace aDestination)
        {
            destinations.Clear();
            destinations.Add(aDestination);
        }

        protected void AddDestination(WorldSpace aDestination) { destinations.Add(aDestination); }



        protected virtual void Death()
        {
            if (unitData.CurrentHealth <= 0)
            {
                ObjectManager.RemoveEntity(this);
                if (corpse != null) corpse.SpawnCorpe(Position); //Make this spawn a default corpse if corpse is null
            }
        }

        void AttackTarget()
        {
            if (target == null) return;

            if (unitData.SecondsPerAttack > timeSinceLastAttack)
            {
                timeSinceLastAttack += (float)TimeManager.SecondsSinceLastFrame;
                return;
            }
            AttackIfInRange();
        }

        void AttackIfInRange()
        {

            if (CheckForRelation() && (target.FeetPos - FeetPos).ToVector2().Length() < unitData.AttackRange)
            {
                timeSinceLastAttack = 0;
                target.TakeDamage(this, unitData.AttackDamage);
                if (target.unitData.CurrentHealth <= 0)
                {
                    target = null;
                }
            }
        }

        bool CheckForRelation()
        {
            if (target.Relation == UnitData.RelationToPlayer.Self && Relation == UnitData.RelationToPlayer.Friendly)
            {
                return false;
            }
            if (target.Relation != Relation)
            {
                return true;
            }

            return false;
        }

        void CheckForCollisions(Vector2 aOldPosition)
        {

            List<Rectangle> resultingCollisions = TileManager.CollisionsWithUnwalkable(WorldRectangle);

            if (resultingCollisions.Count != 0)
            {
                for (int i = 0; i < resultingCollisions.Count; i++)
                {

                    Vector2 collisionDir = Vector2.Normalize((resultingCollisions[i].Center - WorldRectangle.Center).ToVector2());

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

            Vector2 offset = new Vector2(0, size.Y / 2.5f);
            shadowPos.Location = (Position + offset).ToPoint();
            shadowPos.Size = (size.ToVector2() * Camera.Camera.Scale).ToPoint();
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch aBatch)
        {
            Color shadowColor = Color.Black;
            if (GetType() == typeof(Walker))
            {
                if (ObjectManager.Player.IsInCommand(this as Walker))
                {
                    shadowColor = Color.DarkGreen;
                }
            }
            ShadowTexture.Draw(aBatch, Camera.Camera.WorldRectToScreenRect(shadowPos).Location.ToVector2(), shadowColor, FeetPos.Y - 2);
            if (selected == true)
            {
                SelectTexture.Draw(aBatch, Camera.Camera.WorldRectToScreenRect(shadowPos).Location.ToVector2(), UnitData.RelationColor(), FeetPos.Y - 1);
            }
            base.Draw(aBatch);
        }
    }
}
