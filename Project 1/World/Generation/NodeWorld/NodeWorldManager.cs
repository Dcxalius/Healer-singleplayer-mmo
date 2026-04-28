using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using Project_1.Tiles;

namespace Project_1.WorldGeneration
{
    internal readonly struct ChunkWorldGenData
    {
        public ChunkWorldGenData(
            int continentId,
            Point localChunkInContinent,
            Point localChunkInRegion,
            RegionData region,
            bool isFallbackOcean)
        {
            ContinentId = continentId;
            LocalChunkInContinent = localChunkInContinent;
            LocalChunkInRegion = localChunkInRegion;
            Region = region;
            IsFallbackOcean = isFallbackOcean;
        }

        public int ContinentId { get; }
        public Point LocalChunkInContinent { get; }
        public Point LocalChunkInRegion { get; }
        public RegionData Region { get; }
        public bool IsFallbackOcean { get; }
        public bool IsOceanRegion => IsFallbackOcean || (Region?.IsOceanRegion ?? false);
    }

    internal static class NodeWorldManager
    {
        static WorldGraph currentWorld;

        public static WorldGraph CurrentWorld => currentWorld;
        public static WorldGenerationRecipe CurrentRecipe => currentWorld?.Recipe;

        public static void GenerateNewWorld()
        {
            GenerateNewWorld(new WorldGenerationSettings
            {
                Seed = RandomManager.RollInt()
            });
        }

        public static void GenerateNewWorld(WorldGenerationSettings settings)
        {
            settings ??= new WorldGenerationSettings
            {
                Seed = RandomManager.RollInt()
            };
            currentWorld = NodeWorldGenerator.Generate(settings);
        }

        public static ContinentNode GetStarterContinent()
        {
            return currentWorld?.GetContinent(currentWorld.StartingContinentId);
        }

        public static bool TrySampleBlockLevelBand(int continentId, Point localBlock, out LevelBand levelBand)
        {
            levelBand = default;
            if (currentWorld == null) return false;
            ContinentNode continent = currentWorld.GetContinent(continentId);
            if (continent == null) return false;
            levelBand = NodeWorldGenerator.SampleBlockLevelBand(currentWorld, continent, localBlock);
            return true;
        }

        public static bool TrySampleBlockHeight01(int continentId, Point localBlock, out float height01)
        {
            height01 = 0f;
            if (currentWorld == null) return false;
            ContinentNode continent = currentWorld.GetContinent(continentId);
            if (continent == null) return false;
            height01 = NodeWorldGenerator.SampleBlockHeight01(currentWorld, continent, localBlock);
            return true;
        }

        public static bool TryGetChunkGenerationData(Point chunkPosition, out ChunkWorldGenData data)
        {
            data = default;
            ContinentNode starter = GetStarterContinent();
            if (starter == null) return false;

            Point continentChunkSize = new Point(
                starter.RegionWidth * currentWorld.Settings.RegionSizeInChunks.X,
                starter.RegionHeight * currentWorld.Settings.RegionSizeInChunks.Y);
            Point continentOrigin = new Point(
                -continentChunkSize.X / 2,
                -continentChunkSize.Y / 2);
            Point localChunk = chunkPosition - continentOrigin;
            if (localChunk.X < 0 || localChunk.Y < 0 || localChunk.X >= continentChunkSize.X || localChunk.Y >= continentChunkSize.Y)
            {
                data = new ChunkWorldGenData(starter.Id, localChunk, Point.Zero, null, isFallbackOcean: true);
                return true;
            }

            Point regionCoord = new Point(
                localChunk.X / currentWorld.Settings.RegionSizeInChunks.X,
                localChunk.Y / currentWorld.Settings.RegionSizeInChunks.Y);
            RegionData region = starter.Regions[regionCoord.X, regionCoord.Y];
            Point localChunkInRegion = new Point(
                PositiveModulo(localChunk.X, currentWorld.Settings.RegionSizeInChunks.X),
                PositiveModulo(localChunk.Y, currentWorld.Settings.RegionSizeInChunks.Y));
            data = new ChunkWorldGenData(starter.Id, localChunk, localChunkInRegion, region, isFallbackOcean: false);
            return true;
        }

        public static bool TryGetChunkAverageLevel(Point chunkPosition, out int averageLevel)
        {
            averageLevel = 1;
            if (!TryGetChunkGenerationData(chunkPosition, out ChunkWorldGenData data)) return false;
            if (currentWorld == null) return false;

            Point localBlock = new Point(
                data.LocalChunkInContinent.X * currentWorld.Settings.ChunkFootprintInBlocks.X + currentWorld.Settings.ChunkFootprintInBlocks.X / 2,
                data.LocalChunkInContinent.Y * currentWorld.Settings.ChunkFootprintInBlocks.Y + currentWorld.Settings.ChunkFootprintInBlocks.Y / 2);
            if (!TrySampleBlockLevelBand(data.ContinentId, localBlock, out LevelBand band)) return false;

            averageLevel = (int)MathF.Round((band.MinLevel + band.MaxLevel) * 0.5f);
            return true;
        }

