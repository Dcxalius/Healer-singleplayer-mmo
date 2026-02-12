using System;
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
        static readonly object transparencyLock = new object();
        static Color[] pendingTransparencyData;
        static Color[] transparencyWorkData;
        static int pendingTransparencyVersion;
        static int appliedTransparencyVersion;
        static int pendingTransparencyOriginX;
        static int pendingTransparencyOriginY;
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
            EnsureTransparencyBuffers();
            transparencyMap = GraphicsManager.CreateNewTexture(new Point(TransparencySize));
            lock (transparencyLock)
            {
                transparencyMap.SetData(pendingTransparencyData);
            }
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
            EnsureTransparencyBuffers();
            Color[] data = transparencyWorkData;
            Point centreGrid = TileManager.GetGridPos(origin);
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

            PublishTransparencyBuffer(centreGrid.X, centreGrid.Y);
        }

        public static Texture2D GetTransparencyMapTexture()
        {
            ThreadAffinity.AssertMainThread();
            if (!initialized) Init();
            lock (transparencyLock)
            {
                if (pendingTransparencyVersion != appliedTransparencyVersion)
                {
                    transparencyMap.SetData(pendingTransparencyData);
                    appliedTransparencyVersion = pendingTransparencyVersion;
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
            EnsureTransparencyBuffers();
            Array.Clear(transparencyWorkData, 0, transparencyWorkData.Length);
            PublishTransparencyBuffer(0, 0);
        }

        static void EnsureTransparencyBuffers()
        {
            if (pendingTransparencyData != null && transparencyWorkData != null) return;
            lock (transparencyLock)
            {
                if (pendingTransparencyData == null) pendingTransparencyData = new Color[TransparencySize * TransparencySize];
                if (transparencyWorkData == null) transparencyWorkData = new Color[TransparencySize * TransparencySize];
            }
        }

        static void PublishTransparencyBuffer(int originX, int originY)
        {
            lock (transparencyLock)
            {
                Color[] swap = pendingTransparencyData;
                pendingTransparencyData = transparencyWorkData;
                transparencyWorkData = swap;
                pendingTransparencyOriginX = originX;
                pendingTransparencyOriginY = originY;
                pendingTransparencyVersion++;
            }
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
