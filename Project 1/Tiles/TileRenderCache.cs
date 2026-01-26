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
        static bool initialized;
        const int TransparencySize = 65; // matches HLSL in TestDarkness.fx
        static readonly Dictionary<int, Texture2D> minimapTargets = new Dictionary<int, Texture2D>();

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

            Tile centre = TileManager.GetTileUnder(origin);
            if (cachedCentreTile != null && cachedCentreTile == centre)
            {
                return;
            }

            cachedCentreTile = centre;
            Color[] data = new Color[TransparencySize * TransparencySize];
            for (int x = 0; x < TransparencySize; x++)
            {
                for (int y = 0; y < TransparencySize; y++)
                {
                    WorldSpace samplePos = origin + new WorldSpace(TileManager.TileSize.X * (x - TransparencySize / 2), TileManager.TileSize.Y * (y - TransparencySize / 2));
                    if (!TileManager.TryGetTileAt(samplePos, out Tile tile) || tile == null)
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
                }
            }
            return transparencyMap;
        }

        static void PublishBlankTransparency()
        {
            Color[] data = new Color[TransparencySize * TransparencySize];
            pendingTransparencyData = data;
            System.Threading.Interlocked.Increment(ref pendingTransparencyVersion);
        }

        public static Texture2D GetChunkMinimap(Chunk chunk)
        {
            ThreadAffinity.AssertMainThread();

            if (minimapTargets.TryGetValue(chunk.Id, out var cached))
            {
                return cached;
            }

            Texture2D rt = GraphicsManager.CreateRenderTarget(Chunk.ChunkSize);
            Color[] colors = new Color[Chunk.ChunkSize.X * Chunk.ChunkSize.Y];
            chunk.FillMinimapColors(colors);
            rt.SetData(colors);
            minimapTargets[chunk.Id] = rt;
            return rt;
        }
    }
}
