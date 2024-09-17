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
        
        static bool CollisionsWithUnwalkable(Rectangle aWorldRect)
        {
            Tile[,] a = GetSurroundingTiles(aWorldRect.Center / TileSize);

            Rectangle[] colliders = GetColliders(a);



            return true;
        }

        static Tile[,] GetSurroundingTiles(Point aIndex)
        {
            const int sizeOfCheckSquare = 3;
            Tile[,] a = new Tile[sizeOfCheckSquare, sizeOfCheckSquare];

            for (int i = 0; i < sizeOfCheckSquare; i++)
            {
                for (int j = 0; j < sizeOfCheckSquare; j++)
                {
                    a[i, j] = tiles[aIndex.X - (int)Math.Floor(sizeOfCheckSquare/2m) + i, aIndex.Y - (int)Math.Floor(sizeOfCheckSquare / 2m) + j];
                }
            }
            return a;
        }

        static Rectangle[] GetColliders(Tile[,] a)
        {
            List<Rectangle> colliders = new List<Rectangle>();
            for (int i = 0; i < a.GetLength(0); i++)
            {
                for (int j = 0; j < a.GetLength(1); j++)
                {
                    if (!a[i,j].Walkable)
                    {
                        colliders.Add(a[i, j].WorldRectangle);
                    }
                }
            }

            return colliders.ToArray();
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
