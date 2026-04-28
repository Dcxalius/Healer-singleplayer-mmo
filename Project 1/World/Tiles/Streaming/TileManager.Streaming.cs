using Microsoft.Xna.Framework;
using Project_1.GameObjects;
using Project_1.Managers;
using Project_1.Managers.Saves;
using Project_1.WorldGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace Project_1.Tiles
{
    internal static partial class TileManager
    {
        public static void Update()
        {
            ThreadAffinity.AssertSimThread();
            const int surroundingChunkCheckSize = 3;
            Debug.Assert(surroundingChunkCheckSize % 2 == 1);
            const int maxQueuedPrefetch = 4;
            int centreChunkId = ChunkAddressing.GetChunkId(
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
                    int newId = ChunkAddressing.GetChunkId(centreChunkPos + new Point(x, y));
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
                        int id = ChunkAddressing.GetChunkId(centreChunkPos + new Point(x, y));
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
            NodeWorldManager.GenerateNewWorld();
            TileRenderCache.ResetMinimapSnapshotTracking();
            ClearRenderCache();
            chunks.Clear();
            ResetChunkBuildState();
            chunks[0] = CreateStructuredChunk(0, ChunkGenerator.GenerateBlocks(0));
        }

        public static void Load(Save aSave)
        {
            ThreadAffinity.AssertSimThread();
            TileRenderCache.ResetMinimapSnapshotTracking();
            ClearRenderCache();
            chunks.Clear();
            ResetChunkBuildState();

            string[] files = Directory.GetFiles(aSave.Tiles);
            for (int i = 0; i < files.Length; i++)
            {
                string json = File.ReadAllText(files[i]);
                Chunk chunk = SaveManager.ImportData<Chunk>(json);
                int id = int.Parse(SaveManager.TrimToNameOnly(files[i]));
                chunks[id] = chunk;
                EnsureStructureDoodadsForLoadedChunk(chunk);
            }
        }

        public static void LoadFromChunks(List<Chunk> loadedChunks)
        {
            ThreadAffinity.AssertSimThread();
            TileRenderCache.ResetMinimapSnapshotTracking();
            ClearRenderCache();
            chunks.Clear();
            ResetChunkBuildState();
            if (loadedChunks == null || loadedChunks.Count == 0) return;

            for (int i = 0; i < loadedChunks.Count; i++)
            {
                Chunk chunk = loadedChunks[i];
                if (chunk == null) continue;
                chunks[chunk.Id] = chunk;
                EnsureStructureDoodadsForLoadedChunk(chunk);
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
                Chunk chunk = CreateStructuredChunk(chunkId, ChunkGenerator.GenerateBlocks(chunkId));
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

        static Chunk EnsureChunkLoaded(int chunkId)
        {
            chunkLock.EnterUpgradeableReadLock();
            try
            {
                if (chunks.TryGetValue(chunkId, out Chunk existing)) return existing;
                if (unpublishedChunks.ContainsKey(chunkId))
                {
                    chunkLock.EnterWriteLock();
                    try
                    {
                        if (chunks.TryGetValue(chunkId, out existing)) return existing;
                        if (TryPublishUnpublishedChunk(chunkId, out Chunk installedChunk)) return installedChunk;
                    }
                    finally
                    {
                        chunkLock.ExitWriteLock();
                    }
                }

                if (!WorkerPool.IsRunning)
                {
                    chunkLock.EnterWriteLock();
                    try
                    {
                        if (chunks.TryGetValue(chunkId, out existing)) return existing;
                        Chunk synchronousChunk = CreateStructuredChunk(chunkId, ChunkGenerator.GenerateBlocks(chunkId));
                        chunks[chunkId] = synchronousChunk;
                        return synchronousChunk;
                    }
                    finally
                    {
                        chunkLock.ExitWriteLock();
                    }
                }

                ChunkBuildJob buildJob = GetOrQueueChunkBuild(chunkId);
                Chunk builtChunk = buildJob.Completion.Task.GetAwaiter().GetResult();

                chunkLock.EnterWriteLock();
                try
                {
                    if (chunks.TryGetValue(chunkId, out existing)) return existing;
                    if (TryPublishUnpublishedChunk(chunkId, out Chunk installedChunk)) return installedChunk;
                    chunks[chunkId] = builtChunk;
                    return builtChunk;
                }
                finally
                {
                    chunkLock.ExitWriteLock();
                }
            }
            finally
            {
                chunkLock.ExitUpgradeableReadLock();
            }
        }
    }
}
