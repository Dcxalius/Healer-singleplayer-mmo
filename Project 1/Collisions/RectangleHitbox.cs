using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Collisions
{
    internal class RectangleHitbox : Hitbox
    {
        public RectangleHitbox(Rectangle aCollider) : base(aCollider, HitboxType.rectangle)
        {
        }

        protected override bool CheckCollision(Hitbox aCollidesWith)
        {
            throw new NotImplementedException();
        }

        protected override bool CheckVsCircleCollision(CircleHitbox aHitbox)
        {
            throw new NotImplementedException();
        }

        protected override bool CheckVsEllipseCollision(EllipseHitbox aHitbox)
        {
            throw new NotImplementedException();
        }

        protected override bool CheckVsRectangleCollision()
        {
            throw new NotImplementedException();
        }
    }
}
