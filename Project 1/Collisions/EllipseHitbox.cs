using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Collisions
{
    internal class EllipseHitbox : Hitbox
    {
        public EllipseHitbox(Rectangle aCollider) : base(aCollider, HitboxType.ellipse)
        {

        }
        protected override bool CheckCollision(Hitbox aCollidesWith)
        {
            bool col = false;
            switch (aCollidesWith.type)
            {
                case HitboxType.circle:
                    col = CheckVsCircleCollision((CircleHitbox)aCollidesWith);
                    break;
                case HitboxType.ellipse:
                    col = CheckVsEllipseCollision((EllipseHitbox)aCollidesWith);
                    break;
                case HitboxType.rectangle:
                    break;
                default:
                    break;
            }

            return true;
        }

        protected override bool CheckVsCircleCollision(CircleHitbox aHitbox)
        {
            return EllipseVsCircleCollision(this, aHitbox);
        }

        protected override bool CheckVsEllipseCollision(EllipseHitbox aHitbox)
        {
            return EllipseVsEllipseCollision(this, aHitbox);
        }

        static bool EllipseVsCircleCollision(EllipseHitbox aEllipseCollider, CircleHitbox aCircleCollidesWith)
        {
            Vector2 collideWithCentre = aCircleCollidesWith.Collider.Location.ToVector2() + aCircleCollidesWith.Collider.Size.ToVector2() / 2f;

            Vector2 norm = aEllipseCollider.collider.Location.ToVector2() + aEllipseCollider.collider.Size.ToVector2() / 2f - collideWithCentre;
            norm.Normalize();

            double degreeBetweenColliderCentres = Math.Atan2(norm.Y, norm.X);

            Vector2 colliderClosestBorder = EllipseBorderFromAngle(aEllipseCollider.collider, degreeBetweenColliderCentres);
            float distFromCircle = aCircleCollidesWith.Collider.Size.X / 2f;

            return (colliderClosestBorder - collideWithCentre).Length() < distFromCircle;
        }

        public static bool EllipseVsEllipseCollision(EllipseHitbox aEllipseCollider, EllipseHitbox aEllipseToCollideWith)
        {
            Vector2 collideWithCentre = aEllipseToCollideWith.collider.Location.ToVector2() + aEllipseToCollideWith.collider.Size.ToVector2() / 2f;

            Vector2 norm = aEllipseCollider.collider.Location.ToVector2() + aEllipseCollider.collider.Size.ToVector2() / 2f - collideWithCentre;
            norm.Normalize();

            double degreeBetweenEllipses = Math.Atan2(norm.Y, norm.X);

            Vector2 colliderClosestBorder = EllipseBorderFromAngle(aEllipseCollider.collider, degreeBetweenEllipses);
            Vector2 collideWithBorder = EllipseBorderFromAngle(aEllipseToCollideWith.collider, degreeBetweenEllipses * -1);

            return (colliderClosestBorder - collideWithCentre).Length() < (collideWithBorder - collideWithCentre).Length();
        }

        static Vector2 EllipseBorderFromAngle(Rectangle aEllipse, double aDegrees)
        {

            double a = aEllipse.Size.X / 2;
            double b = aEllipse.Size.Y / 2;



            double x = a * b * Math.Cos(aDegrees) / Math.Sqrt(Math.Pow(b * Math.Cos(aDegrees), 2) + Math.Pow(a * Math.Sin(aDegrees), 2));
            double y = a * b * Math.Sin(aDegrees) / Math.Sqrt(Math.Pow(b * Math.Cos(aDegrees), 2) + Math.Pow(a * Math.Sin(aDegrees), 2));

            return new Vector2((float)x, (float)y);
        }

        protected override bool CheckVsRectangleCollision()
        {
            throw new NotImplementedException();
        }
    }
}
