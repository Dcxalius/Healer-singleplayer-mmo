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

        public virtual void Draw(SpriteBatch aBatch, Vector2 aPos) => Draw(aBatch, aPos, color, offset, Camera.Camera.WorldRectangle.Bottom);

        public virtual void Draw(SpriteBatch aBatch, Vector2 aPos, float aFeetPosY) => Draw(aBatch, aPos, color, offset, aFeetPosY);

        public virtual void Draw(SpriteBatch aBatch, Vector2 aPos, Color aColor, float aFeetPosY) => Draw(aBatch, aPos, aColor, offset, aFeetPosY); //TODO: Remove these?

        public virtual void Draw(SpriteBatch aBatch, Vector2 aPos, Color aColor, Vector2 aOffset, float aFeetPosY)
        {
            if (gfx == null) return;

            if (Camera.Camera.MomAmIInFrame(new Rectangle(aPos.ToPoint(), (size.ToVector2() * Camera.Camera.Scale).ToPoint())))
            {
                //aBatch.Draw(gfx, aPos, visible, aColor, rotation, offset, Camera.Camera.Scale, flip, aFeetPosY / (Camera.Camera.WorldRectangle.Bottom + size.Y));
                //aBatch.Draw(gfx, new Rectangle(aPos.ToPoint(),ScaledSize), visible, aColor, rotation, offset, flip, aFeetPosY / (Camera.Camera.WorldRectangle.Bottom + size.Y));
                aBatch.Draw(gfx, new Rectangle(new Point((int)Math.Round(aPos.X), (int)Math.Round(aPos.Y)), ScaledSize), visible, aColor, rotation, aOffset, flip, aFeetPosY / (Camera.Camera.WorldRectangle.Bottom + size.Y));
            }
        }

        public virtual void Draw(SpriteBatch aBatch, AbsoluteScreenPosition aPos, float aFeetPosY) => Draw(aBatch, aPos, Color.White, aFeetPosY);
        public virtual void Draw(SpriteBatch aBatch, AbsoluteScreenPosition aPos, Color aColor, float aFeetPosY) => Draw(aBatch, aPos, aColor, offset, aFeetPosY);
        public virtual void Draw(SpriteBatch aBatch, AbsoluteScreenPosition aPos, Color aColor, Vector2 aOffset, float aFeetPosY)
        {
            if (gfx == null) return;

            if (Camera.Camera.MomAmIInFrame(new Rectangle(aPos, (size.ToVector2() * Camera.Camera.Scale).ToPoint())))
            {
                aBatch.Draw(gfx, new Rectangle(aPos, ScaledSize), visible, aColor, rotation, aOffset, flip, aFeetPosY / (Camera.Camera.WorldRectangle.Bottom + size.Y));
            }
        }

        public virtual void Draw(SpriteBatch aBatch, Rectangle aPos, Color aColor, Vector2 aOffset, float aFeetPosY)
        {
            if (gfx == null) return;

            //if (Camera.Camera.MomAmIInFrame(aPos * Camera.Camera.Scale)) TODO: Make this check if its in frame.
            {
                aBatch.Draw(gfx, aPos, visible, aColor, rotation, aOffset, flip, aFeetPosY / (Camera.Camera.WorldRectangle.Bottom + size.Y)); 
            }
        }
    }
}
