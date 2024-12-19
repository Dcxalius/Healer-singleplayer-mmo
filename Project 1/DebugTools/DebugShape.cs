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
    internal class DebugShape
    {
        protected Texture2D texture;
        protected float size = 1.0f;

        [DebuggerStepThrough]
        public DebugShape(Color c)
        {
            texture = GraphicsManager.CreateNewTexture(new Point(1));
            Color[] data = { c };
            texture.SetData(data);
        }

        public virtual void Draw(SpriteBatch aBatch)
        {

        }

        protected virtual void Draw(SpriteBatch aBatch, WorldSpace aPos)
        {
            aBatch.Draw(texture, aPos.ToAbsoltueScreenPosition().ToVector2(), null, Color.White, 0f, Vector2.Zero, size, SpriteEffects.None, 1f);

        }

        protected virtual void Draw(SpriteBatch aBatch, Rectangle aRect)
        {
            aBatch.Draw(texture, Camera.Camera.WorldRectToScreenRect(aRect), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1f);
        }
    }
}
