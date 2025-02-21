using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.DebugTools;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.Managers;
using SharpDX.Direct3D9;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static Project_1.GameObjects.Spells.AoE.AreaOfEffectData;

namespace Project_1.Tiles
{
    internal static class TileManager
    {
        static Tile Tile(int aX, int aY)
        {
            if (aX < 0 || aX >= debugSize.X || aY < 0 || aY >= debugSize.Y) return null;

            return tiles[aX, aY];
        }

        static Tile[,] tiles;
        readonly static Point TileSize = new Point(32, 32);
        const int sizeOfSquareToCheck = 3; // this should always be odd
        static Dictionary<string, TileData> tileData = new Dictionary<string, TileData>();

        static Point debugSize = new Point(100, 100);

        static PathFinder pathFinder = new PathFinder();

        public static void Init(ContentManager aContentManager)
        {
            ImportData(aContentManager.RootDirectory, aContentManager);
            GenerateTiles(new Point(0), debugSize);
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

        public static float GetDragCoeficient(WorldSpace aFeetPos)
        {
            return GetTileUnder(aFeetPos).DragCoeficient;
        }
        
        public static bool CheckLineOfSight(Entity aCaster, WorldSpace aTarget)
        {
            

            WorldSpace start = aCaster.FeetPosition;
            if (start == aTarget) return true;



            Tile startTile = GetTileUnder(start);
            Tile targetTile = GetTileUnder(aTarget);
            if (!startTile.Transparent || !targetTile.Transparent) return false;

            return LineOfSight(start, aTarget, lastTile => !lastTile.Transparent, lastTile => lastTile == targetTile);
        }

        static bool LineOfSight(WorldSpace aStartPos, WorldSpace aEndPos, Func<Tile, bool> aFalseCondition, Func<Tile, bool> aTrueCondition)
        {
            Vector2 dirVector = Vector2.Normalize(aEndPos - aStartPos);

            float dirX = Math.Sign(dirVector.X);
            float dirY = Math.Sign(dirVector.Y);
            double m = (aEndPos.Y - aStartPos.Y) / (aEndPos.X - aStartPos.X);
            double c = aStartPos.Y - m * aStartPos.X;

            Tile lastTile = GetTileUnder(aStartPos);
            while (true)
            {

                float borderInX = dirX > 0 ? lastTile.WorldRectangle.Right : lastTile.WorldRectangle.Left;
                double yAtBorder = m * borderInX + c;
                if (yAtBorder > lastTile.WorldRectangle.Top && yAtBorder < lastTile.WorldRectangle.Bottom)
                {
                    lastTile = tiles[lastTile.GridPos.X + (int)dirX, lastTile.GridPos.Y];
                }
                else
                {
                    lastTile = tiles[lastTile.GridPos.X, lastTile.GridPos.Y + (int)dirY];
                }

                if (aFalseCondition(lastTile)) return false;
                if (aTrueCondition(lastTile)) return true;
            }
        }


        public static Path GetPath(WorldSpace aStartPosition, WorldSpace aTargetPosition, WorldSpace aSize) => pathFinder.GeneratePath(aStartPosition, aTargetPosition, aSize);

        public static List<(Rectangle, Rectangle)> CollisionsWithUnwalkable(Entity aEntity)
        {

            List<Rectangle> finalColliders = ConvertUnwalkableTilesToRectangles(aEntity.FeetPosition);
            Rectangle entityRectangle = aEntity.WorldRectangle;
            List<(Rectangle, Rectangle)> collisions = new List<(Rectangle, Rectangle)>();
            foreach (var collider in finalColliders)
            {
                if (entityRectangle.Intersects(collider))
                {
                    collisions.Add((Rectangle.Intersect(entityRectangle, collider), collider));
                }
            }
            return collisions;
        }

        static List<Rectangle> ConvertUnwalkableTilesToRectangles(WorldSpace aPos)
        {
            Tile[,] tilesSurroundingObject = GetSurroundingTiles(GetTileUnder(aPos));

            Rectangle?[,] colliders = GetColliders(tilesSurroundingObject);

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

        public static Tile GetTileUnder(WorldSpace aWorldSpace)
        {
            if (aWorldSpace.X < 0 || aWorldSpace.X > debugSize.X * TileSize.X || aWorldSpace.Y < 0 || aWorldSpace.Y > debugSize.Y * TileSize.Y) return null; //DEBUG
            Tile t = tiles[(int)Math.Floor(aWorldSpace.X / TileSize.X), (int)Math.Floor(aWorldSpace.Y / TileSize.Y)];
            return t;
        }

        public static WorldSpace FindClosestWalkableWorldSpace(WorldSpace aWorldSpace, WorldSpace aSize)
        {
            if (GetTileUnder(aWorldSpace).Walkable)
            {
                return aWorldSpace;
            }

            Tile closestTile = FindClosestWalkable(aWorldSpace);

            //DebugManager.Print(typeof(TileManager), "Tileboundries are " + closestTile.WorldRectangle);


            Vector2 dirVector = Vector2.Normalize(aWorldSpace - closestTile.Centre);

            float dirX = Math.Sign(dirVector.X);
            float dirY = Math.Sign(dirVector.Y);
            float m = (aWorldSpace.Y - closestTile.Centre.Y) / (aWorldSpace.X - closestTile.Centre.X);
            float c = aWorldSpace.Y - m * aWorldSpace.X;

            if (Math.Abs(dirVector.X) >= Math.Abs(dirVector.Y))
            {
                float borderInX = dirX > 0 ? closestTile.WorldRectangle.Right - aSize.X / 2 : closestTile.WorldRectangle.Left + aSize.X / 2;
                float yAtBorder = m * borderInX + c;
                return new WorldSpace(borderInX, yAtBorder);
            }
            else
            {
                float borderInY = dirY > 0 ? closestTile.WorldRectangle.Bottom - aSize.Y / 2 : closestTile.WorldRectangle.Top + aSize.Y / 2;
                float xAtBorder = (borderInY - c) / m;
                return new WorldSpace(xAtBorder, borderInY);
            }
            

            //if (s.X > s.Y) //TODO: Fix names
            //{
            //    float lengthToSide = TileSize.X / 2 * Math.Sign(s.X);
            //    WorldSpace xdd = closestTile.Centre;
            //    //WorldSpace xdd = closestTile.Centre - new WorldSpace(lengthToSide, lengthToSide / s.X * s.Y);
            //    //WorldSpace xddd = xdd - new WorldSpace(aSize.X / 2 * Math.Sign(s.X), aSize.Y / 2 * Math.Sign(s.Y));
            //    DebugManager.Print(typeof(TileManager), "X was bigger and is " + xdd);
            //    return xdd;
            //}
            //else
            //{
            //    float lengthToSide = TileSize.Y / 2 * Math.Sign(s.Y);
            //    WorldSpace xdd = closestTile.Centre;
            //    //WorldSpace xdd = closestTile.Centre - new WorldSpace(lengthToSide / s.Y * s.X, lengthToSide);
            //    //WorldSpace xddd = xdd - new WorldSpace(aSize.X / 2 * Math.Sign(s.X), aSize.Y / 2 * Math.Sign(s.Y));
            //    DebugManager.Print(typeof(TileManager), "Y was bigger and is " + xdd);
            //    return xdd;

            //}
        }

        public static Tile FindClosestWalkable(WorldSpace aWorldSpace)
        {
            Tile underStart = GetTileUnder(aWorldSpace);
            if (underStart.Walkable) return underStart;
            //DebugManager.Print(typeof(TileManager), "Started looking for better tile around " + underStart.GridPos);
            float x = aWorldSpace.X / TileSize.X;
            x -= (MathF.Floor(x) + 0.5f);
            float y = aWorldSpace.Y / TileSize.Y;
            y -=( MathF.Floor(y) + 0.5f);

            return LookThroughNeighboursForWalkable(aWorldSpace, new WorldSpace(x, y), new WorldSpace(x, 0), new WorldSpace(Math.Abs(x) + 0.5f * -Math.Sign(x), 0), new WorldSpace(0, y), new WorldSpace(0, Math.Abs(y) + 0.5f * -Math.Sign(y)));


        }

        static Tile LookThroughNeighboursForWalkable(WorldSpace aStart, WorldSpace aStartInRelationToStartTile, WorldSpace aCloserDistanceToLeftRight, WorldSpace aFurtherDistanceToLeftRight, WorldSpace aCloserDistanceToTopBottom, WorldSpace aFurtherDistanceToTopBottom)
        {

            List<WorldSpace> spaces = new List<WorldSpace>();
            WorldSpace closerCloserDistance;
            WorldSpace closerFurtherDistance;
            WorldSpace furtherCloserDistance;
            WorldSpace furtherFurtherDistance;

            if (aCloserDistanceToLeftRight.DistanceTo(WorldSpace.Zero) > aCloserDistanceToTopBottom.DistanceTo(WorldSpace.Zero))
            {
                closerCloserDistance = aCloserDistanceToLeftRight;
                closerFurtherDistance = aCloserDistanceToTopBottom;
            }
            else
            {
                closerCloserDistance = aCloserDistanceToTopBottom;
                closerFurtherDistance = aCloserDistanceToLeftRight;
            }

            spaces.Add(closerCloserDistance);
            spaces.Add(closerFurtherDistance);
            spaces.Add(new WorldSpace(aCloserDistanceToLeftRight.X, aCloserDistanceToTopBottom.Y));

            if (aFurtherDistanceToLeftRight.DistanceTo(WorldSpace.Zero) > aFurtherDistanceToTopBottom.DistanceTo(WorldSpace.Zero))
            {
                furtherCloserDistance = aFurtherDistanceToLeftRight;
                furtherFurtherDistance = aFurtherDistanceToTopBottom;
                spaces.Add(furtherCloserDistance);
                spaces.Add(new WorldSpace(aFurtherDistanceToLeftRight.X, aCloserDistanceToTopBottom.Y));
                spaces.Add(furtherFurtherDistance);
                spaces.Add(new WorldSpace(aCloserDistanceToLeftRight.X, aFurtherDistanceToTopBottom.Y));
            }
            else
            {
                furtherCloserDistance = aFurtherDistanceToTopBottom;
                furtherFurtherDistance = aFurtherDistanceToLeftRight;

                spaces.Add(furtherCloserDistance);
                spaces.Add(new WorldSpace(aCloserDistanceToLeftRight.X, aFurtherDistanceToTopBottom.Y));
                spaces.Add(furtherFurtherDistance);
                spaces.Add(new WorldSpace(aFurtherDistanceToLeftRight.X, aCloserDistanceToTopBottom.Y));
            }

            spaces.Add(new WorldSpace(furtherFurtherDistance.X, furtherFurtherDistance.Y));

            for (int i = 0; i < spaces.Count; i++)
            {
                WorldSpace s = new WorldSpace(Math.Sign(spaces[i].X), Math.Sign(spaces[i].Y));
                spaces[i] = s;
            }

            int counter = 0;
            while (true)
            {
                counter++;
                List<int> removables = new List<int>();
                for (int i = 0; i < spaces.Count; i++)
                {
                    Tile t = GetTileUnder(aStart + new WorldSpace(TileSize.X, TileSize.Y) * spaces[i]); //TODO: Inefficent I think
                    if (t == null)
                    {
                        removables.Add(i);
                        //DebugManager.Print(typeof(TileManager), "Remove tile at index: " + i);
                        continue;
                    }

                    //DebugManager.Print(typeof(TileManager), "Check tile at: " + t.GridPos);
                    if (t.Walkable)
                    {
                        //DebugManager.Print(typeof(TileManager), "Found tile at: " + t.GridPos);
                        return t;
                    }
                }
                for (int i = removables.Count - 1; i >= 0; i--)
                {
                    spaces.RemoveAt(removables[i]);
                }

                for (int i = spaces.Count - 1; i >= 0; i--)
                {
                    float x = Math.Sign(spaces[i].X);
                    float y = Math.Sign(spaces[i].Y);
                    spaces[i] += new WorldSpace(x, y);
                    WorldSpace s = spaces[i];
                    if (Math.Abs(spaces[i].X) == counter && Math.Abs(spaces[i].Y) == counter)
                    {
                        WorldSpace closerNew;
                        WorldSpace furtherNew;

                        WorldSpace newInXDir = new WorldSpace(s.X, y);
                        WorldSpace newInYDir = new WorldSpace(x, s.Y);

                        if (newInXDir.DistanceTo(aStartInRelationToStartTile) >= newInYDir.DistanceTo(aStartInRelationToStartTile))
                        {
                            closerNew = newInXDir;
                            furtherNew = newInYDir;
                        }
                        else
                        {
                            closerNew = newInYDir;
                            furtherNew = newInXDir;
                        }

                        spaces.Insert(i, closerNew);
                        spaces.Insert(i + 1, furtherNew);
                    }
                }
            }
        }


        static Tile[,] GetSurroundingTiles(Tile aTile)
        {
            Tile[,] a = new Tile[sizeOfSquareToCheck, sizeOfSquareToCheck];

            for (int i = 0; i < sizeOfSquareToCheck; i++)
            {
                for (int j = 0; j < sizeOfSquareToCheck; j++)
                {
                    int x = aTile.GridPos.X - (int)Math.Floor(sizeOfSquareToCheck / 2m) + i;
                    int y = aTile.GridPos.Y - (int)Math.Floor(sizeOfSquareToCheck / 2m) + j;
                    a[i, j] = tiles[x, y];
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
                    if (aTileArray[i, j] == null) continue;
                    if (!aTileArray[i, j].Walkable)
                    {
                        colliders[i,j] = aTileArray[i, j].WorldRectangle;
                    }
                }
            }

            return colliders;
        }

        public static Tile GetTileAt(Point aCoord) => GetTileAt(aCoord.X, aCoord.Y);

        public static Tile GetTileAt(int aX, int aY) => Tile(aX, aY);

        public static Tile[] GetNeighbours(Point aCoord)
        {
            Tile[] neighbours = new Tile[8];
            neighbours[0] = GetTileAt(aCoord.X - 1, aCoord.Y - 0);
            neighbours[1] = GetTileAt(aCoord.X + 1, aCoord.Y - 0);
            neighbours[2] = GetTileAt(aCoord.X + 0, aCoord.Y - 1);
            neighbours[3] = GetTileAt(aCoord.X + 0, aCoord.Y + 1);
            neighbours[4] = GetTileAt(aCoord.X + 1, aCoord.Y + 1);
            neighbours[5] = GetTileAt(aCoord.X + 1, aCoord.Y - 1);
            neighbours[6] = GetTileAt(aCoord.X - 1, aCoord.Y + 1);
            neighbours[7] = GetTileAt(aCoord.X - 1, aCoord.Y - 1);
            //int index = 0;
            //for (int i = -1; i <= 1; i++)
            //{
            //    for (int j = -1; j <= 1; j++)
            //    {
            //        if (i == 0 && j == 0) continue;
            //        neighbours[index] = GetTileAt(aCoord.X - i, aCoord.Y - j);

            //        index++;
            //    }
            //}
            return neighbours;
        }

        public static Tile[] GetTilesAroundPosition(WorldSpace aPosition, float aDistance)
        {
            List<Tile> returnable = new List<Tile>();
            Tile midTile = GetTileUnder(aPosition);
            float distanceInTiles = aDistance / TileSize.X;
            int tilesAround = (int)(distanceInTiles * Math.PI) * 2;
            for (int i = 0; i < tilesAround; i++)
            {
                Point tilePos = midTile.GridPos;
                //DebugManager.Print(typeof(TileManager), "Sine = " + (Math.Sin((i * 2 * Math.PI / tilesAround))).ToString());
                //DebugManager.Print(typeof(TileManager), "Tile = " + ((int)Math.Round(tilePos.X + distanceInTiles * Math.Sin((i * 2 * Math.PI / tilesAround)))).ToString());
                
                Tile t = Tile((int)Math.Round(tilePos.X + distanceInTiles * Math.Sin((i * 2 * Math.PI / tilesAround))), (int)Math.Floor(tilePos.Y + distanceInTiles * Math.Cos(i * 2 * Math.PI / tilesAround)));
                //DebugManager.Print(typeof(TileManager), "Tile is at X: " + t.TilePos.X + " and Y: " + t.TilePos.Y);

                if (t == null) continue;

                if (!t.Walkable) continue;
                returnable.Add(t);
            }

            return returnable.ToArray();
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
