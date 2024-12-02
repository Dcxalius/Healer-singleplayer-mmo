using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Textures
{
    internal class UITexture : Texture
    {
        readonly Color defaultColor = Color.White;

        public static UITexture Null => new UITexture(GfxPath.NullPath, Color.White);

        public UITexture(Texture2D aText, Point aSize) : base(aText, aSize)
        {

        }

        public UITexture(GfxPath aPath, Color aColor) : base(aPath, aColor) { }

        public UITexture(string aPath, Color aColor) : base( new GfxPath(GfxType.UI, aPath), aColor)
        {

        }


        public virtual void Draw(SpriteBatch aBatch, Rectangle aPosRectangle)
        {
            Draw(aBatch, aPosRectangle, color);
        }

        public virtual void Draw(SpriteBatch aBatch, Rectangle aPosRectangle, Color aColor)
        {
            if (gfx == null)
            {
                return;
            }
            if (Camera.Camera.MomAmIInFrame(aPosRectangle))
            {
                aBatch.Draw(gfx, aPosRectangle, visible, aColor, rotation, offset, flip, 1f);

            }
        }
    }
}
