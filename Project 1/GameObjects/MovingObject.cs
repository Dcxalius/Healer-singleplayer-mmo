using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects
{
    internal abstract class MovingObject : GameObject
    {
        public enum FacingDirection
        {
            Right,
            Left
        }

        public WorldSpace Velocity { get => velocity; protected set => velocity = value; }
        public WorldSpace Momentum { get => momentum; protected set => velocity = momentum; }
        public abstract float MaxSpeed { get; }

        FacingDirection facing;

        protected WorldSpace momentum = WorldSpace.Zero;
        protected WorldSpace velocity = WorldSpace.Zero;
        //float maxSpeed;



        public MovingObject(Texture aTexture, WorldSpace aStartingPos) : base(aTexture, aStartingPos)
        {
            facing = FacingDirection.Right;
        }

        public override void Update()
        {
            SetMomentum();
            ChangePosition();
            FlipGfx();

            base.Update();
        }

        protected virtual void SetMomentum()
        {
            momentum += velocity;
            if (momentum.ToVector2().Length() > MaxSpeed)
            {
                momentum = (WorldSpace)Vector2.Normalize(momentum) * MaxSpeed;
            }
            velocity = WorldSpace.Zero;
            momentum = new WorldSpace(momentum.X * TileManager.GetDragCoeficient(FeetPosition), momentum.Y * TileManager.GetDragCoeficient(FeetPosition));
        }

        void ChangePosition()
        {
            Position += momentum;
        }

        protected virtual void FlipGfx()
        {
            if (momentum.X > 0 && facing == FacingDirection.Left)
            {
                facing = FacingDirection.Right;
                gfx.Flip();
            }

            if (momentum.X < 0 && facing == FacingDirection.Right)
            {
                facing = FacingDirection.Left;
                gfx.Flip();
            }
        }
    }
}
