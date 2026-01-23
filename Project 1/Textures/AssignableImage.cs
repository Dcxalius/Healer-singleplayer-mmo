using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Textures
{
    internal class AssignableImage : UITexture
    {
        string pendingPath;
        bool textureDirty;

        public AssignableImage() : base(GfxPath.NullPath, Color.White)
        {

        }
        public AssignableImage(string aPath) : base(GfxPath.NullPath, Color.White)
        {
            NewImage(aPath);
        }

        public void NewImage(string aPath)
        {
            if (!File.Exists(aPath))
            {
                //TODO: File not found gfx
                pendingPath = null;
                textureDirty = true;
                return;
            }
            pendingPath = aPath;
            textureDirty = true;

            if (ThreadAffinity.IsMainThread)
            {
                EnsureTexture();
            }
        }

        public override void Draw(SpriteBatch aBatch, Rectangle aPosRectangle)
        {
            EnsureTexture();
            base.Draw(aBatch, aPosRectangle);
        }

        public override void Draw(SpriteBatch aBatch, Rectangle aPosRectangle, Color aColor)
        {
            EnsureTexture();
            base.Draw(aBatch, aPosRectangle, aColor);
        }

        void EnsureTexture()
        {
            if (!textureDirty) return;
            textureDirty = false;
            if (pendingPath == null)
            {
                gfx = null;
                return;
            }
            // Render-side texture loading, main-thread only.
            ThreadAffinity.AssertMainThread();
            gfx = GraphicsManager.CreateTextureFromFile(pendingPath);
        }
    }
}
