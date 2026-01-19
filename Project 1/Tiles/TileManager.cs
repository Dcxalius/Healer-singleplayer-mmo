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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Collections.Concurrent;
using System.Threading;
using static Project_1.GameObjects.Spells.AoE.AreaOfEffectData;

namespace Project_1.Tiles
{
    internal static class TileManager
    {
        static int Modulo(int x, int y)
        {
            int r = x % y;
            return r < 0 ? r + y : r;
        }
        public static Tile GetTile(WorldSpace aSpace) => GetTile(GetChunk(aSpace), Modulo((int)MathF.Floor(aSpace.X / TileSize.X), Chunk.ChunkSize.X), Modulo((int)MathF.Floor(aSpace.Y / TileSize.Y), Chunk.ChunkSize.Y));
        public static Tile GetTile(Chunk aChunk, int aX, int aY) => aChunk.Tile(aX, aY);
        public static Tile GetTile(int aChunkId, int aX, int aY) => chunks.Find(x => x.Id == aChunkId).Tile(aX, aY);
        public static Tile GetTileAt(WorldSpace aSpace) => GetTile(aSpace);
        public static Tile GetTilePublic(WorldSpace aSpace) => GetTile(aSpace);

        public static Chunk GetChunk(int aId) => chunks.Find(x => x.Id == aId);
        public static Chunk GetChunk(int aX, int aY) => GetChunk(GetChunkId(aX, aY));
        public static Chunk GetChunk(Point aPos) => GetChunk(aPos.X, aPos.Y);
        public static Chunk GetChunk(WorldSpace aSpaceInWorld) => GetChunk(new Point((int)MathF.Floor(aSpaceInWorld.X / Chunk.ChunkSize.X / TileSize.X), (int)MathF.Floor(aSpaceInWorld.Y / Chunk.ChunkSize.Y / TileSize.Y)));

        static List<Chunk> chunks; //TODO: Use SortedList?
        static volatile Chunk[] renderChunks = Array.Empty<Chunk>();
        static readonly ReaderWriterLockSlim chunkLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        static readonly ConcurrentDictionary<int, int[,]> generatedChunkIds = new ConcurrentDictionary<int, int[,]>();
        static readonly HashSet<int> pendingChunkGenerations = new HashSet<int>();
        const int surroundingChunkCheckSize = 3;// this should always be odd

        public static CollisionManager CollisionManager;


        public readonly static Point TileSize = new Point(32, 32); //If this is changed, change TILE_SIZE in TestDarkness.fx
        const int sizeOfSquareToCheck = 3; // this should always be odd


        static PathFinder pathFinder = new PathFinder();

        static bool initialized;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;
            chunks = new List<Chunk>();
            CollisionManager = new CollisionManager();
            Debug.Assert(sizeOfSquareToCheck % 2 == 1);
            Debug.Assert(surroundingChunkCheckSize % 2 == 1);
        }

