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

        public Minimap(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(null, new UITexture("GrayBackground", Color.White), aPos, aSize)
        {
            Init();
            ForceVolatileRender = true;
        }

        protected override void DrawSelf(SpriteBatch aBatch)
        {
            Project_1.Managers.ThreadAffinity.AssertMainThread();
            base.DrawSelf(aBatch);
            GraphicsManager.CaptureScissor(this, AbsolutePos);

            if (!MinimapSnapshotManager.TryGetSnapshot(out MinimapDotSnapshot[] dots, out int dotCount, out WorldSpace ws))
            {
                GraphicsManager.ReleaseScissor(this);
                return;
            }
            TileManager.DrawMinimapSnapshots(aBatch, ws, Location, Size);
            Camera.Camera.MinimapDraw(aBatch, ws, Location, Size);
            DrawEntities(aBatch, dots, dotCount, ws);
            GraphicsManager.ReleaseScissor(this);

        }

        void DrawEntities(SpriteBatch aBatch, MinimapDotSnapshot[] dots, int dotCount, WorldSpace origin)
        {
            Point minimapCentre = (Location + Size / 2).ToPoint();
            for (int i = 0; i < dotCount; i++)
            {
                MinimapDotSnapshot dot = dots[i];
                if (dot.IsPlayer)
                {
                    minimapDot.Draw(aBatch, new Rectangle(minimapCentre + new Point(0, 1), new Point(1)), dot.Color);
                    continue;
                }
                int tileX = (int)MathF.Floor((dot.Position.X - origin.X) / Tile.Size.X);
                int tileY = (int)MathF.Floor((dot.Position.Y - origin.Y) / Tile.Size.Y);
                minimapDot.Draw(aBatch,
                    new Rectangle(minimapCentre + new Point(tileX, tileY + 1), new Point(1)),
                    dot.Color);
            }
        }
    }
}
