using Microsoft.Xna.Framework;
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
        public Vector2 Velocity { get => velocity; protected set => velocity = value; }
        public Vector2 Momentum { get => momentum; protected set => velocity = momentum; }


        bool facingRight = true;

        protected Vector2 momentum = Vector2.Zero;
        protected Vector2 velocity = Vector2.Zero;
        float maxSpeed;
        //Vector2 drag = new Vector2(0.9f);

        public MovingObject(Texture aTexture, Vector2 aStartingPos, float aMaxSpeed) : base(aTexture, aStartingPos)
        {
            maxSpeed = aMaxSpeed;
        }

        public override void Update()
        {
            momentum += velocity;
            if (momentum.Length() > maxSpeed)
            {
                momentum = Vector2.Normalize(momentum) * maxSpeed;
            }

            velocity = Vector2.Zero;
            Position += momentum;
            momentum = new Vector2(momentum.X * TileManager.GetDragCoeficient(Centre), momentum.Y * TileManager.GetDragCoeficient(Centre));

            if (momentum.X > 0 && facingRight == false)
            {
                facingRight = true;
                gfx.Flip();
            }

            if (momentum.X < 0 && facingRight == true)
            {
                facingRight = false;
                gfx.Flip();
            }

            base.Update();
        }
    }
}
