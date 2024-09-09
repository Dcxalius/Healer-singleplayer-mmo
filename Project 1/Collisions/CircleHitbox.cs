using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Collisions
{
    internal class CircleHitbox : Hitbox
    {
        public CircleHitbox(Rectangle aCollider) : base(aCollider, HitboxType.circle)
        {
        }

        protected override bool CheckCollision(Hitbox aCollidesWith)
        {
            bool collision = false;

            switch (aCollidesWith.type)
            {
                case HitboxType.circle:
                    collision = CheckVsCircleCollision((CircleHitbox)aCollidesWith);
                    break;
                case HitboxType.ellipse:
                    break;
                case HitboxType.rectangle:
                    break;
                default:
                    break;
            }

            return collision;
        }

        public static bool CircleVsCircleCollision(CircleHitbox aCircleCollider, CircleHitbox aCollidesWith)
        {
            Vector2 circleColliderCentre = aCircleCollider.collider.Location.ToVector2() + aCircleCollider.collider.Size.ToVector2() / 2;
            Vector2 circleCollidesWithCentre = aCollidesWith.collider.Location.ToVector2() + aCollidesWith.collider.Size.ToVector2() / 2;


            Vector2 norm = circleColliderCentre - circleCollidesWithCentre;
            norm.Normalize();

            float colliderLength = circleColliderCentre.X - aCircleCollider.collider.Location.X;
            float collidesWithLength = circleCollidesWithCentre.X - aCollidesWith.collider.Location.X;


            return (circleColliderCentre + norm * colliderLength - circleCollidesWithCentre).Length() < collidesWithLength;
        }

        protected override bool CheckVsEllipseCollision(EllipseHitbox aHitbox)
        {
            throw new NotImplementedException();
        }

        protected override bool CheckVsCircleCollision(CircleHitbox aHitbox)
        {

            return CircleVsCircleCollision(this, aHitbox);
        }

        protected override bool CheckVsRectangleCollision()
        {
            throw new NotImplementedException();
        }
    }
}
