using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Managers;
using Project_1.Tiles;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
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
        //public Point ScaledSize { get => (size.ToVector2() * Camera.Camera.Scale).ToPoint(); }
        public Point ScaledSize { get => new Point((int)Math.Ceiling(size.X * Camera.Camera.Scale), (int)Math.Ceiling(size.Y * Camera.Camera.Scale)); } //TODO: Find out wtf is wrong with this.
        public Point size;

        public Rectangle? Visible { get => visible; protected set => visible = value; }
        Rectangle? visible;

        public Color Color { get => color; set => color = value; }
        Color color;

        public float Rotation { get =>  rotation; set => rotation = value; }
        float rotation = 0f;

        public Vector2 Offset { get => offset; set => offset = value; }

        public static Color AvgColor(GfxPath aPath) //TODO: Move this?
        {
            Texture2D gfx = TextureManager.GetTexture(aPath);
            
            Point bounds = gfx.Bounds.Size;
            Color[] c = new Color[bounds.X * bounds.Y];
            gfx.GetData(c);
            Color c2 = c[0];
            for (int i = 1; i < c.Length; i++)
            {
                c2.R = (byte)((c2.R + c[i].R) / 2);
                c2.G = (byte)((c2.G + c[i].G) / 2);
                c2.B = (byte)((c2.B + c[i].B) / 2);
            }
            c2.A = 255;
            return c2;
            
        }

        protected Vector2 offset;
        protected SpriteEffects flip;
        public Texture(GfxPath aPath) : this(aPath, Vector2.Zero, Point.Zero, Color.White) { }
        public Texture(GfxPath aPath, Vector2 aOffset) : this(aPath, aOffset, Point.Zero, Color.White) { }
        public Texture(GfxPath aPath, Point aSize) : this(aPath, Vector2.Zero, aSize, Color.White) { }
        public Texture(GfxPath aPath, Vector2 aOffset, Point aSize) : this(aPath, aOffset, aSize, Color.White) { }
        public Texture(GfxPath aPath,Color aColor) : this(aPath, Vector2.Zero, Point.Zero, aColor) { }
        public Texture(GfxPath aPath, Vector2 aOffset, Color aColor) : this(aPath, aOffset, Point.Zero, aColor) { }
        public Texture(GfxPath aPath, Vector2 aOffset, Point aSize, Color aColor)
        {
            flip = SpriteEffects.None;
            visible = null;


            color = aColor;
            offset = aOffset;


            if (aPath == null) return;
            if (aPath.Name == null) return;
            gfx = TextureManager.GetTexture(aPath);
            if (aSize == Point.Zero) { size = gfx.Bounds.Size; }
            else { size = aSize; }
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

        public virtual void Draw(SpriteBatch aBatch, Vector2 aPos) => Draw(aBatch, aPos, color, offset, Camera.Camera.WorldRectangle.Top);

        public virtual void Draw(SpriteBatch aBatch, Vector2 aPos, Color aColor) => Draw(aBatch, aPos, aColor, offset, Camera.Camera.WorldRectangle.Top);

        public virtual void Draw(SpriteBatch aBatch, Vector2 aPos, float aFeetPosY) => Draw(aBatch, aPos, color, offset, aFeetPosY);

        public virtual void Draw(SpriteBatch aBatch, Vector2 aPos, Color aColor, float aFeetPosY) => Draw(aBatch, aPos, aColor, offset, aFeetPosY);

        public virtual void Draw(SpriteBatch aBatch, Vector2 aPos, Color aColor, Vector2 aOffset, float aFeetPosY) => FinalDraw(aBatch, new Rectangle(aPos.ToPoint(), ScaledSize), aColor, aOffset, aFeetPosY);

        public virtual void Draw(SpriteBatch aBatch, AbsoluteScreenPosition aPos, float aFeetPosY) => Draw(aBatch, aPos, Color.White, aFeetPosY);
        public virtual void Draw(SpriteBatch aBatch, AbsoluteScreenPosition aPos, Color aColor, float aFeetPosY) => Draw(aBatch, aPos, aColor, offset, aFeetPosY);
        public virtual void Draw(SpriteBatch aBatch, AbsoluteScreenPosition aPos, Color aColor, Vector2 aOffset, float aFeetPosY) => FinalDraw(aBatch, new Rectangle(aPos, ScaledSize), aColor, aOffset, aFeetPosY);

        public virtual void Draw(SpriteBatch aBatch, Rectangle aPos, Color aColor, Vector2 aOffset, float aFeetPosY) => FinalDraw(aBatch, aPos, aColor, aOffset, aFeetPosY);


        void FinalDraw(SpriteBatch aBatch, Rectangle aPos, Color aColor, Vector2 aOffset, float aFeetPosY)
        {
            if (gfx == null) return;
            if (!Camera.Camera.ScreenspaceBoundsCheck(aPos)) return;
            aBatch.Draw(gfx, aPos, visible, aColor, rotation, aOffset, flip, (aFeetPosY - Camera.Camera.WorldRectangle.Top) / (Camera.Camera.WorldRectangle.Bottom - Camera.Camera.WorldRectangle.Top));
        }

        public void ChangeGfx(GfxPath aPath)
        {
            if (aPath == null || aPath.Name == null)
            {
                gfx = null;
                return;
            }

            gfx = TextureManager.GetTexture(aPath);
        }
    }
}
