using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.Tiles;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
{
    internal class Minimap : Box
    {
        public static UITexture minimapDot;
        static bool initialized;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;
            minimapDot = new UITexture("MinimapDot", Color.White);
        }

        public Minimap(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("GrayBackground", Color.White), aPos, aSize)
        {
            Init();
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);
            GraphicsManager.CaptureScissor(this, AbsolutePos);

            if (!MinimapSnapshotManager.TryGetOrigin(out WorldSpace ws))
            {
                GraphicsManager.ReleaseScissor(this);
                return;
            }
            TileManager.MinimapDraw(aBatch, ws, Location, Size);
            Camera.Camera.MinimapDraw(aBatch, ws, Location, Size);
            DrawEntities(aBatch, ws);
            GraphicsManager.ReleaseScissor(this);

        }

        void DrawEntities(SpriteBatch aBatch, WorldSpace origin)
        {
            MinimapDotSnapshot[] dots = MinimapSnapshotManager.Snapshot;
            for (int i = 0; i < dots.Length; i++)
            {
                MinimapDotSnapshot dot = dots[i];
                minimapDot.Draw(aBatch,
                    new Rectangle(new AbsoluteScreenPosition((dot.Position - origin).ToPoint()) / (TileManager.TileSize) + Location + Size / 2 + new Point(0, 1), new Point(1)),
                    dot.Color);
            }
        }
    }
}