        public static void Update()
        {
            ThreadAffinity.AssertSimThread();
            chunkLock.EnterWriteLock();
            try
            {
                Chunk[,] surroundingChunks = new Chunk[surroundingChunkCheckSize, surroundingChunkCheckSize];
                Chunk centreChunk = GetChunkUnder(ObjectManager.Player.FeetPosition);
                Point centreChunkPos = GetChunkPosition(centreChunk.Id);
                surroundingChunks[surroundingChunkCheckSize / 2, surroundingChunkCheckSize / 2] = centreChunk;
                bool addedNew = false;
                int queuedPrefetch = 0;
                int immediateRadius = surroundingChunkCheckSize / 2;
                int prefetchRadius = immediateRadius + 1;

                for (int i = 0; i < surroundingChunks.GetLength(0); i++)
                {
                    for (int j = 0; j < surroundingChunks.GetLength(1); j++)
                    {
                        if (i == surroundingChunkCheckSize / 2 && j == surroundingChunkCheckSize / 2) continue;
                        int newId = GetChunkId(centreChunkPos + new Point(i - surroundingChunkCheckSize / 2, j - surroundingChunkCheckSize / 2));
                        surroundingChunks[i, j] = GetChunk(newId);

                        if (surroundingChunks[i, j] != null) continue;
                        addedNew = true;

                        if (!generatedChunkIds.TryRemove(newId, out int[,] tileIds))
                        {
                            tileIds = Chunk.GenerateTileIds(newId);
                        }
                        surroundingChunks[i, j] = new Chunk(tileIds, newId);
                        chunks.Add(surroundingChunks[i, j]);
                    }
                }
                if (addedNew) chunks.Sort((x, y) => x.Id.CompareTo(y.Id));

                if (WorkerPool.IsRunning)
                {
                    for (int x = -prefetchRadius; x <= prefetchRadius && queuedPrefetch < 4; x++)
                    {
                        for (int y = -prefetchRadius; y <= prefetchRadius && queuedPrefetch < 4; y++)
                        {
                            if (Math.Abs(x) <= immediateRadius && Math.Abs(y) <= immediateRadius) continue;
                            int id = GetChunkId(centreChunkPos + new Point(x, y));
                            if (chunks.Any(c => c.Id == id)) continue;
                            if (generatedChunkIds.ContainsKey(id)) continue;
                            if (pendingChunkGenerations.Contains(id)) continue;

                            QueueChunkGeneration(id);
                            queuedPrefetch++;
                        }
                    }
                }
            }
            finally
            {
                chunkLock.ExitWriteLock();
            }
        }

        static int GetChunkId(Point aPos) => GetChunkId(aPos.X, aPos.Y);
        static int GetChunkId(int aX, int aY)
        {
            if (aX == 0 && aY == 0) return 0;
            
            int dirInt;
            int furthestDir;
            int shortestDir;

            if (Math.Abs(aX) >= Math.Abs(aY))
            {
                if (aX < 0) //Left
                {
                    dirInt = 3;
                    shortestDir = aY;
                }
                else //Right
                {
                    dirInt = 7;
                    shortestDir = -aY;
                }

                furthestDir = aX;
            }
            else
            {
                if (aY < 0) //Up
                {
                    dirInt = 1;
                    shortestDir = -aX;
                }
                else //Down
                {
                    dirInt = 5;
                    shortestDir = aX;
                }

                furthestDir = aY;
            }

            return dirInt * Math.Abs(furthestDir) + HighestNrInCircle(Math.Abs(furthestDir) - 1) + shortestDir;
        }

        public static Point GetChunkPosition(int aId)
        {

            int circle = (int)Math.Ceiling((Math.Sqrt(aId + 1) - 1) / 2);
            int highestNrInCircle = HighestNrInCircle(circle);
            int dif = highestNrInCircle - aId;
            if (dif == 0)
            {
                return new Point(circle, -circle);
            }

            int sideLength = circle * 2;
            if (dif % (sideLength) == 0)
            {
                if (dif / (sideLength) == 1) return new Point(circle, circle);
                if (dif / (sideLength) == 2) return new Point(-circle, circle);
                if (dif / (sideLength) == 3) return new Point(-circle, -circle);
                throw new Exception("ohno");
            }

            Point returnPoint = new Point();

            if ((float)dif / (sideLength) < 1f)//Right
            {
                returnPoint.X = circle;
                returnPoint.Y = -circle + dif;
            }
            else if ((float)dif / (sideLength) < 2f)//Down
            {
                returnPoint.X = circle - (dif - sideLength);
                returnPoint.Y = circle;
            }
            else if ((float)dif / sideLength < 3f)//Left
            {
                returnPoint.X = -circle;
                returnPoint.Y = circle - (dif - sideLength * 2);
            }
            else if ((float)dif / sideLength < 4f)//Up
            {
                returnPoint.X = -circle + (dif - sideLength * 3);
                returnPoint.Y = -circle;
            }
            else throw new Exception("ohno");

            return returnPoint;
        }

        static int HighestNrInCircle(int aCricleSize) => 4 * (((aCricleSize+1) * (aCricleSize+1)) - (aCricleSize+1)); //(2*aCS-1)^2-1



