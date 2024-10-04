using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Textures;
using Project_1.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Tiles
{
    internal class Tile : GameObject
    {
        readonly static Point tileSize = new Point(32);
        public string Name { get => tileData.Name; }
        public bool Walkable { get => tileData.Walkable; }
        public float DragCoeficient { get => tileData.DragCoeficient; }


        TileData tileData;

        public Tile(TileData aTileData, Point aPos) : base(new Textures.RandomlyGeneratedTexture(true, tileSize, new GfxPath(GfxType.Tile, aTileData.Name)), aPos.ToVector2())
        { 
            tileData = aTileData;
        }

        public override void Draw(SpriteBatch aBatch)
        {
            //base.Draw(aBatch); Dont draw as a gameobject to make all tiles appear in the background
            gfx.Draw(aBatch, Camera.WorldPosToCameraSpace(Position), Vector2.Zero); 
        }
    }
}
