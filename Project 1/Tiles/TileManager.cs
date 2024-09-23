using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Tiles
{
    internal static class TileManager
    {
        static Tile[,] tiles;
        readonly static Point TileSize = new Point(32, 32);
        const int sizeOfSquareToCheck = 3; // this should always be odd
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

                    if (i == 0 || j == 0 || i == aSize.X-1 || j == aSize.Y-1)
                    {
                        tiles[i, j] = new Wall(pos);
                    }
                    else
                    {
                        tiles[i, j] = new Grass(pos);
                    }
                }
            }
        }
        
        public static List<Rectangle> CollisionsWithUnwalkable(Rectangle aWorldRect)
        {

            List<Rectangle> finalColliders = ConvertUnwalkableTilesToRectangles(aWorldRect.Center);

            List<Rectangle> collisions = new List<Rectangle>();
            foreach (var collider in finalColliders)
            {
                if (aWorldRect.Intersects(collider))
                {
                    collisions.Add(Rectangle.Intersect(aWorldRect, collider));
                }
            }
            return collisions;
        }

        static List<Rectangle> ConvertUnwalkableTilesToRectangles(Point aPos)
        {
            Tile[,] tilesSurroundingPlayer = GetSurroundingTiles(aPos / TileSize);

            Rectangle?[,] colliders = GetColliders(tilesSurroundingPlayer);

            return Merge(colliders);
        }

        static List<Rectangle> Merge(Rectangle?[,] aCollidersToMerge)
        {
            List<Rectangle> finalColliders = RightMerge(aCollidersToMerge);
            finalColliders.AddRange(DownMerge(aCollidersToMerge));

            return finalColliders;
        }
        static List<Rectangle> DownMerge(Rectangle?[,] aCollidersToCheck)
        {
            List<Rectangle> finalColliders = new List<Rectangle>();
            int[,] consumedBy = new int[aCollidersToCheck.GetLength(0), aCollidersToCheck.GetLength(1)];
            for (int i = 0; i < aCollidersToCheck.GetLength(0); i++)
            {
                for (int j = 0; j < aCollidersToCheck.GetLength(1) - 1; j++)
                {
                    if (aCollidersToCheck[i, j] == null || aCollidersToCheck[i, j + 1] == null)
                    {
                        continue;
                    }
                    if (aCollidersToCheck[i, j].Value.Bottom == aCollidersToCheck[i, j + 1].Value.Top && aCollidersToCheck[i, j].Value.X == aCollidersToCheck[i, j + 1].Value.X)
                    {
                        consumedBy[i, j + 1] = finalColliders.Count;
                        if (consumedBy[i, j] != 0)
                        {
                            Rectangle r = Rectangle.Union(finalColliders[consumedBy[i, j]], aCollidersToCheck[i, j + 1].Value);

                            finalColliders[consumedBy[i, j]] = r;
                            consumedBy[i, j + 1] = consumedBy[i, j];
                        }
                        else
                        {
                            finalColliders.Add(Rectangle.Union(aCollidersToCheck[i, j].Value, aCollidersToCheck[i, j + 1].Value));
                        }

                    }
                }

            }

            return finalColliders;
        }

        static List<Rectangle> RightMerge(Rectangle?[,] aCollidersToCheck)
        {
            List<Rectangle> finalColliders = new List<Rectangle>();
            bool[] consumed = new bool[aCollidersToCheck.Length];
            for (int i = 0; i < aCollidersToCheck.GetLength(0) - 1; i++)
            {
                for (int j = 0; j < aCollidersToCheck.GetLength(1); j++)
                {
                    if (aCollidersToCheck[i, j] == null || aCollidersToCheck[i + 1, j] == null)
                    {
                        continue;
                    }
                    if (aCollidersToCheck[i, j].Value.Right == aCollidersToCheck[i + 1, j].Value.Left && aCollidersToCheck[i, j].Value.Y == aCollidersToCheck[i + 1, j].Value.Y)
                    {
                        consumed[i + 1] = true;
                        if (consumed[i])
                        {
                            Rectangle r = Rectangle.Union(finalColliders.Last(), aCollidersToCheck[i + 1, j].Value);

                            finalColliders.RemoveAt(finalColliders.Count - 1);

                            finalColliders.Add(r);
                        }
                        else
                        {
                            finalColliders.Add(Rectangle.Union(aCollidersToCheck[i, j].Value, aCollidersToCheck[i + 1, j].Value));
                        }

                    }
                }
            }
            return finalColliders;
        }



        static Tile[,] GetSurroundingTiles(Point aIndex)
        {
            Tile[,] a = new Tile[sizeOfSquareToCheck, sizeOfSquareToCheck];

            for (int i = 0; i < sizeOfSquareToCheck; i++)
            {
                for (int j = 0; j < sizeOfSquareToCheck; j++)
                {
                    a[i, j] = tiles[aIndex.X - (int)Math.Floor(sizeOfSquareToCheck/2m) + i, aIndex.Y - (int)Math.Floor(sizeOfSquareToCheck / 2m) + j];
                }
            }
            return a;
        }

        static Rectangle?[,] GetColliders(Tile[,] aTileArray)
        {
            Rectangle?[,] colliders = new Rectangle?[aTileArray.GetLength(0),aTileArray.GetLength(1)];
            for (int i = 0; i < aTileArray.GetLength(0); i++)
            {
                for (int j = 0; j < aTileArray.GetLength(1); j++)
                {
                    if (!aTileArray[i,j].Walkable)
                    {
                        colliders[i,j] = aTileArray[i, j].WorldRectangle;
                    }
                }
            }

            return colliders;
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
