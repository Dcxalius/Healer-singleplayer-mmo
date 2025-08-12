using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.DebugTools;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spawners;
using Project_1.Managers;
using Project_1.Managers.Saves;
using SharpDX.Direct3D9;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static Project_1.GameObjects.Spells.AoE.AreaOfEffectData;

namespace Project_1.Tiles
{
    internal static class TileManager
    {
        static Tile Tile(int aChunkId, int aX, int aY) => chunks.Find(x => x.Id == aChunkId).Tile(aX, aY);
        static Chunk GetChunk(int aId) => chunks.Find(x => x.Id == aId);
        static List<Chunk> chunks;

        static TileManager()
        {
            chunks = new List<Chunk>();
        }

        static int GetChunkId(int aX, int aY) //TODO: Needs testing
        {
            if (aX == 0 && aY == 0) return 0;
            int x = aX;
            int y = aY;

            int dirInt;

            int max;
            int min;

            if (Math.Abs(x) >= Math.Abs(y))
            {
                if (x < 0) //Left
                {
                    dirInt = 3;
                }
                else //Right
                {
                    dirInt = 7;
                }

                max = x;
                min = y;
            }
            else
            {
                if (y < 0) //Up
                {
                    dirInt = 1;
                }
                else //Down
                {
                    dirInt = 5;
                }

                max = y;
                min = x;
            }

            return (dirInt * max) + TriangleNumber(max) + min;
        }

        public static Point GetChunkPosition(int aId)//TODO: Needs testing
        {
            double triangle = (Math.Sqrt(aId + 1) - 1) / 2;
            int circle = (int)Math.Ceiling(triangle);
            int highestNrInCircle = TriangleNumber(circle);
            int dif = highestNrInCircle - aId;
            if (dif == 0) return new Point(highestNrInCircle, -highestNrInCircle);
            
            int sideLength = circle * 2;
            if (dif % (sideLength) == 0)
            {
                if (dif / (sideLength) == 1) return new Point(highestNrInCircle, highestNrInCircle);
                if (dif / (sideLength) == 2) return new Point(-highestNrInCircle, highestNrInCircle);
                if (dif / (sideLength) == 3) return new Point(-highestNrInCircle, -highestNrInCircle);
                throw new Exception("ohno");
            }

            int x;
            int y;

            if (dif / (sideLength) < 1)
            {
                x = highestNrInCircle;
                y = -sideLength / 2 + sideLength * dif / sideLength;
            }
            else if (dif / (sideLength) < 2)
            {
                x = sideLength / 2 - sideLength * (dif - sideLength) / sideLength;
                y = highestNrInCircle;
            }
            else if (dif / sideLength < 3)
            {
                x = -highestNrInCircle;
                y = sideLength / 2 - sideLength * (dif - sideLength * 2) / sideLength;
            }
            else if (dif / sideLength < 4)
            {
                x = -sideLength / 2 + sideLength * (dif - sideLength * 3) / sideLength;
                y = -highestNrInCircle;
            }
            else throw new Exception("ohno");

            return new Point(x, y);
        }

        static int TriangleNumber(int aX) => (8 * ((aX * aX) - aX) / 2);


        public readonly static Point TileSize = new Point(32, 32);
        const int sizeOfSquareToCheck = 3; // this should always be odd

        public readonly static Point debugSize = new Point(100, 100);

        static PathFinder pathFinder = new PathFinder();

        public static void New()
        {
            chunks.Add(new Chunk(Point.Zero, 0));
        }

        public static void Load(Save aSave)
        {
            chunks.Clear();

            string[] files = System.IO.Directory.GetFiles(aSave.Tiles);
            for (int i = 0; i < files.Length; i++)
            {
                string json = System.IO.File.ReadAllText(files[i]);
                Chunk c = SaveManager.ImportData<Chunk>(json);
                //int[,] tileIds = JsonConvert.DeserializeObject<int[,]>(json);
                int id = int.Parse(SaveManager.TrimToNameOnly(files[i]));
                chunks.Add(c);

                
            }
        }

        public static float GetDragCoeficient(WorldSpace aFeetPos) => GetTileUnder(aFeetPos).DragCoeficient;

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
                    lastTile = chunks[0].Tile(lastTile.GridPos.X + (int)dirX, lastTile.GridPos.Y);
                    //lastTile = tiles[lastTile.GridPos.X + (int)dirX, lastTile.GridPos.Y];
                }
                else
                {
                    lastTile = chunks[0].Tile(lastTile.GridPos.X, lastTile.GridPos.Y + (int)dirY);
                    //lastTile = tiles[lastTile.GridPos.X, lastTile.GridPos.Y + (int)dirY];
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
            Tile t = chunks[0].Tile((int)Math.Floor(aWorldSpace.X / TileSize.X), (int)Math.Floor(aWorldSpace.Y / TileSize.Y));
            //Tile t = tiles[(int)Math.Floor(aWorldSpace.X / TileSize.X), (int)Math.Floor(aWorldSpace.Y / TileSize.Y)];
            return t;
        }

        public static Chunk GetChunkUnder(WorldSpace aWorldSpace)
        {
            return chunks[0];
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
        }

        public static Tile FindClosestWalkable(WorldSpace aWorldSpace)
        {
            Tile underStart = GetTileUnder(aWorldSpace);
            if (underStart.Walkable) return underStart;

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
                        continue;
                    }

                    if (t.Walkable)
                    {
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
                    a[i, j] = chunks[0].Tile(x, y);
                    //a[i, j] = tiles[x, y];
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

        public static Tile GetTileAt(int aX, int aY) => Tile(0, aX, aY);
        //public static Tile GetTileAt(int aX, int aY) => Tile(aX, aY);

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
                
                Tile t = Tile(0, (int)Math.Round(tilePos.X + distanceInTiles * Math.Sin((i * 2 * Math.PI / tilesAround))), (int)Math.Floor(tilePos.Y + distanceInTiles * Math.Cos(i * 2 * Math.PI / tilesAround)));

                if (t == null) continue;

                if (!t.Walkable) continue;
                returnable.Add(t);
            }

            return returnable.ToArray();
        }

        public static void SaveData(Save aSave)
        {


            for (int i = 0; i < chunks.Count; i++)
            {
                SaveManager.ExportData(aSave.Tiles + "\\" + chunks[i].Id + ".tilemap", chunks[i]);

            }

        }

        public static void MinimapDraw(SpriteBatch aBatch, WorldSpace aOrigin, AbsoluteScreenPosition aMinimapOffset, AbsoluteScreenPosition aSize)
        {
            //TODO: Draw neighbours as well
            //int chunkID = GetChunkId((int)aMinimapWorldPos.X / (debugSize.X * TileSize.X), (int)aMinimapWorldPos.Y / (debugSize.Y * TileSize.Y));
            //GetChunk(chunkID).MinimapDraw(aBatch, aMinimapWorldPos);
            GetChunk(0).MinimapDraw(aBatch, aOrigin, aMinimapOffset, aSize);

        }

        public static void Draw(SpriteBatch aBatch)
        {
            foreach (var chunk in chunks)
            {
                chunk.Draw(aBatch);
            }
        }
    }
}
