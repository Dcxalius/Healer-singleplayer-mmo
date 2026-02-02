using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Managers;
using Project_1.Tiles;
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
        protected GfxPath gfxPath;

        internal readonly struct TextureRenderSnapshot
        {
            public TextureRenderSnapshot(GfxPath path, Rectangle? visible, Color color, float rotation, Vector2 offset, SpriteEffects flip, Point size)
            {
                Path = path;
                Visible = visible;
                Color = color;
                Rotation = rotation;
                Offset = offset;
                Flip = flip;
                Size = size;
            }

            public GfxPath Path { get; }
            public Rectangle? Visible { get; }
            public Color Color { get; }
            public float Rotation { get; }
            public Vector2 Offset { get; }
            public SpriteEffects Flip { get; }
            public Point Size { get; }
        }
        
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
            return TextureManager.GetAvgColor(aPath);
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


            gfxPath = aPath;
            if (aPath == null) return;
            if (aPath.Name == null) return;

            if (ThreadAffinity.IsMainThread)
            {
                gfx = TextureManager.GetTexture(aPath);
                if (aSize == Point.Zero) { size = gfx.Bounds.Size; }
                else { size = aSize; }
            }
            else
            {
                if (aSize == Point.Zero)
                {
                    size = TextureManager.GetTextureSize(aPath);
                }
                else
                {
                    size = aSize;
                }
            }
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
            if (gfx == null)
            {
                EnsureLoaded();
            }
            if (gfx == null) return;
            if (!Camera.Camera.ScreenspaceBoundsCheck(aPos)) return;
            aBatch.Draw(gfx, aPos, visible, aColor, rotation, aOffset, flip, (aFeetPosY - Camera.Camera.WorldRectangle.Top) / (Camera.Camera.WorldRectangle.Bottom - Camera.Camera.WorldRectangle.Top));
        }

        public void ChangeGfx(GfxPath aPath)
        {
            if (aPath == null || aPath.Name == null)
            {
                gfx = null;
                gfxPath = aPath;
                return;
            }

            gfxPath = aPath;
            if (ThreadAffinity.IsMainThread)
            {
                gfx = TextureManager.GetTexture(aPath);
                if (size == Point.Zero)
                {
                    size = gfx.Bounds.Size;
                }
            }
            else
            {
                gfx = null;
                if (size == Point.Zero)
                {
                    size = TextureManager.GetTextureSize(aPath);
                }
            }
        }

        protected void EnsureLoaded()
        {
            if (gfx != null) return;
            if (gfxPath == null || gfxPath.Name == null) return;
            ThreadAffinity.AssertMainThread();
            gfx = TextureManager.GetTexture(gfxPath);
            if (size == Point.Zero)
            {
                size = gfx.Bounds.Size;
            }
        }

        internal TextureRenderSnapshot BuildRenderSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            return new TextureRenderSnapshot(gfxPath, visible, color, rotation, offset, flip, size);
        }

        internal static void DrawSnapshot(SpriteBatch aBatch, in TextureRenderSnapshot snapshot, WorldSpace worldPos, float feetPosY)
        {
            ThreadAffinity.AssertMainThread();
            if (snapshot.Path == null || snapshot.Path.Name == null) return;

            Texture2D texture = TextureManager.GetTexture(snapshot.Path);
            Point size = snapshot.Size == Point.Zero ? texture.Bounds.Size : snapshot.Size;
            Point scaledSize = new Point((int)Math.Ceiling(size.X * Camera.Camera.Scale), (int)Math.Ceiling(size.Y * Camera.Camera.Scale));
            Rectangle dest = new Rectangle(worldPos.ToAbsoltueScreenPosition(), scaledSize);
            if (!Camera.Camera.ScreenspaceBoundsCheck(dest)) return;

            float depth = (feetPosY - Camera.Camera.WorldRectangle.Top) / (Camera.Camera.WorldRectangle.Bottom - Camera.Camera.WorldRectangle.Top);
            aBatch.Draw(texture, dest, snapshot.Visible, snapshot.Color, snapshot.Rotation, snapshot.Offset, snapshot.Flip, depth);
        }
    }
}
