using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;

namespace Project_1.Tiles
{
    /// <summary>
    /// Render-side cache for tile-derived GPU resources (transparency maps, etc.).
    /// Main-thread only for GPU operations; sim thread prepares data snapshots.
    /// </summary>
    internal static class TileRenderCache
    {
        static Tile cachedCentreTile;
        static Texture2D transparencyMap;
        static volatile Color[] pendingTransparencyData;
        static int pendingTransparencyVersion;
        static int appliedTransparencyVersion;
        static volatile int pendingTransparencyOriginX;
        static volatile int pendingTransparencyOriginY;
        static int appliedTransparencyOriginX;
        static int appliedTransparencyOriginY;
        static bool initialized;
        const int TransparencySize = 65; // matches HLSL in TestDarkness.fx
        static readonly Dictionary<int, Texture2D> minimapTargets = new Dictionary<int, Texture2D>();
        static readonly ConcurrentQueue<ChunkMinimapSnapshot> pendingMinimapSnapshots = new ConcurrentQueue<ChunkMinimapSnapshot>();
        static readonly HashSet<int> publishedMinimapIds = new HashSet<int>();

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;
            transparencyMap = GraphicsManager.CreateNewTexture(new Point(TransparencySize));
            transparencyMap.SetData(new Color[TransparencySize * TransparencySize]);
        }

        public static void BuildTransparencySnapshot(WorldSpace origin)
        {
            ThreadAffinity.AssertSimThread();
            Chunk centreChunk = TileManager.GetChunkUnder(origin);
            if (centreChunk == null)
            {
                PublishBlankTransparency();
                cachedCentreTile = null;
                return;
            }

            Tile centre = TileManager.GetTile(origin);
            if (cachedCentreTile != null && cachedCentreTile == centre)
            {
                return;
            }

            cachedCentreTile = centre;
            Color[] data = new Color[TransparencySize * TransparencySize];
            Point centreGrid = TileManager.GetGridPos(origin);
            pendingTransparencyOriginX = centreGrid.X;
            pendingTransparencyOriginY = centreGrid.Y;
            for (int x = 0; x < TransparencySize; x++)
            {
                for (int y = 0; y < TransparencySize; y++)
                {
                    int gridX = centreGrid.X + (x - TransparencySize / 2);
                    int gridY = centreGrid.Y + (y - TransparencySize / 2);
                    Tile tile = TileManager.GetTileAtGrid(new Point(gridX, gridY));
                    if (tile == null)
                    {
                        data[y * TransparencySize + x] = new Color(0, 0, 0, 0);
                        continue;
                    }
                    data[y * TransparencySize + x] = tile.Transparent ? new Color(0, 0, 0, 0) : new Color(1, 1, 1, 1);
                }
            }

            pendingTransparencyData = data;
            System.Threading.Interlocked.Increment(ref pendingTransparencyVersion);
        }

        public static Texture2D GetTransparencyMapTexture()
        {
            ThreadAffinity.AssertMainThread();
            if (!initialized) Init();
            int pending = System.Threading.Volatile.Read(ref pendingTransparencyVersion);
            if (pending != appliedTransparencyVersion)
            {
                Color[] data = pendingTransparencyData;
                if (data != null)
                {
                    transparencyMap.SetData(data);
                    appliedTransparencyVersion = pending;
                    appliedTransparencyOriginX = pendingTransparencyOriginX;
                    appliedTransparencyOriginY = pendingTransparencyOriginY;
                }
            }
            return transparencyMap;
        }

        public static Vector2 GetTransparencyOriginTile()
        {
            return new Vector2(appliedTransparencyOriginX, appliedTransparencyOriginY);
        }

        static void PublishBlankTransparency()
        {
            Color[] data = new Color[TransparencySize * TransparencySize];
            pendingTransparencyData = data;
            System.Threading.Interlocked.Increment(ref pendingTransparencyVersion);
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
