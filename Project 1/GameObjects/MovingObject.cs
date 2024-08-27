using Microsoft.Xna.Framework;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1
{
    internal abstract class MovingObject : GameObject
    {
        public Vector2 Velocity { get; }

        Vector2 momentum = Vector2.Zero;
        protected Vector2 velocity = Vector2.Zero;
        float maxSpeed;
        Vector2 drag = new Vector2(0.9f);

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
            pos += momentum;
            momentum = new Vector2(momentum.X * drag.X, momentum.Y * drag.Y);
        
            base.Update();
        }
    }
}
