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
    [DebuggerStepThrough]
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
