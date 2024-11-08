using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using Project_1.Tiles;
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
        public Rectangle? Visible { get => visible; }

        public Color Color { get => color; set => color = value; }

        protected Texture2D gfx;
        public Point size;
        protected Color color;
        public float scale;
        protected Rectangle? visible;
        protected Vector2 offset;
        protected float rotation = 0f;
        protected SpriteEffects flip;

        public Texture(Texture2D aGfx, Point aSize)
        {
            gfx = aGfx;
            size = aSize;
            scale = 1;
            color = Color.White;
            flip = SpriteEffects.None;
            visible = null;
            offset = Vector2.Zero;
        }

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
            offset = Vector2.Zero;
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
            offset = Vector2.Zero;
            if (gfx == null) return;
            size = gfx.Bounds.Size;
        }

        public Texture(GfxPath aPath, Vector2 aOffset, Color aColor)
        {
            __Constructor__(aPath);
            scale = 1;
            color = aColor;
            offset = aOffset;
            if (gfx == null) return;
            size = gfx.Bounds.Size;
        }


        void __Constructor__(GfxPath aPath)
        {
            
            flip = SpriteEffects.None;
            visible = null;
            offset = Vector2.Zero;
            if (aPath == null) return;
            if (aPath.Name == null) return;
            gfx = TextureManager.GetTexture(aPath);
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

        public virtual void Update() {}

        public virtual void Draw(SpriteBatch aBatch, Vector2 aPos)
        {
            Draw(aBatch, aPos, color, Camera.WorldRectangle.Bottom);
        }

        public virtual void Draw(SpriteBatch aBatch, Vector2 aPos, float aFeetPosY)
        {
            Draw(aBatch, aPos, color, aFeetPosY);
        }

        public virtual void Draw(SpriteBatch aBatch, Vector2 aPos, Color aColor, float aFeetPosY)
        {
            if (gfx == null) return;

            if (Camera.MomAmIInFrame(new Rectangle(aPos.ToPoint(), (size.ToVector2() *Camera.Scale).ToPoint())))
            {
                aBatch.Draw(gfx, aPos, visible, aColor, rotation, offset, Camera.Scale, flip, aFeetPosY / (Camera.WorldRectangle.Bottom + size.Y));
            }
        }
    }
}
