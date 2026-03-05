using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Managers;

namespace Project_1.Tiles
{
    /// <summary>
    /// Render-side cache for tile-derived GPU resources (shadow map, minimap textures, etc.).
    /// Main-thread only for GPU operations; sim thread prepares data snapshots.
    /// </summary>
    internal static class TileRenderCache
    {
        const int MinShadowSizeTiles = 65;
        const int ShadowCoverageSafetyTiles = 4;
        static Texture2D shadowMap;
        static readonly object shadowLock = new object();
        static Color[] pendingShadowData;
        static Color[] shadowWorkData;
        static int shadowWorkSize = MinShadowSizeTiles;
        static int pendingShadowSize = MinShadowSizeTiles;
        static int appliedShadowSize = MinShadowSizeTiles;
        static int pendingShadowVersion;
        static int appliedShadowVersion;
        static int pendingShadowOriginX;
        static int pendingShadowOriginY;
        static int appliedShadowOriginX;
        static int appliedShadowOriginY;
        static float[] shadowBrightnessScratch;
        static bool initialized;
        const float ShadowFullBrightnessDistanceWorld = 200f;
        const float ShadowMaxDistanceWorld = 500f;
        static readonly Dictionary<int, Texture2D> minimapTargets = new Dictionary<int, Texture2D>();
        static readonly ConcurrentQueue<ChunkMinimapSnapshot> pendingMinimapSnapshots = new ConcurrentQueue<ChunkMinimapSnapshot>();
        static readonly HashSet<int> publishedMinimapIds = new HashSet<int>();

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;
            EnsureShadowBuffers(shadowWorkSize);
            shadowMap = GraphicsManager.CreateNewTexture(new Point(shadowWorkSize));
            lock (shadowLock)
            {
                shadowMap.SetData(pendingShadowData);
            }
        }

        public static int ShadowMapSize => appliedShadowSize;

        public static void BuildShadowSnapshot(ObjectManager.PartyLightSnapshot lightSnapshot)
        {
            ThreadAffinity.AssertSimThread();
            Point originTile = ResolveShadowOriginTile(lightSnapshot);
            int requiredSize = CalculateRequiredShadowSize(originTile);
            EnsureShadowBuffers(requiredSize);
            EnsureShadowBrightnessScratch();

            int shadowSize = shadowWorkSize;
            int mapMinX = originTile.X - shadowSize / 2;
            int mapMinY = originTile.Y - shadowSize / 2;

            Array.Clear(shadowBrightnessScratch, 0, shadowBrightnessScratch.Length);

            if (lightSnapshot != null && lightSnapshot.Count > 0)
            {
                int radiusTiles = Math.Max(1, (int)MathF.Ceiling(ShadowMaxDistanceWorld / Tile.Size.X));
                for (int i = 0; i < lightSnapshot.Count; i++)
                {
                    WorldSpace lightPosition = lightSnapshot.GetPosition(i);
                    Point lightTile = TileManager.GetGridPos(lightPosition);
                    RecursiveShadowcaster.ComputeVisibleTiles(lightTile, radiusTiles, (visibleTile, distanceSquared) =>
                    {
                        int localX = visibleTile.X - mapMinX;
                        int localY = visibleTile.Y - mapMinY;
                        if ((uint)localX >= shadowSize || (uint)localY >= shadowSize) return;

                        float distanceWorld = MathF.Sqrt(distanceSquared) * Tile.Size.X;
                        float brightness = DistanceToBrightness(distanceWorld);
                        int index = localY * shadowSize + localX;
                        if (brightness > shadowBrightnessScratch[index])
                        {
                            shadowBrightnessScratch[index] = brightness;
                        }
                    });
                }
            }

            for (int i = 0; i < shadowWorkData.Length; i++)
            {
                float clampedBrightness = Math.Clamp(shadowBrightnessScratch[i], 0f, 1f);
                byte alpha = (byte)Math.Clamp((int)MathF.Round((1f - clampedBrightness) * byte.MaxValue), 0, byte.MaxValue);
                shadowWorkData[i] = new Color((byte)0, (byte)0, (byte)0, alpha);
            }

            PublishShadowBuffer(originTile.X, originTile.Y);
        }

        public static void PublishShadowSnapshot(Color[] data, Point originTile)
        {
            ThreadAffinity.AssertSimThread();
            EnsureShadowBuffers(shadowWorkSize);
            if (data == null)
            {
                PublishBlankShadow(originTile.X, originTile.Y);
                return;
            }

            int copyLength = Math.Min(data.Length, shadowWorkData.Length);
            Array.Copy(data, shadowWorkData, copyLength);
            if (copyLength < shadowWorkData.Length)
            {
                Array.Clear(shadowWorkData, copyLength, shadowWorkData.Length - copyLength);
            }

            PublishShadowBuffer(originTile.X, originTile.Y);
        }

