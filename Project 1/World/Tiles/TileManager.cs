using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.DebugTools;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Doodads;
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
using Project_1.GameObjects.Entities.Friendlies.Players;

namespace Project_1.Tiles
{
    internal static partial class TileManager
    {
        public static Tile GetTile(WorldSpace aSpace)
        {
            Chunk chunk = GetChunk(aSpace);
            Debug.Assert(chunk != null, "GetTile called for an unloaded chunk.");
            if (chunk == null) return null;
            return GetTile(chunk, Modulo((int)MathF.Floor(aSpace.X / Tile.Size.X), Chunk.ChunkSize.X), Modulo((int)MathF.Floor(aSpace.Y / Tile.Size.Y), Chunk.ChunkSize.Y));
        }
        public static Tile GetTile(Chunk aChunk, int aX, int aY)
        {
            Debug.Assert(aChunk != null, "GetTile called with a null chunk.");
            return aChunk?.Tile(aX, aY);
        }
        public static Tile GetTile(int aChunkId, int aX, int aY)
        {
            if (!chunks.TryGetValue(aChunkId, out Chunk chunk))
            {
                Debug.Assert(false, $"GetTile called for missing chunk id {aChunkId}.");
                return null;
            }
            return chunk.Tile(aX, aY);
        }
        public static bool TryGetTile(WorldSpace aSpace, out Tile tile)
        {
            Chunk chunk = GetChunk(aSpace);
            if (chunk == null)
            {
                tile = null;
                return false;
            }
            int x = Modulo((int)MathF.Floor(aSpace.X / Tile.Size.X), Chunk.ChunkSize.X);
            int y = Modulo((int)MathF.Floor(aSpace.Y / Tile.Size.Y), Chunk.ChunkSize.Y);
            tile = chunk.Tile(x, y);
            return tile != null;
        }

        public static Chunk GetChunk(int aId) => chunks.TryGetValue(aId, out Chunk chunk) ? chunk : null;

        static Dictionary<int, Chunk> chunks;
        static readonly RenderCache<ChunkRenderSnapshot> renderChunks = new RenderCache<ChunkRenderSnapshot>();
        static readonly HashSet<int> knownChunkIds = new HashSet<int>();
        static readonly HashSet<int> currentChunkIds = new HashSet<int>();
        static readonly ReaderWriterLockSlim chunkLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        static readonly ConcurrentDictionary<int, Chunk> unpublishedChunks = new ConcurrentDictionary<int, Chunk>();
        static readonly ConcurrentDictionary<int, ChunkBuildJob> activeChunkBuildJobs = new ConcurrentDictionary<int, ChunkBuildJob>();
        static readonly List<Chunk> chunkSnapshotScratch = new List<Chunk>();
        static readonly ChunkIdComparer chunkIdComparer = new ChunkIdComparer();
        static int chunkBuildEpoch;

        public static CollisionManager CollisionManager;


        static PathFinder pathFinder = new PathFinder();

        static bool initialized;

        sealed class ChunkIdComparer : IComparer<Chunk>
        {
            public int Compare(Chunk x, Chunk y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                return x.Id.CompareTo(y.Id);
            }
        }

        sealed class ChunkBuildJob
        {
            public ChunkBuildJob(int epoch)
            {
                Epoch = epoch;
                Completion = new TaskCompletionSource<Chunk>(TaskCreationOptions.RunContinuationsAsynchronously);
            }

            public int Epoch { get; }
            public TaskCompletionSource<Chunk> Completion { get; }
        }

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;
            chunks = new Dictionary<int, Chunk>();
            CollisionManager = new CollisionManager();
        }

