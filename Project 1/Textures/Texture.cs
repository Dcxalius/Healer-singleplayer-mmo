using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Textures
{
    internal class Texture
    {
        protected Texture2D gfx;
        public Point size;
        public Color color;
        public float scale;
        protected Rectangle? visible;
        protected Vector2 offset;
        SpriteEffects flip;

        public Texture(GfxPath aPath)
        {
            __Constructor__(aPath);
            color = Color.White;   
            size = gfx.Bounds.Size;
            scale = 1;
        }

        public Texture(GfxPath aPath, Vector2 aOffset)
        {
            __Constructor__(aPath);
            color = Color.White;
            size = gfx.Bounds.Size;
            scale = 1;
            offset = aOffset;
        }

        public Texture(GfxPath aPath, Point aSize)
        {
            __Constructor__(aPath);
            color = Color.White;   
            scale = 1;
            size = aSize;
        }

        public Texture(GfxPath aPath, Vector2 aOffset, Point aSize)
        {
            __Constructor__(aPath);
            color = Color.White;
            scale = 1;
            size = aSize;
            offset = aOffset;
        }


        public Texture(GfxPath aPath,  Color aColor) 
        {
            __Constructor__(aPath);
            scale = 1;
            color = aColor;
        }

        public Texture(GfxPath aPath, Vector2 aOffset, Color aColor)
        {
            __Constructor__(aPath);
            scale = 1;
            color = aColor;
            offset = aOffset;
        }


        void __Constructor__(GfxPath aPath)
        {
            gfx = GraphicsManager.GetTexture(aPath);
            flip = SpriteEffects.None;
            visible = null;
        }

        public void Flip()
        {
            if (flip == SpriteEffects.None)
            {
                flip = SpriteEffects.FlipHorizontally;
                return;
            }

            flip = SpriteEffects.None;
        }

        public virtual void Update()
        {

        }

        public virtual void Draw(SpriteBatch aBatch, Vector2 aPos)
        {
            Draw(aBatch, aPos, color);
        }

        public virtual void Draw(SpriteBatch aBatch, Rectangle aPosRectangle)
        {
            Draw(aBatch, aPosRectangle, color);
        }

        public virtual void Draw(SpriteBatch aBatch, Vector2 aPos, Color aColor)
        {
            Debug.Assert(gfx != null);

            aBatch.Draw(gfx, aPos, visible, aColor, 0f, offset, Camera.Scale, flip, 1f);
        }
        public virtual void Draw(SpriteBatch aBatch, Rectangle aPosRectangle, Color aColor)
        {
            Debug.Assert(gfx != null);

            aBatch.Draw(gfx, aPosRectangle, visible, aColor, 0f, offset, flip, 1f);
        }
    }
}
