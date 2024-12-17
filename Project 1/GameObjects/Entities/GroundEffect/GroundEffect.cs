using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities.GroundEffect
{
    internal abstract class GroundEffect
    {
        protected Textures.Texture gfx;
        protected Rectangle Position => position;
        Rectangle position;

        protected GroundEffect(GfxPath gfxPath)
        {
            gfx = new Textures.Texture(gfxPath);
        }

        public void UpdatePosition(WorldSpace aPos, Point aSize)
        {
            Vector2 offset = new Vector2(0, aSize.Y / 2.5f);

            position.Location = (aPos + offset).ToPoint();
            position.Size = (aSize.ToVector2() * Camera.Camera.Scale).ToPoint();
        }

        protected void Draw(SpriteBatch aBatch, Entity aOwner, Color aColor, float aOrder)
        {
            gfx.Draw(aBatch, Camera.Camera.WorldRectToScreenRect(Position).Location.ToVector2(), aColor, aOwner.FeetPosition.Y - aOrder); //TODO: Make this actually use rects to draw

        }
    }
}
