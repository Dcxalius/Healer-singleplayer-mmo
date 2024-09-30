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

namespace Project_1.GameObjects
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
        public Vector2 FeetPos { get => pos + new Vector2(size.X / 2, size.Y); }
        public Color RelationColor { get => unitData.RelationColor(); }
        public UnitData.RelationToPlayer Relation { get => unitData.Relation; }
        public Entity Target { get => target; set => target = value; }

        public string Name { get => unitData.Name; }
        public float CurrentHealth {  get => unitData.CurrentHealth;}
        public float MaxHealth {  get => unitData.MaxHealth;}


        Rectangle shadowPos;

        static Texture ShadowTexture = new Texture(new GfxPath(GfxType.Object, "Shadow"));

        Dictionary<Entity, float> aggroTable = new Dictionary<Entity, float>();
        List<Vector2> destinations = new List<Vector2>();

        protected Entity target = null;

        float timesSinceLastAttack = 0;

        UnitData unitData;

        protected Corpse corpse;

        public Entity(Texture aTexture, Vector2 aStartingPos, float aMaxSpeed) : base(aTexture, aStartingPos, aMaxSpeed)
        {
            shadowPos = new Rectangle((pos + new Vector2(size.X/2, size.Y)).ToPoint(), size);

            unitData = ObjectManager.GetData(GetType().Name);

            DebugManager.Print(GetType(), GetType().Name);
        }

        public void TakeDamage(Entity aAttacker, float aDamageTaken)
        {
            if (!aggroTable.TryAdd(aAttacker, aDamageTaken))
            {
                aggroTable[aAttacker] += aDamageTaken;
            }
            unitData.CurrentHealth -= aDamageTaken;

        }

        public override bool Click(ClickEvent aClickEvent)
        {
            if (Camera.WorldPosToCameraSpace(WorldRectangle).Contains(aClickEvent.ClickPoint))
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


            base.Update();
            AttackTarget();
            
            CheckForCollisions();

            AggroStuff();

            if (unitData.CurrentHealth <= 0)
            {
                ObjectManager.AddCorpse(corpse);
                ObjectManager.RemoveEntity(this);
            }
        }

        protected virtual void AggroStuff()
        {
            float highestThreat = 0;

            if (target != null) //TODO: Move this out to a nonfriendly class
            {
                highestThreat = aggroTable[target];
            }
            foreach (var item in aggroTable)
            {
                if (item.Value > highestThreat * 1.05)
                {
                    target = item.Key;

                }
            }
        }

        void AttackTarget()
        {
            if (target == null) return;

            if (unitData.SecondsPerAttack > timesSinceLastAttack)
            {
                timesSinceLastAttack += (float)TimeManager.SecondsSinceLastFrame;
                return;     
            }

            if (CheckForRelation() && (target.FeetPos - FeetPos).Length() < this.unitData.AttackRange)
            {
                timesSinceLastAttack = 0;
                target.TakeDamage(this, unitData.AttackDamage);
                if (target.unitData.CurrentHealth <= 0)
                {
                    target = null;
                }
            }
        }

        bool CheckForRelation()
        {
            if (target.Relation == UnitData.RelationToPlayer.Self && this.Relation == UnitData.RelationToPlayer.Friendly )
            {
                return false;
            }
            if (target.Relation != this.Relation)
            {
                return true;
            }

            return false;
        }

        void CheckForCollisions()
        {
            Vector2 oldPosition = pos;

            List<Rectangle> resultingCollisions = TileManager.CollisionsWithUnwalkable(WorldRectangle);

            if (resultingCollisions.Count != 0)
            {
                for (int i = 0; i < resultingCollisions.Count; i++)
                {

                    Vector2 collisionDir = Vector2.Normalize((resultingCollisions[i].Center - WorldRectangle.Center).ToVector2());

                    if (Math.Abs(collisionDir.X) > Math.Abs(collisionDir.Y))
                    {
                        velocity.X = 0;
                        momentum.X = 0;//TOOD: Has it so if momentum changes rapidly it deals damage? Maybe i dunnu I dont have my gamedesigner hat with me today
                        pos.X = oldPosition.X;
                    }
                    if (Math.Abs(collisionDir.X) < Math.Abs(collisionDir.Y))
                    {
                        velocity.Y = 0;
                        momentum.Y = 0;
                        pos.Y = oldPosition.Y;
                    }
                }
                //else
                //{
                //    velocity = Vector2.Zero;
                //    momentum = Vector2.Zero;
                //    pos = oldPosition;
                //}
            }

            Vector2 offset = new Vector2(0, size.Y / 2.5f);
            shadowPos.Location = (pos + offset).ToPoint();
            shadowPos.Size = (size.ToVector2() * Camera.Scale).ToPoint();
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch aBatch)
        {


            ShadowTexture.Draw(aBatch, Camera.WorldPosToCameraSpace(shadowPos));
            
            base.Draw(aBatch);

            
        }
    }
}