        public static void New()
        {
            chunks.Clear();
            chunks.Add(new Chunk(Chunk.GenerateTileIds(0), 0));
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

        static void QueueChunkGeneration(int chunkId)
        {
            if (!pendingChunkGenerations.Add(chunkId)) return;

            WorkerPool.Enqueue(() => Chunk.GenerateTileIds(chunkId), tileIds =>
            {
                if (tileIds != null)
                {
                    generatedChunkIds[chunkId] = tileIds;
                }
                pendingChunkGenerations.Remove(chunkId);
            });
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
            Vector2 dirVector = aEndPos - aStartPos;

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
                    lastTile = chunks[0].Tile(lastTile.GridPos.X + (int)dirX, lastTile.GridPos.Y); //TODO: Chunkize this
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


        static Path GeneratePathThreadSafe(WorldSpace aStartPosition, WorldSpace aTargetPosition, WorldSpace aSize)
        {
            chunkLock.EnterReadLock();
            try
            {
                return pathFinder.GeneratePath(aStartPosition, aTargetPosition, aSize);
            }
            finally
            {
                chunkLock.ExitReadLock();
            }
        }

        public static Path GetPath(WorldSpace aStartPosition, WorldSpace aTargetPosition, WorldSpace aSize) => GeneratePathThreadSafe(aStartPosition, aTargetPosition, aSize);

        public static void RequestPath(WorldSpace aStartPosition, WorldSpace aTargetPosition, WorldSpace aSize, Action<Path> onComplete)
        {
            if (onComplete == null) return;
            if (!WorkerPool.IsRunning)
            {
                onComplete(GeneratePathThreadSafe(aStartPosition, aTargetPosition, aSize));
                return;
            }

            WorkerPool.Enqueue(() => GeneratePathThreadSafe(aStartPosition, aTargetPosition, aSize), onComplete);
        }

        

        public static Tile GetTileUnder(WorldSpace aWorldSpace)
        {
            Chunk c = GetChunkUnder(aWorldSpace);
            int x = (int)Math.Floor((aWorldSpace.X - c.Position.X) / TileSize.X);
            int y = (int)Math.Floor((aWorldSpace.Y - c.Position.Y) / TileSize.Y);
            Tile t = c.Tile(x, y);
            Debug.Assert(t != null);
            //Tile t = tiles[(int)Math.Floor(aWorldSpace.X / TileSize.X), (int)Math.Floor(aWorldSpace.Y / TileSize.Y)];
            return t;
        }

        public static Chunk GetChunkUnder(WorldSpace aWorldSpace)
        {
            int id = GetChunkId((int)MathF.Floor(aWorldSpace.X / TileSize.X / Chunk.ChunkSize.X), (int)MathF.Floor(aWorldSpace.Y / TileSize.Y / Chunk.ChunkSize.Y));
            return chunks.Find(x => x.Id == id);
        }

        public static WorldSpace FindClosestWalkableWorldSpace(WorldSpace aWorldSpace, WorldSpace aSize)
        {
            if (GetTileUnder(aWorldSpace).Walkable)
            {
                return aWorldSpace;
            }

            Tile closestTile = FindClosestWalkableTile(aWorldSpace);

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

        public static Tile FindClosestWalkableTile(WorldSpace aWorldSpace)
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


        public static Tile[,] GetSurroundingTiles(Tile aTile)
        {
            Tile[,] a = new Tile[sizeOfSquareToCheck, sizeOfSquareToCheck];
            int chunkX = (int)MathF.Floor((float)aTile.WorldRectangle.Center.X / Chunk.ChunkSize.X / TileSize.X);
            int chunkY = (int)MathF.Floor((float)aTile.WorldRectangle.Center.Y / Chunk.ChunkSize.Y / TileSize.Y);
            Chunk middleChunk = GetChunk(chunkX, chunkY);

            for (int i = 0; i < sizeOfSquareToCheck; i++)
            {
                for (int j = 0; j < sizeOfSquareToCheck; j++)
                {
                    int x = aTile.GridPos.X - sizeOfSquareToCheck / 2 + i;
                    int y = aTile.GridPos.Y - sizeOfSquareToCheck / 2 + j;
                    Chunk chunkToGetTileFrom = middleChunk;

                    if (x < 0)
                    {
                        chunkToGetTileFrom = GetChunk(chunkToGetTileFrom.ChunkPosition + new Point(-1, 0));
                        x += Chunk.ChunkSize.X;
                    }
                    else if (x >= Chunk.ChunkSize.X)
                    {
                        chunkToGetTileFrom = GetChunk(chunkToGetTileFrom.ChunkPosition + new Point(1, 0));
                        x -= Chunk.ChunkSize.X; 
                    }

                    if (y < 0)
                    {
                        chunkToGetTileFrom = GetChunk(chunkToGetTileFrom.ChunkPosition + new Point(0, -1));
                        y += Chunk.ChunkSize.Y;
                    }
                    else if (y >= Chunk.ChunkSize.Y)
                    {
                        chunkToGetTileFrom = GetChunk(chunkToGetTileFrom.ChunkPosition + new Point(0, 1));
                        y -= Chunk.ChunkSize.Y;
                    }

                    a[i, j] = chunkToGetTileFrom.Tile(x, y);
                }
            }
            return a;
        }

        

        public static Tile[] GetTilesAroundPosition(WorldSpace aPosition, float aDistance)
        {
            List<Tile> returnable = new List<Tile>();
            Tile midTile = GetTileUnder(aPosition);
            float distanceInTiles = aDistance / TileSize.X;
            int tilesAround = (int)(distanceInTiles * Math.PI) * 2;

            int chunkX = (int)MathF.Floor((float)midTile.WorldRectangle.Center.X / Chunk.ChunkSize.X / TileSize.X);
            int chunkY = (int)MathF.Floor((float)midTile.WorldRectangle.Center.Y / Chunk.ChunkSize.Y / TileSize.Y);
            Chunk middleChunk = GetChunk(chunkX, chunkY);

            for (int i = 0; i < tilesAround; i++)
            {
                Point tilePos = midTile.GridPos;

                int x = (int)Math.Round(tilePos.X + distanceInTiles * Math.Sin(i * 2 * Math.PI / tilesAround));
                int y = (int)Math.Floor(tilePos.Y + distanceInTiles * Math.Cos(i * 2 * Math.PI / tilesAround));

                Chunk chunkToGetTileFrom = middleChunk;

                if (x < 0)
                {
                    chunkToGetTileFrom = GetChunk(chunkToGetTileFrom.ChunkPosition + new Point(-1, 0));
                    x += Chunk.ChunkSize.X;
                }
                else if (x >= Chunk.ChunkSize.X)
                {
                    chunkToGetTileFrom = GetChunk(chunkToGetTileFrom.ChunkPosition + new Point(1, 0));
                    x -= Chunk.ChunkSize.X;
                }

                if (y < 0)
                {
                    chunkToGetTileFrom = GetChunk(chunkToGetTileFrom.ChunkPosition + new Point(0, -1));
                    y += Chunk.ChunkSize.Y;
                }
                else if (y >= Chunk.ChunkSize.Y)
                {
                    chunkToGetTileFrom = GetChunk(chunkToGetTileFrom.ChunkPosition + new Point(0, 1));
                    y -= Chunk.ChunkSize.Y;
                }

                Tile t = chunkToGetTileFrom.Tile(x, y);

                if (t == null)
                {
                    DebugManager.Print(typeof(TileManager), "Tried to get tile from chunk " + chunkToGetTileFrom.ToString() + "but it returned null.");
                    continue;
                }

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
            ThreadAffinity.AssertMainThread();
            Chunk[] snapshot = renderChunks;
            for (int i = 0; i < snapshot.Length; i++)
            {
                //TODO: Boundscheck before drawing
                snapshot[i].MinimapDraw(aBatch, aOrigin, aMinimapOffset, aSize);
            }

        }

        public static void Draw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            Chunk[] snapshot = renderChunks;
            for (int i = 0; i < snapshot.Length; i++)
            {
                Chunk chunk = snapshot[i];
                if (!Camera.Camera.WorldspaceBoundsCheck(chunk.WorldRectangle)) continue;
                chunk.Draw(aBatch);
            }
        }

        internal static void BuildRenderSnapshot()
        {
            renderChunks = chunks.ToArray();
        }
    }
}