        public static bool TryGenerateChunkTileIds(Point chunkPosition, Point tileGridSize, out int[,] tileIds)
        {
            tileIds = null;
            if (!TryGetChunkGenerationData(chunkPosition, out ChunkWorldGenData data)) return false;

            int grassId = TileFactory.GetTileData("Grass").ID;
            int dirtId = TileFactory.GetTileData("Dirt").ID;
            int wallId = TileFactory.GetTileData("Wall").ID;

            tileIds = new int[tileGridSize.X, tileGridSize.Y];
            for (int x = 0; x < tileGridSize.X; x++)
            {
                for (int y = 0; y < tileGridSize.Y; y++)
                {
                    Point blockSample = GetBlockSample(currentWorld.Settings, data.LocalChunkInContinent, tileGridSize, x, y);
                    int tileId = SelectTileId(data, blockSample, grassId, dirtId, wallId);
                    tileIds[x, y] = tileId;
                }
            }

            return true;
        }

        public static bool TryGetStarterSpawnPoint(out WorldSpace spawnPoint)
        {
            spawnPoint = WorldSpace.Zero;
            if (currentWorld == null) return false;

            ContinentNode starter = GetStarterContinent();
            if (starter == null || starter.Regions == null) return false;

            RegionData chosen = starter.Regions.Cast<RegionData>()
                .Where(region => region != null && !region.IsOceanRegion && (region.IsCapital || region.SiteKind == RegionSiteKind.City))
                .OrderBy(region => region.IsCapital ? 0 : 1)
                .ThenBy(region => region.RepresentativeLevelBand.MinLevel)
                .ThenBy(region => region.RepresentativeLevelBand.MaxLevel)
                .FirstOrDefault();
            if (chosen == null) return false;

            Point continentChunkSize = new Point(
                starter.RegionWidth * currentWorld.Settings.RegionSizeInChunks.X,
                starter.RegionHeight * currentWorld.Settings.RegionSizeInChunks.Y);
            Point continentOrigin = new Point(
                -continentChunkSize.X / 2,
                -continentChunkSize.Y / 2);
            Point chunkCoord = new Point(
                chosen.Coordinate.X * currentWorld.Settings.RegionSizeInChunks.X + currentWorld.Settings.RegionSizeInChunks.X / 2,
                chosen.Coordinate.Y * currentWorld.Settings.RegionSizeInChunks.Y + currentWorld.Settings.RegionSizeInChunks.Y / 2) + continentOrigin;

            spawnPoint = new WorldSpace(
                (chunkCoord.X + 0.5f) * Chunk.ChunkSize.X * Tile.Size.X,
                (chunkCoord.Y + 0.5f) * Chunk.ChunkSize.Y * Tile.Size.Y);
            return true;
        }

        static Point GetBlockSample(WorldGenerationSettings settings, Point localChunkInContinent, Point tileGridSize, int tileX, int tileY)
        {
            int blockX = localChunkInContinent.X * settings.ChunkFootprintInBlocks.X + (tileX * settings.ChunkFootprintInBlocks.X / Math.Max(1, tileGridSize.X));
            int blockY = localChunkInContinent.Y * settings.ChunkFootprintInBlocks.Y + (tileY * settings.ChunkFootprintInBlocks.Y / Math.Max(1, tileGridSize.Y));
            return new Point(blockX, blockY);
        }

        static int SelectTileId(ChunkWorldGenData data, Point blockSample, int grassId, int dirtId, int wallId)
        {
            float height01 = SampleBlockHeight01(data, blockSample);
            if (data.IsOceanRegion)
            {
                return SelectOceanTileId(height01, grassId, dirtId, wallId);
            }

            RegionBiome biome = data.Region?.Biome ?? RegionBiome.Plains;
            float broad = PerlinNoiseGenerator.Fractal01(blockSample.X, blockSample.Y, seed: 5911 + data.ContinentId, scale: 28f, octaves: 3, persistence: 0.5f, lacunarity: 2f);
            float detail = PerlinNoiseGenerator.Fractal01(blockSample.X, blockSample.Y, seed: 8167 + data.ContinentId, scale: 12f, octaves: 2, persistence: 0.55f, lacunarity: 2f);
            float signal = broad * 0.7f + detail * 0.3f;

            return biome switch
            {
                RegionBiome.Forest => height01 > 0.88f && signal > 0.55f ? wallId : signal < 0.28f ? dirtId : grassId,
                RegionBiome.Beach => height01 > 0.92f ? wallId : dirtId,
                RegionBiome.Cliffs => height01 > 0.58f ? wallId : height01 > 0.28f ? dirtId : grassId,
                _ => height01 > 0.94f && signal > 0.6f ? wallId : signal < 0.42f ? dirtId : grassId
            };
        }

        static int SelectOceanTileId(float height01, int grassId, int dirtId, int wallId)
        {
            if (height01 > 0.72f) return grassId;
            if (height01 > 0.24f) return dirtId;
            return wallId;
        }

        static float SampleBlockHeight01(ChunkWorldGenData data, Point blockSample)
        {
            if (currentWorld == null) return 0f;
            if (TrySampleBlockHeight01(data.ContinentId, blockSample, out float height01)) return height01;
            return 0f;
        }

        static int PositiveModulo(int value, int modulo)
        {
            int result = value % modulo;
            return result < 0 ? result + modulo : result;
        }
    }
}