        public static Texture2D GetShadowMapTexture()
        {
            ThreadAffinity.AssertMainThread();
            if (!initialized) Init();
            lock (shadowLock)
            {
                if (pendingShadowVersion != appliedShadowVersion)
                {
                    if (shadowMap == null || shadowMap.Width != pendingShadowSize || shadowMap.Height != pendingShadowSize)
                    {
                        shadowMap?.Dispose();
                        shadowMap = GraphicsManager.CreateNewTexture(new Point(pendingShadowSize));
                    }
                    shadowMap.SetData(pendingShadowData);
                    appliedShadowVersion = pendingShadowVersion;
                    appliedShadowSize = pendingShadowSize;
                    appliedShadowOriginX = pendingShadowOriginX;
                    appliedShadowOriginY = pendingShadowOriginY;
                }
            }
            return shadowMap;
        }

        public static Vector2 GetShadowOriginTile()
        {
            return new Vector2(appliedShadowOriginX, appliedShadowOriginY);
        }

        static void PublishBlankShadow(int originX, int originY)
        {
            EnsureShadowBuffers(shadowWorkSize);
            Array.Clear(shadowWorkData, 0, shadowWorkData.Length);
            PublishShadowBuffer(originX, originY);
        }

        static void EnsureShadowBuffers(int requiredSize)
        {
            int normalizedSize = NormalizeShadowSize(requiredSize);
            lock (shadowLock)
            {
                if (pendingShadowData != null && shadowWorkData != null && normalizedSize <= shadowWorkSize) return;

                shadowWorkSize = normalizedSize;
                pendingShadowSize = normalizedSize;
                pendingShadowData = new Color[normalizedSize * normalizedSize];
                shadowWorkData = new Color[normalizedSize * normalizedSize];
                shadowBrightnessScratch = new float[normalizedSize * normalizedSize];
            }
        }

        static void EnsureShadowBrightnessScratch()
        {
            if (shadowBrightnessScratch != null) return;
            lock (shadowLock)
            {
                if (shadowBrightnessScratch == null) shadowBrightnessScratch = new float[shadowWorkSize * shadowWorkSize];
            }
        }

        static float DistanceToBrightness(float distanceWorld)
        {
            if (distanceWorld <= ShadowFullBrightnessDistanceWorld) return 1f;
            if (distanceWorld >= ShadowMaxDistanceWorld) return 0f;
            float ratio = (distanceWorld - ShadowFullBrightnessDistanceWorld) / (ShadowMaxDistanceWorld - ShadowFullBrightnessDistanceWorld);
            return 1f - Math.Clamp(ratio, 0f, 1f);
        }

        static Point ResolveShadowOriginTile(ObjectManager.PartyLightSnapshot lightSnapshot)
        {
            if (lightSnapshot != null && lightSnapshot.Count > 0)
            {
                return lightSnapshot.OriginTile;
            }
            return TileManager.GetGridPos(Camera.Camera.CentreInWorldSpace);
        }

        static int CalculateRequiredShadowSize(Point originTile)
        {
            Rectangle worldRect = Camera.Camera.WorldRectangle;
            WorldSpace originWorld = new WorldSpace((originTile.X + 0.5f) * Tile.Size.X, (originTile.Y + 0.5f) * Tile.Size.Y);
            WorldSpace cameraCentre = Camera.Camera.CentreInWorldSpace;

            float offsetX = MathF.Abs(cameraCentre.X - originWorld.X);
            float offsetY = MathF.Abs(cameraCentre.Y - originWorld.Y);

            float requiredWorldWidth = worldRect.Width + offsetX * 2f;
            float requiredWorldHeight = worldRect.Height + offsetY * 2f;

            int sizeX = (int)MathF.Ceiling(requiredWorldWidth / Tile.Size.X) + ShadowCoverageSafetyTiles;
            int sizeY = (int)MathF.Ceiling(requiredWorldHeight / Tile.Size.Y) + ShadowCoverageSafetyTiles;
            return Math.Max(sizeX, sizeY);
        }

        static int NormalizeShadowSize(int requested)
        {
            int size = Math.Max(MinShadowSizeTiles, requested);
            if ((size & 1) == 0) size++;
            return size;
        }

        static void PublishShadowBuffer(int originX, int originY)
        {
            lock (shadowLock)
            {
                Color[] swap = pendingShadowData;
                pendingShadowData = shadowWorkData;
                shadowWorkData = swap;
                pendingShadowOriginX = originX;
                pendingShadowOriginY = originY;
                pendingShadowVersion++;
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
