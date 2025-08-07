using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
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
        public Minimap(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("GrayBackground", Color.White), aPos, aSize)
        {
        }

        public override void Draw(SpriteBatch aBatch)
        {
            //TODO: Draw to a rendertarget first and then draw that in the correct positon?
            base.Draw(aBatch);
            GraphicsManager.CaptureScissor(this, AbsolutePos);
            Point p = ObjectManager.Player.FeetPosition.ToPoint() - (Size * TileManager.TileSize) / new Point(2);
            DebugManager.Print(GetType(), "New draw");
            DebugManager.Print(GetType(), p);
            DebugManager.Print(GetType(), (Size * TileManager.TileSize));
            DebugManager.Print(GetType(), "");

            WorldSpace ws = new WorldSpace(Location + p / (TileManager.TileSize));
            DebugManager.Print(GetType(), p / (TileManager.TileSize));
            DebugManager.Print(GetType(), ws);
            TileManager.MinimapDraw(aBatch, ws, Size.ToVector2());
            GraphicsManager.ReleaseScissor(this);

        }
    }
}
