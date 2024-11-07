using Microsoft.Xna.Framework;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.Tiles;
using Project_1.UI.HUD;
using SharpDX.Direct2D1.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
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

        protected UnitData Data { get => unitData; }
        public bool HasDestination { get => destinations.Count > 0; }
        public Color RelationColor { get => unitData.RelationColor(); }
        public UnitData.RelationToPlayer Relation { get => unitData.Relation; }
        public Entity Target { get => target; set => target = value; }

        public string Name { get => unitData.Name; }
        public bool Alive { get => unitData.CurrentHealth > 0; }
        public float CurrentHealth { get => unitData.CurrentHealth; }
        public float MaxHealth { get => unitData.MaxHealth; }

        public override float MaxSpeed { get => unitData.MaxSpeed; }

        Rectangle shadowPos;

        List<NonFriendly> aggroTablesIAmOn = new List<NonFriendly>();

        static Texture SelectTexture = new Texture(new GfxPath(GfxType.Object, "SelectRing"));
        static Texture ShadowTexture = new Texture(new GfxPath(GfxType.Object, "Shadow"));

        List<Vector2> destinations = new List<Vector2>();

        protected Entity target = null;
        bool selected = false;

        float timeSinceLastAttack = 0;

        UnitData unitData;

        protected Corpse corpse;

        public Entity(Texture aTexture, Vector2 aStartingPos, Corpse aCorpse = null) : base(aTexture, aStartingPos)
        {
            corpse = aCorpse;
            shadowPos = new Rectangle((Position + new Vector2(size.X / 2, size.Y)).ToPoint(), size);
            unitData = ObjectManager.GetData(GetType().Name);
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
            FloatingText floatingText = new FloatingText(aDamageTaken.ToString(), Color.Red, FeetPos, Vector2.Normalize(FeetPos - aAttacker.FeetPos));
            ObjectManager.SpawnFloatingText(floatingText);
        }



        public override bool Click(ClickEvent aClickEvent)
        {
            if (Camera.WorldPosToCameraSpace(WorldRectangle).Contains(aClickEvent.AbsolutePos))
            {
                if (aClickEvent.NoModifiers())
                {
                    HUDManager.SetNewTarget(this);
                }
                ClickedOn(aClickEvent);

                return true;
            }
            return false;
        }

        protected virtual void ClickedOn(ClickEvent aClickEvent)
        {
            if (aClickEvent.NoModifiers() && aClickEvent.ButtonPressed == ClickEvent.ClickType.Right)
            {
                ObjectManager.Player.IssueTargetOrder(this);
            }
        }

        Vector2 GetDirVectorToNextDestination(Vector2 aDestination, out float aLenghtTillDestination)
        {
            Vector2 dirV = aDestination - FeetPos;
            aLenghtTillDestination = dirV.Length();
            dirV.Normalize();
            return dirV;
        }

        void Walk()
        {
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
            Vector2 directionToWalk = GetDirVectorToNextDestination(destinations[0], out length);

            if (target == null)
            {
                if (length < momentum.Length() * 10f) //TODO: Find a good factor
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

        protected void OverwriteDestination(Vector2 aDestination)
        {
            destinations.Clear();
            destinations.Add(aDestination);
        }

        protected void AddDestination(Vector2 aDestination) { destinations.Add(aDestination); }

        public override void Update()
        {
            Walk();
            Vector2 oldPosition = Position;


            base.Update();
            AttackTarget();

            CheckForCollisions(oldPosition);


            Death();
        }



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

            if (CheckForRelation() && (target.FeetPos - FeetPos).Length() < unitData.AttackRange)
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
                        Position = new Vector2(aOldPosition.X, Position.Y);
                    }
                    if (Math.Abs(collisionDir.X) < Math.Abs(collisionDir.Y))
                    {
                        velocity.Y = 0;
                        momentum.Y = 0;
                        Position = new Vector2(Position.X, aOldPosition.Y);
                    }
                }
            }

            Vector2 offset = new Vector2(0, size.Y / 2.5f);
            shadowPos.Location = (Position + offset).ToPoint();
            shadowPos.Size = (size.ToVector2() * Camera.Scale).ToPoint();
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
            ShadowTexture.Draw(aBatch, Camera.WorldPosToCameraSpace(shadowPos).Location.ToVector2(), shadowColor, FeetPos.Y - 2);
            if (selected == true)
            {
                SelectTexture.Draw(aBatch, Camera.WorldPosToCameraSpace(shadowPos).Location.ToVector2(), Data.RelationColor(), FeetPos.Y - 1);
            }
            base.Draw(aBatch);
        }
    }
}
