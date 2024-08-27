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

        public Texture(GfxPath aPath)
        {
            __Constructor__(aPath);
            color = Color.White;   
            size = gfx.Bounds.Size;
            scale = 1;
        }

        public Texture(GfxPath aPath, Point aSize)
        {
            __Constructor__(aPath);
            color = Color.White;   
            scale = 1;
            size = aSize;
        }

        public Texture(GfxPath aPath,  Color aColor) 
        {
            __Constructor__(aPath);
            scale = 1;
            color = aColor;
        }

        public Texture(GfxPath aPath, Point aSize, Color aColor, float aScale)
        {
            __Constructor__(aPath);
            color = aColor;
            size = aSize;
            scale = aScale; 
        }

        void __Constructor__(GfxPath path)
        {
            gfx = GraphicsManager.GetTexture(path);

        }

        public virtual void Update()
        {

        }

        public virtual void Draw(SpriteBatch aBatch, Vector2 pos)
        {
            Debug.Assert(gfx != null);

            aBatch.Draw(gfx, pos, null ,color, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
        }

        public virtual void Draw(SpriteBatch aBatch, Rectangle rectangle)
        {
            Debug.Assert(gfx != null);
            {
                aBatch.Draw(gfx, rectangle, null, color, 0f, Vector2.Zero, SpriteEffects.None, 1f);

            }
        }
    }
}