        public static void Update()
        {
            ThreadAffinity.AssertSimThread();
            const int surroundingChunkCheckSize = 3; // this should always be odd
            Debug.Assert(surroundingChunkCheckSize % 2 == 1);
            const int maxQueuedPrefetch = 4;
            int centreChunkId = Chunk.GetChunkId(
                (int)MathF.Floor(ObjectManager.Player.FeetPosition.X / Tile.Size.X / Chunk.ChunkSize.X),
                (int)MathF.Floor(ObjectManager.Player.FeetPosition.Y / Tile.Size.Y / Chunk.ChunkSize.Y));
            Chunk centreChunk = EnsureChunkLoaded(centreChunkId);
            Point centreChunkPos = centreChunk.ChunkPosition;
            int queuedPrefetch = 0;
            int immediateRadius = surroundingChunkCheckSize / 2;
            int prefetchRadius = immediateRadius + 1;

            for (int x = -immediateRadius; x <= immediateRadius; x++)
            {
                for (int y = -immediateRadius; y <= immediateRadius; y++)
                {
                    if (x == 0 && y == 0) continue;
                    int newId = Chunk.GetChunkId(centreChunkPos + new Point(x, y));
                    EnsureChunkLoaded(newId);
                }
            }

            if (WorkerPool.IsRunning)
            {
                for (int x = -prefetchRadius; x <= prefetchRadius && queuedPrefetch < maxQueuedPrefetch; x++)
                {
                    for (int y = -prefetchRadius; y <= prefetchRadius && queuedPrefetch < maxQueuedPrefetch; y++)
                    {
                        if (Math.Abs(x) <= immediateRadius && Math.Abs(y) <= immediateRadius) continue;
                        int id = Chunk.GetChunkId(centreChunkPos + new Point(x, y));
                        if (IsChunkAvailable(id)) continue;

                        GetOrQueueChunkBuild(id);
                        queuedPrefetch++;
                    }
                }
            }

            UpdateChunkDoodads();
        }

        public static void New()
        {
            ThreadAffinity.AssertSimThread();
            TileRenderCache.ResetMinimapSnapshotTracking();
            ClearRenderCache();
            chunks.Clear();
            ResetChunkBuildState();
            chunks[0] = CreateStructuredChunk(0, Chunk.GenerateTileIds(0));
        }

        public static void Load(Save aSave)
        {
            ThreadAffinity.AssertSimThread();
            TileRenderCache.ResetMinimapSnapshotTracking();
            ClearRenderCache();
            chunks.Clear();
            ResetChunkBuildState();

            string[] files = System.IO.Directory.GetFiles(aSave.Tiles);
            for (int i = 0; i < files.Length; i++)
            {
                string json = System.IO.File.ReadAllText(files[i]);
                Chunk c = SaveManager.ImportData<Chunk>(json);
                //int[,] tileIds = JsonConvert.DeserializeObject<int[,]>(json);
                int id = int.Parse(SaveManager.TrimToNameOnly(files[i]));
                chunks[id] = c;
                EnsureStructureDoodadsForLoadedChunk(c);

                
            }
        }

        public static void LoadFromChunks(List<Chunk> loadedChunks)
        {
            ThreadAffinity.AssertSimThread();
            TileRenderCache.ResetMinimapSnapshotTracking();
            ClearRenderCache();
            chunks.Clear();
            ResetChunkBuildState();
            if (loadedChunks != null && loadedChunks.Count > 0)
            {
                for (int i = 0; i < loadedChunks.Count; i++)
                {
                    Chunk chunk = loadedChunks[i];
                    if (chunk == null) continue;
                    chunks[chunk.Id] = chunk;
                    EnsureStructureDoodadsForLoadedChunk(chunk);
                }
            }
        }

        static ChunkBuildJob GetOrQueueChunkBuild(int chunkId)
        {
            while (true)
            {
                if (activeChunkBuildJobs.TryGetValue(chunkId, out ChunkBuildJob existingJob)) return existingJob;

                ChunkBuildJob newJob = new ChunkBuildJob(Volatile.Read(ref chunkBuildEpoch));
                if (!activeChunkBuildJobs.TryAdd(chunkId, newJob))
                {
                    continue;
                }

                WorkerPool.Enqueue(() => BuildChunkOnWorker(chunkId, newJob));
                return newJob;
            }
        }

