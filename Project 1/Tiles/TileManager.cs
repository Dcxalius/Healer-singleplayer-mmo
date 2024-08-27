using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Tiles
{
    internal static class TileManager
    {
        static Tile[,] tiles;
        readonly static Point TileSize = new Point(32, 32);
        public static void Init()
        {
            GenerateTiles(new Point(0), new Point(100, 100));
        }

        static void GenerateTiles(Point aLeftUppermostTile, Point aSize)
        {
            tiles = new Tile[aSize.X, aSize.Y];

            for (int i = 0; i < aSize.X; i++)
            {
                for (int j = 0; j < aSize.Y; j++)
                {
                    Point pos = new Point(aLeftUppermostTile.X + TileSize.X * i, aLeftUppermostTile.Y + TileSize.Y * j);
                    tiles[i, j] = new Grass(pos);
                }
            }
        }

        public static void Draw(SpriteBatch aBatch)
        {
            foreach (var tile in tiles)
            {
                tile.Draw(aBatch);
            }
        }
    }
}
