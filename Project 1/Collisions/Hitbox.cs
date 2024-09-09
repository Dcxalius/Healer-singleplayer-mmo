using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Collisions
{
    internal abstract class Hitbox
    {
        public enum HitboxType
        {
            circle,
            ellipse,
            rectangle
        }
        static Texture2D circleDebug = GraphicsManager.GetTexture(new GfxPath(GfxType.Debug, "DebugCircle"));
        static Texture2D rectangleDebug = GraphicsManager.GetTexture(new GfxPath(GfxType.Debug, "Debug"));

        public HitboxType type;

        public Rectangle Collider
        {
            get => collider;
        }

        protected Rectangle collider;
        public Hitbox(Rectangle aCollider, HitboxType aType)
        {
            collider = aCollider;
            type = aType;
        }

        static public bool CheckCollision(Hitbox aSender, Hitbox aCollidesWith)
        {
            return aSender.CheckCollision(aCollidesWith);
        }

        protected abstract bool CheckCollision(Hitbox aCollidesWith);

        protected abstract bool CheckVsEllipseCollision(EllipseHitbox aHitbox);

        protected abstract bool CheckVsCircleCollision(CircleHitbox aHitbox);
        protected abstract bool CheckVsRectangleCollision();

        public void Draw(SpriteBatch aBatch)
        {
            switch (type)
            {
                case HitboxType.circle:
                case HitboxType.ellipse:
                    aBatch.Draw(circleDebug, collider, Color.White);
                    break;
                case HitboxType.rectangle:
                    aBatch.Draw(rectangleDebug, collider, Color.White);
                    break;
                default:
                    break;
            }
        }
    }
}
