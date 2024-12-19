using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.DebugTools;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
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
        static Dictionary<string, TileData> tileData = new Dictionary<string, TileData>();

        public static void Init(ContentManager aContentManager)
        {
            ImportData(aContentManager.RootDirectory, aContentManager);
            GenerateTiles(new Point(0), new Point(100, 100));
        }


        static void ImportData(string aPathToData, ContentManager aContentManager)
        {
            string[] dataAsString = System.IO.File.ReadAllLines(aPathToData + "\\Data\\TileData.json");

            for (int i = 0; i < dataAsString.Length; i++)
            {
                TileData data = JsonConvert.DeserializeObject<TileData>(dataAsString[i]);
                tileData.Add(data.Name, data);
            }
        }

        static void GenerateTiles(Point aLeftUppermostTile, Point aSize)
        {
            tiles = new Tile[aSize.X, aSize.Y];

            for (int i = 0; i < aSize.X; i++)
            {
                for (int j = 0; j < aSize.Y; j++)
                {
                    Point pos = new Point(aLeftUppermostTile.X + TileSize.X * i, aLeftUppermostTile.Y + TileSize.Y * j);

                    if (i == 0 || j == 0 || i == aSize.X-1 || j == aSize.Y-1 || (i >= 4 && i < 6 && j >= 4 && j < 6 ))
                    {
                        tiles[i, j] = new Tile(tileData["Wall"], pos, new Point(i, j));
                    }
                    else
                    {
                        Tile leftTile = tiles[i - 1, j];
                        Tile upTile = tiles[i, j - 1];

                        float oddsOfDirt = 0.1f;

                        if (leftTile.Name == "Wall" || upTile.Name == "Wall")
                        {
                            oddsOfDirt++;
                        }
                        if (leftTile.Name == "Wall" || leftTile.Name == "Dirt")
                        {
                            oddsOfDirt += 0.1f;

                        }
                        if (upTile.Name == "Wall" || upTile.Name == "Dirt")
                        {
                            if (oddsOfDirt > 0)
                            {
                                oddsOfDirt += 0.3f;
                            }
                            oddsOfDirt += 0.2f;
                        }

                        if (RandomManager.RollDouble() < oddsOfDirt)
                        {
                            tiles[i, j] = new Tile(tileData["Dirt"], pos, new Point(i, j));
                        }
                        else
                        {
                            tiles[i, j] = new Tile(tileData["Grass"], pos, new Point(i, j));
                        }
                    }
                }
            }
        }

        public static float GetDragCoeficient(WorldSpace aCentreOfObject)
        {
            return GetTileUnder(aCentreOfObject).DragCoeficient;
        }
        
        public static bool CheckLineOfSight(Entity aCaster, WorldSpace aTarget)
        {
            

            WorldSpace start = aCaster.FeetPosition;

            Vector2 line = aTarget - start;
            Vector2 dirVector = Vector2.Normalize(line);

            //DebugManager.debugShapes.Add(new DebugTools.DebugLine(start, (WorldSpace)dirVector, line.Length()));

            float dirX = dirVector.X / Math.Abs(dirVector.X);
            float dirY = dirVector.Y / Math.Abs(dirVector.Y);

            Tile startTile = GetTileUnder(start);
            //DebugManager.Print(typeof(TileManager), "Start tile X: " + startTile.TilePos.X + " Y: " + startTile.TilePos.Y);
            Tile targetTile = GetTileUnder(aTarget);
            //DebugManager.Print(typeof(TileManager), "Target tile X: " + targetTile.TilePos.X + " Y: " + targetTile.TilePos.Y);
            if (!startTile.Transparent || !targetTile.Transparent) return false;

            Vector2 startInTileSpace = new Vector2(start.X / TileSize.X, (start.Y / TileSize.Y));
            Vector2 targetInTileSpace = new Vector2(aTarget.X / TileSize.X, (aTarget.Y / TileSize.Y));
            double m = (targetInTileSpace.Y - startInTileSpace.Y) / (targetInTileSpace.X - startInTileSpace.X);

            double x = start.X;
            double y = start.Y;
            double c = y - m * x;
            //float y = m * x + c;
            //float x = (y - c) / m

            Tile lastTile = startTile;
            while (true)
            {

                float borderInX = dirX > 0 ? lastTile.WorldRectangle.Right : lastTile.WorldRectangle.Left;
                double yAtBorder = m * borderInX + c;
                if (yAtBorder > lastTile.WorldRectangle.Top && yAtBorder < lastTile.WorldRectangle.Bottom)
                {
                    lastTile = tiles[lastTile.TilePos.X + (int)dirX, lastTile.TilePos.Y];
                }
                else
                {
                    lastTile = tiles[lastTile.TilePos.X, lastTile.TilePos.Y + (int)dirY];
                }

                if (!lastTile.Transparent) return false;
                if (lastTile == targetTile) return true;
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

        static Tile GetTileUnder(WorldSpace aWorldSpace)
        {
            return tiles[(int)Math.Floor(aWorldSpace.X / TileSize.X), (int)Math.Floor(aWorldSpace.Y / TileSize.Y)];
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
