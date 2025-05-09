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

        protected GroundEffect(GfxPath gfxPath)
        {
            gfx = new Textures.Texture(gfxPath);
        }
        protected void Draw(SpriteBatch aBatch, Rectangle aDrawRect, Color aColor, float aOrder)
        {
            //gfx.Draw(aBatch, aFeetPos.ToAbsoltueScreenPosition(), aColor, gfx.size.ToVector2() / 2, aFeetPos.Y - aOrder); //TODO: Make this actually use rects to draw
            gfx.Draw(aBatch, aDrawRect, aColor, new Vector2(0, -gfx.size.Y / 2), aDrawRect.Bottom - aOrder);

        }

        public abstract void Draw(SpriteBatch aBatch, WorldObject aOwner);
    }
}
