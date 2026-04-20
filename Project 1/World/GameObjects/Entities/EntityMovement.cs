using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Spells.Buff;
using Project_1.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities
{
    internal partial class Entity
    {

        public override WorldSpace FeetSize { get => new WorldSpace(Size.X, Size.Y / 2); }
        public override Rectangle WorldRectangle
        {
            get
            {
                Point pos = FeetPosition.ToPoint() - new Point((int)(FeetSize.X / 2), (int)(FeetSize.Y / 2));
                return new Rectangle(pos, FeetSize.ToPoint());
            }
        }
        public bool HasControl
        {
            get
            {
                Buff[] buffs = buffList.GetAllBuffs().ToArray();
                for (int i = 0; i < buffs.Length; i++)
                {
                    if (buffs[i].EffectId == 1)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public float Speed => unitData.MovementData.Speed * CalculateMovementSpeedMultiplier();

        public float CalculateMovementSpeedMultiplier()
        {
            double biggestSlow = 1;
            double msBuffs = 1;
            Buff[] buffs = buffList.GetAllBuffs().ToArray();
            for (int i = 0; i < buffs.Length; i++)
            {
                if (buffs[i].MovementSpeedModifier < biggestSlow)
                {
                    biggestSlow = buffs[i].MovementSpeedModifier;
                }
                msBuffs *= buffs[i].MovementSpeedModifier;
            }

            return 1 * (float)biggestSlow * (float)msBuffs;
        }

        public void Movement()
        {
            Destination.Update();

            float minAttackRange = GetMinAttackRange();

            velocity += Destination.GetVelocity(minAttackRange, Speed, new WorldSpace(FeetSize));
            base.Update(); //TODO: This shouldnt be here
            CheckForCollisions();

            unitData.Position = FeetPosition;
            unitData.Momentum = momentum;
            unitData.Velocity = velocity;
        }

        void CheckForCollisions() 
        {

            List<(Rectangle, Rectangle)> resultingCollisions = TileManager.CollisionManager.CollisionsWithUnwalkable(this);

            if (resultingCollisions.Count != 0)
            {
                for (int i = 0; i < resultingCollisions.Count; i++)
                {
                    //TODO: Ponder how to make it not jump
                    //Related to the fact that when colliding with corners it uses feetpos rather than the border of feet

                    //if (WorldRectangle.Right > resultingCollisions[i].Item2.Left)
                    //{
                    //    FeetPosition = new WorldSpace(resultingCollisions[i].Item2.Location.X - FeetSize.X / 2, FeetPosition.Y);

                    //    velocity.X = 0;
                    //    momentum.X = 0;
                    //}

                    if (FeetPosition.X - resultingCollisions[i].Item1.Left < 0)
                    {
                        FeetPosition = new WorldSpace(resultingCollisions[i].Item2.Location.X - FeetSize.X / 2, FeetPosition.Y);

                        velocity.X = 0;
                        momentum.X = 0;
                    }
                    if (FeetPosition.X - resultingCollisions[i].Item1.Right > 0)
                    {
                        FeetPosition = new WorldSpace(resultingCollisions[i].Item2.Location.X + resultingCollisions[i].Item2.Width + FeetSize.X / 2, FeetPosition.Y);

                        velocity.X = 0;
                        momentum.X = 0;

                    }
                    if (FeetPosition.Y - resultingCollisions[i].Item1.Top < 0)
                    {
                        FeetPosition = new WorldSpace(FeetPosition.X, MathF.Round(FeetPosition.Y)) - new WorldSpace(0, resultingCollisions[i].Item1.Height);

                        velocity.Y = 0;
                        momentum.Y = 0;

                    }
                    if (FeetPosition.Y - resultingCollisions[i].Item1.Bottom > 0)
                    {
                        FeetPosition = new WorldSpace(FeetPosition.X, MathF.Round(FeetPosition.Y)) + new WorldSpace(0, resultingCollisions[i].Item1.Height);

                        velocity.Y = 0;
                        momentum.Y = 0;
                    }
                    //Debug.Assert(hit, "How did we get here?");
                }
            }

        }
    }
}