        static void BuildChunkOnWorker(int chunkId, ChunkBuildJob buildJob)
        {
            try
            {
                Chunk chunk = CreateStructuredChunk(chunkId, Chunk.GenerateTileIds(chunkId));
                if (buildJob.Epoch == Volatile.Read(ref chunkBuildEpoch))
                {
                    unpublishedChunks[chunkId] = chunk;
                }
                buildJob.Completion.TrySetResult(chunk);
            }
            catch (Exception ex)
            {
                buildJob.Completion.TrySetException(ex);
            }
            finally
            {
                activeChunkBuildJobs.TryRemove(new KeyValuePair<int, ChunkBuildJob>(chunkId, buildJob));
            }
        }

        public static float GetDragCoeficient(WorldSpace aFeetPos) => GetTile(aFeetPos).DragCoeficient;

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

        public static void RequestPath(WorldSpace aStartPosition, WorldSpace aTargetPosition, WorldSpace aSize, Action<Path> onComplete)
        {
            ThreadAffinity.AssertSimThread();
            if (onComplete == null) return;
            void Complete(Path path)
            {
                ThreadAffinity.AssertSimThread();
                onComplete(path);
            }
            if (!WorkerPool.IsRunning)
            {
                Complete(GeneratePathThreadSafe(aStartPosition, aTargetPosition, aSize));
                return;
            }

            WorkerPool.Enqueue(() => GeneratePathThreadSafe(aStartPosition, aTargetPosition, aSize), Complete);
        }

        public static void SaveData(Save aSave)
        {
            ThreadAffinity.AssertSimThread();


            foreach (Chunk chunk in chunks.Values)
            {
                SaveManager.ExportData(aSave.Tiles + "\\" + chunk.Id + ".tilemap", chunk);
            }

        }

        static void UpdateChunkDoodads()
        {
            chunkLock.EnterReadLock();
            try
            {
                foreach (Chunk chunk in chunks.Values)
                {
                    chunk?.Doodads?.Update();
                }
            }
            finally
            {
                chunkLock.ExitReadLock();
            }
        }

        public static bool TryGetDoodadAt(WorldSpace worldPos, out Doodad doodad)
        {
            ThreadAffinity.AssertSimThread();
            doodad = null;
            chunkLock.EnterReadLock();
            try
            {
                foreach (Chunk chunk in chunks.Values)
                {
                    if (chunk?.Doodads == null) continue;
                    if (!chunk.WorldRectangle.Contains(worldPos.ToPoint())) continue;
                    if (chunk.Doodads.TryGetDoodadAt(worldPos, out doodad)) return true;
                }
            }
            finally
            {
                chunkLock.ExitReadLock();
            }

            return false;
        }

        public static bool TryGetDoodadByRenderId(int renderId, out Doodad doodad)
        {
            ThreadAffinity.AssertSimThread();
            doodad = null;
            chunkLock.EnterReadLock();
            try
            {
                foreach (Chunk chunk in chunks.Values)
                {
                    if (chunk?.Doodads == null) continue;
                    if (chunk.Doodads.TryGetDoodadByRenderId(renderId, out doodad)) return true;
                }
            }
            finally
            {
                chunkLock.ExitReadLock();
            }

            return false;
        }

        internal static void DrawMinimapSnapshots(SpriteBatch aBatch, WorldSpace aOrigin, AbsoluteScreenPosition aMinimapOffset, AbsoluteScreenPosition aSize)
        {
            ThreadAffinity.AssertMainThread();
            TileRenderCache.FlushMinimapSnapshots();
            renderChunks.ApplyUpdates();
            foreach (ChunkRenderSnapshot snapshot in renderChunks.Values)
            {
                //TODO: Boundscheck before drawing
                snapshot.MinimapDraw(aBatch, aOrigin, aMinimapOffset, aSize);
            }

        }
        internal static void DrawSnapshots(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            // Snapshot-only draw path. Do not read live sim chunk data here.
            renderChunks.ApplyUpdates();
            foreach (ChunkRenderSnapshot chunk in renderChunks.Values)
            {
                if (!Camera.Camera.WorldspaceBoundsCheck(chunk.WorldRectangle)) continue;
                chunk.Draw(aBatch);
            }
        }

