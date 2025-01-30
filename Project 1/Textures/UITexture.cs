using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Textures
{
    //[DebuggerStepThrough]
    internal class UITexture : Texture
    {
        public enum Layer
        {
            Base,
            Window,
            Menu, 
            Count
        }
        readonly Color defaultColor = Color.White;


        public static UITexture Null => new UITexture(GfxPath.NullPath, Color.White);

        public float layer;

        public UITexture(GfxPath aPath, Color aColor) : base(aPath, aColor) { }

        public UITexture(string aPath, Color aColor) : base( new GfxPath(GfxType.UI, aPath), aColor)
        {

        }


        public virtual void Draw(SpriteBatch aBatch, Rectangle aPosRectangle)
        {
            Draw(aBatch, aPosRectangle, Color);
        }

        public virtual void Draw(SpriteBatch aBatch, Rectangle aPosRectangle, Color aColor)
        {
            if (gfx == null)
            {
                return;
            }
            if (Camera.Camera.MomAmIInFrame(aPosRectangle))
            {
                aBatch.Draw(gfx, aPosRectangle, Visible, aColor, Rotation, offset, flip, 1f);

            }
        }
    }
}
