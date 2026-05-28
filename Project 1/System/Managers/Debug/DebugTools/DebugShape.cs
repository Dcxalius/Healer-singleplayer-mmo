using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.DebugTools
{
    [DebuggerStepThrough]
    internal class DebugShape
    {
        //TODO: Needs DebugCircle
        //TODO: Needs DebugComplex; A shape consisting of other smaller debugshapes
        protected Texture2D texture;
        protected float size = 1.0f;
        readonly Color color;

        [DebuggerStepThrough]
        public DebugShape(Color c)
        {
            color = c;
        }

        public virtual void Draw(SpriteBatch aBatch)
        {

        }

        protected virtual void Draw(SpriteBatch aBatch, WorldSpace aPos)
        {
            ThreadAffinity.AssertMainThread();
            EnsureTexture();
            aBatch.Draw(texture, aPos.ToAbsoltueScreenPosition().ToVector2(), null, Color.White, 0f, Vector2.Zero, size, SpriteEffects.None, 1f);

        }

        protected virtual void Draw(SpriteBatch aBatch, Rectangle aRect)
        {
            ThreadAffinity.AssertMainThread();
            EnsureTexture();
            aBatch.Draw(texture, Camera.Camera.WorldRectToScreenRect(aRect), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1f);
        }

        void EnsureTexture()
        {
            if (texture != null) return;
            ThreadAffinity.AssertMainThread();
            texture = GraphicsManager.CreateNewTexture(new Point(1));
            Color[] data = { color };
            texture.SetData(data);
        }
    }
}
