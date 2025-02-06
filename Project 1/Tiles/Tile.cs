using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Textures;
using Project_1.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.Camera;
using Project_1.Managers;

namespace Project_1.Tiles
{
    internal class Tile : GameObject
    {
        readonly static Point tileSize = new Point(32);
        public string Name => tileData.Name;
        public bool Walkable => tileData.Walkable;
        public float DragCoeficient => tileData.DragCoeficient;

        public bool Transparent
        {
            get
            {
                //DebugManager.debugShapes.Add(new DebugTools.DebugSquare(new Rectangle(Position.ToPoint(), tileSize)));
                return tileData.Transparent;
            }
        }

        public Point GridPos => tilePos;
        Point tilePos;

        Text xText;
        Text yText;

        UITexture debugTexture;
        TileData tileData;

        //bool 

        public Tile(TileData aTileData, Point aPos, Point aTilePos) : base(new Textures.RandomlyGeneratedTexture(true, tileSize, new GfxPath(GfxType.Tile, aTileData.Name)), (WorldSpace)aPos.ToVector2())
        {
            debugTexture = new UITexture(new GfxPath(GfxType.Debug, "Debug"), Color.White);
            tileData = aTileData;
            tilePos = aTilePos;
            if (DebugManager.Mode(DebugMode.TileCoords))
            {
                xText = new Text("Gloryse", tilePos.X.ToString(), Color.Black);
                yText = new Text("Gloryse", tilePos.Y.ToString(), Color.Yellow);
            }
        }

        public void AddDebugSquare()
        {
            DebugManager.AddDebugShape(new DebugTools.DebugSquare(new Rectangle(Position.ToPoint(), tileSize)));
        }

        public override void Draw(SpriteBatch aBatch)
        {
            //base.Draw(aBatch); Skip draw as a gameobject to make all tiles appear in the background
            gfx.Draw(aBatch, Position.ToAbsoltueScreenPosition(), Color.White, 0);

            //xText.LeftAllignedDraw(aBatch, new WorldSpace(Position - Size.ToVector2() / 2).ToAbsoltueScreenPosition());
            if (DebugManager.Mode(DebugMode.TileCoords))
            {
                xText.CentreLeftDraw(aBatch, new WorldSpace(Position).ToAbsoltueScreenPosition());
                yText.CentreLeftDraw(aBatch, new WorldSpace(Position + new WorldSpace(xText.Offset.X, 0)).ToAbsoltueScreenPosition());
            }
            //Camera.Camera.WorldPosToCameraSpace(Position), 0); 
        }
    }
}
