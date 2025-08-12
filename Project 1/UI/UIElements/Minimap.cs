using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Spawners;
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

        static Minimap()
        {
            minimapDot = new UITexture("MinimapDot", Color.White);
        }

        public Minimap(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("GrayBackground", Color.White), aPos, aSize)
        {
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);
            GraphicsManager.CaptureScissor(this, AbsolutePos);

            WorldSpace ws = ObjectManager.Player.FeetPosition;
            TileManager.MinimapDraw(aBatch, ws, Location, Size);
            Camera.Camera.MinimapDraw(aBatch, ws, Location, Size);
            SpawnerManager.MinimapDraw(aBatch, ws, Location, Size);
            ObjectManager.MinimapDraw(aBatch, ws, Location, Size);
            GraphicsManager.ReleaseScissor(this);

        }
    }
}
