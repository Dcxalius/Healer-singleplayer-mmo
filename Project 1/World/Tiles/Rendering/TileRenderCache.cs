using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;

namespace Project_1.Tiles
{
    /// <summary>
    /// Render-side cache for tile-derived GPU resources.
    /// Main-thread only for GPU operations; sim thread prepares data snapshots.
    /// </summary>
    internal static class TileRenderCache
    {
        static readonly Dictionary<int, Texture2D> minimapTargets = new Dictionary<int, Texture2D>();
        static readonly ConcurrentQueue<ChunkMinimapSnapshot> pendingMinimapSnapshots = new ConcurrentQueue<ChunkMinimapSnapshot>();
        static readonly HashSet<int> publishedMinimapIds = new HashSet<int>();
        static bool initialized;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;
        }

        public static void PublishChunkMinimapSnapshot(Chunk chunk)
        {
            ThreadAffinity.AssertSimThread();
            if (chunk == null) return;
            if (!publishedMinimapIds.Add(chunk.Id)) return;

            Color[] colors = new Color[Chunk.ChunkSize.X * Chunk.ChunkSize.Y];
            chunk.FillMinimapColors(colors);
            pendingMinimapSnapshots.Enqueue(new ChunkMinimapSnapshot(chunk.Id, colors));
        }

        public static void FlushMinimapSnapshots()
        {
            ThreadAffinity.AssertMainThread();
            while (pendingMinimapSnapshots.TryDequeue(out ChunkMinimapSnapshot snapshot))
            {
                Texture2D rt = GraphicsManager.CreateRenderTarget(Chunk.ChunkSize);
                rt.SetData(snapshot.Colors);
                if (minimapTargets.TryGetValue(snapshot.ChunkId, out var existing))
                {
                    existing.Dispose();
                }
                minimapTargets[snapshot.ChunkId] = rt;
            }
        }

        public static Texture2D GetChunkMinimap(int chunkId)
        {
            ThreadAffinity.AssertMainThread();
            if (minimapTargets.TryGetValue(chunkId, out var cached))
            {
                return cached;
            }
            return null;
        }

        public static void ResetMinimapSnapshotTracking()
        {
            ThreadAffinity.AssertSimThread();
            publishedMinimapIds.Clear();
            while (pendingMinimapSnapshots.TryDequeue(out _)) { }
        }

        readonly struct ChunkMinimapSnapshot
        {
            public ChunkMinimapSnapshot(int chunkId, Color[] colors)
            {
                ChunkId = chunkId;
                Colors = colors;
            }

            public int ChunkId { get; }
            public Color[] Colors { get; }
        }
    }
}