        internal static void DrawDoodadSnapshots(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            chunkLock.EnterReadLock();
            try
            {
                foreach (Chunk chunk in chunks.Values)
                {
                    chunk?.Doodads?.DrawSnapshots(aBatch);
                }
            }
            finally
            {
                chunkLock.ExitReadLock();
            }
        }

        internal static void BuildRenderSnapshot()
        {
            //TODO: Should this really sort?
            //Also shouldn't it just send the ChunkSize / Minimap surrounding the player?
            ThreadAffinity.AssertSimThread();
            chunkSnapshotScratch.Clear();
            foreach (Chunk chunk in chunks.Values)
            {
                if (chunk == null) continue;
                chunkSnapshotScratch.Add(chunk);
            }
            chunkSnapshotScratch.Sort(chunkIdComparer);
            currentChunkIds.Clear();
            for (int i = 0; i < chunkSnapshotScratch.Count; i++)
            {
                Chunk chunk = chunkSnapshotScratch[i];
                TileRenderCache.PublishChunkMinimapSnapshot(chunk);
                ChunkRenderSnapshot snapshot = chunk.BuildRenderSnapshot();
                renderChunks.EnqueueUpdate(snapshot);
                currentChunkIds.Add(snapshot.RenderId);
            }
            PublishRemovals();
        }

        internal static void BuildDoodadRenderSnapshots()
        {
            ThreadAffinity.AssertSimThread();
            chunkLock.EnterReadLock();
            try
            {
                foreach (Chunk chunk in chunks.Values)
                {
                    chunk?.Doodads?.BuildRenderSnapshot();
                }
            }
            finally
            {
                chunkLock.ExitReadLock();
            }
        }

        static void PublishRemovals()
        {
            foreach (int id in knownChunkIds)
            {
                if (!currentChunkIds.Contains(id))
                {
                    renderChunks.EnqueueRemove(id);
                }
            }
            knownChunkIds.Clear();
            foreach (int id in currentChunkIds)
            {
                knownChunkIds.Add(id);
            }
        }

        static void ClearRenderCache()
        {
            renderChunks.RequestClear();
            knownChunkIds.Clear();
            currentChunkIds.Clear();
        }

        static bool IsChunkAvailable(int chunkId)
        {
            chunkLock.EnterReadLock();
            try
            {
                if (chunks.ContainsKey(chunkId)) return true;
            }
            finally
            {
                chunkLock.ExitReadLock();
            }

            if (unpublishedChunks.ContainsKey(chunkId)) return true;
            return activeChunkBuildJobs.ContainsKey(chunkId);
        }

        static bool TryPublishUnpublishedChunk(int chunkId, out Chunk chunk)
        {
            if (!unpublishedChunks.TryRemove(chunkId, out chunk)) return false;
            chunks[chunkId] = chunk;
            return true;
        }

        static void ResetChunkBuildState()
        {
            Interlocked.Increment(ref chunkBuildEpoch);
            unpublishedChunks.Clear();
            foreach (var pair in activeChunkBuildJobs.ToArray())
            {
                if (!activeChunkBuildJobs.TryRemove(pair.Key, out ChunkBuildJob buildJob)) continue;
                buildJob.Completion.TrySetCanceled();
            }
        }

        public static Chunk[] GetChunksSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            if (chunks == null || chunks.Count == 0) return Array.Empty<Chunk>();
            return chunks.Values.ToArray();
        }
    }
}
