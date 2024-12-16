using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
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
        protected Texture2D gfx;
        
        //TODO: public WorldSpace Size { get => size; }
        public Point ScaledSize { get => (size.ToVector2() * Camera.Camera.Scale).ToPoint(); }
        public Point size;

        public Rectangle? Visible { get => visible; protected set => visible = value; }
        Rectangle? visible;

        public Color Color { get => color; set => color = value; }
        Color color;

        public float Rotation { get =>  rotation; set => rotation = value; }
        float rotation = 0f;

        public Vector2 Offset { get => offset; set => offset = value; }

        protected Vector2 offset;
        protected SpriteEffects flip;

        public Texture(Texture2D aGfx, Point aSize)
        {
            gfx = aGfx;
            size = aSize;
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
        }

        public Texture(GfxPath aPath, Vector2 aOffset)
        {
            __Constructor__(aPath);
            color = Color.White;
            size = gfx.Bounds.Size;
            offset = aOffset;
        }

        public Texture(GfxPath aPath, Point aSize)
        {
            __Constructor__(aPath);
            color = Color.White;   
            size = aSize;
            offset = Vector2.Zero;
        }

        public Texture(GfxPath aPath, Vector2 aOffset, Point aSize)
        {
            __Constructor__(aPath);
            color = Color.White;
            size = aSize;
            offset = aOffset;
        }


        public Texture(GfxPath aPath,  Color aColor) 
        {
            __Constructor__(aPath);
            color = aColor;
            offset = Vector2.Zero;
            if (gfx == null) return;
            size = gfx.Bounds.Size;
        }

        public Texture(GfxPath aPath, Vector2 aOffset, Color aColor)
        {
            __Constructor__(aPath);
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
            Draw(aBatch, aPos, color, Camera.Camera.WorldRectangle.Bottom);
        }

        public virtual void Draw(SpriteBatch aBatch, Vector2 aPos, float aFeetPosY)
        {
            Draw(aBatch, aPos, color, aFeetPosY);
        }

        public virtual void Draw(SpriteBatch aBatch, Vector2 aPos, Color aColor, float aFeetPosY)
        {
            if (gfx == null) return;

            if (Camera.Camera.MomAmIInFrame(new Rectangle(aPos.ToPoint(), (size.ToVector2() * Camera.Camera.Scale).ToPoint())))
            {
                //aBatch.Draw(gfx, aPos, visible, aColor, rotation, offset, Camera.Camera.Scale, flip, aFeetPosY / (Camera.Camera.WorldRectangle.Bottom + size.Y));
                aBatch.Draw(gfx, new Rectangle(aPos.ToPoint(),ScaledSize), visible, aColor, rotation, offset, flip, aFeetPosY / (Camera.Camera.WorldRectangle.Bottom + size.Y));
            }
        }
    }
}
