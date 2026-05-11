using Microsoft.Xna.Framework;
using Project_1.Managers;
using Project_1.WorldGeneration;
using System;

namespace Project_1.Tiles
{
    internal static class ChunkGenerator
    {
        public static Block[,,] GenerateBlocks(int chunkId)
        {
            Point chunkPos = ChunkAddressing.GetChunkPosition(chunkId);
            int[,] tileIds = GenerateTileIds(chunkId);
            Block[,,] blocks = new Block[tileIds.GetLength(0), tileIds.GetLength(1), Chunk.ChunkHeight];
            float[,] height01Samples = DebugManager.ShouldDumpPerlinNoisePngs
                ? new float[tileIds.GetLength(0), tileIds.GetLength(1)]
                : null;

            for (int i = 0; i < tileIds.GetLength(0); i++)
            {
                for (int j = 0; j < tileIds.GetLength(1); j++)
                {
                    float height01 = SampleColumnHeight01(chunkPos, i, j);
                    if (height01Samples != null)
                    {
                        height01Samples[i, j] = height01;
                    }

                    int columnHeight = SampleColumnHeight(height01);
                    int topZ = Math.Max(0, columnHeight - 1);
                    for (int z = 0; z < columnHeight; z++)
                    {
                        blocks[i, j, z] = CreateColumnBlock(tileIds[i, j], z, topZ);
                    }
                }
            }

            if (height01Samples != null)
            {
                DebugManager.QueuePerlinHeightmapExport(chunkId, chunkPos, height01Samples);
            }

            return blocks;
        }

        static int[,] GenerateTileIds(int chunkId)
        {
            Point chunkPos = ChunkAddressing.GetChunkPosition(chunkId);
            if (NodeWorldManager.TryGenerateChunkTileIds(chunkPos, Chunk.ChunkSize, out int[,] nodeWorldTileIds))
            {
                return nodeWorldTileIds;
            }

            int dirtId = TileFactory.GetTileData("Dirt").ID;
            int grassId = TileFactory.GetTileData("Grass").ID;

            int[,] ids = new int[Chunk.ChunkSize.X, Chunk.ChunkSize.Y];
            int chunkTileX = chunkPos.X * Chunk.ChunkSize.X;
            int chunkTileY = chunkPos.Y * Chunk.ChunkSize.Y;

            for (int i = 0; i < Chunk.ChunkSize.X; i++)
            {
                for (int j = 0; j < Chunk.ChunkSize.Y; j++)
                {
                    int globalX = chunkTileX + i;
                    int globalY = chunkTileY + j;

                    float broadNoise = PerlinNoiseGenerator.Fractal01(globalX, globalY, seed: 4871, scale: 56f, octaves: 4, persistence: 0.5f, lacunarity: 2f);
                    float detailNoise = PerlinNoiseGenerator.Fractal01(globalX, globalY, seed: 9323, scale: 24f, octaves: 3, persistence: 0.55f, lacunarity: 2f);
                    float dirtBlend = (broadNoise * 0.75f) + (detailNoise * 0.25f);

                    ids[i, j] = dirtBlend < 0.5f ? dirtId : grassId;
                }
            }

            return ids;
        }

        public static int GetAverageLevelForChunkPosition(Point chunkPos) => GenerateAverageLevel(chunkPos);

        public static int GetAverageLevelForChunkId(int chunkId) => GenerateAverageLevel(ChunkAddressing.GetChunkPosition(chunkId));

        static int GenerateAverageLevel(Point chunkPos)
        {
            if (NodeWorldManager.TryGetChunkAverageLevel(chunkPos, out int averageLevel))
            {
                return averageLevel;
            }

            float macroNoise = PerlinNoiseGenerator.Fractal01(chunkPos.X, chunkPos.Y, seed: 14717, scale: 46f, octaves: 4, persistence: 0.5f, lacunarity: 2f);
            float detailNoise = PerlinNoiseGenerator.Fractal01(chunkPos.X, chunkPos.Y, seed: 21341, scale: 16f, octaves: 3, persistence: 0.55f, lacunarity: 2f);
            float worldLevelSignal = MathHelper.Clamp((macroNoise * 0.72f) + (detailNoise * 0.28f), 0f, 1f);

            float starterZoneNoise = PerlinNoiseGenerator.Fractal01(chunkPos.X, chunkPos.Y, seed: 38183, scale: 24f, octaves: 2, persistence: 0.5f, lacunarity: 2f);
            const float starterZoneThreshold = 0.11f;
            if (starterZoneNoise < starterZoneThreshold)
            {
                float pocketSignal = starterZoneNoise / starterZoneThreshold;
                float starterLevelSignal = pocketSignal * pocketSignal;
                return 1 + (int)MathF.Round(starterLevelSignal * 9f);
            }

            return 2 + (int)MathF.Round(worldLevelSignal * 58f);
        }

        static Block CreateColumnBlock(int tileId, int z, int topZ)
        {
            if (z >= topZ)
            {
                return BlockTileBridge.CreateBlockFromTileId(tileId);
            }

            if (z >= topZ - 1)
            {
                return Block.CreateSolid(BlockMaterialType.Dirt, elevation: z);
            }

            return Block.CreateSolid(BlockMaterialType.Stone, elevation: z);
        }

        static int SampleColumnHeight(float height01)
        {
            int height = 1 + (int)MathF.Round(height01 * (Chunk.ChunkHeight - 1));
            return Math.Clamp(height, 1, Chunk.ChunkHeight);
        }

        static float SampleColumnHeight01(Point chunkPos, int localX, int localY)
        {
            if (TrySampleNodeWorldHeight01(chunkPos, localX, localY, out float nodeWorldHeight01))
            {
                return MathHelper.Clamp(nodeWorldHeight01, 0f, 1f);
            }

            int globalX = chunkPos.X * Chunk.ChunkSize.X + localX;
            int globalY = chunkPos.Y * Chunk.ChunkSize.Y + localY;

            float broadNoise = PerlinNoiseGenerator.Fractal01(globalX, globalY, seed: 17021, scale: 84f, octaves: 4, persistence: 0.5f, lacunarity: 2f);
            float detailNoise = PerlinNoiseGenerator.Fractal01(globalX, globalY, seed: 21929, scale: 28f, octaves: 3, persistence: 0.55f, lacunarity: 2f);
            float ridgeNoise = PerlinNoiseGenerator.Fractal01(globalX, globalY, seed: 28411, scale: 14f, octaves: 2, persistence: 0.5f, lacunarity: 2f);
            float combined = broadNoise * 0.6f + detailNoise * 0.25f + ridgeNoise * 0.15f;
            return MathHelper.Clamp(combined, 0f, 1f);
        }

        static bool TrySampleNodeWorldHeight01(Point chunkPos, int localX, int localY, out float height01)
        {
            height01 = 0f;
            if (NodeWorldManager.CurrentWorld == null) return false;
            if (!NodeWorldManager.TryGetChunkGenerationData(chunkPos, out ChunkWorldGenData data)) return false;

            WorldGenerationSettings settings = NodeWorldManager.CurrentWorld.Settings;
            Point localBlock = new Point(
                data.LocalChunkInContinent.X * settings.ChunkFootprintInBlocks.X + (localX * settings.ChunkFootprintInBlocks.X / Math.Max(1, Chunk.ChunkSize.X)),
                data.LocalChunkInContinent.Y * settings.ChunkFootprintInBlocks.Y + (localY * settings.ChunkFootprintInBlocks.Y / Math.Max(1, Chunk.ChunkSize.Y)));

            return NodeWorldManager.TrySampleBlockHeight01(data.ContinentId, localBlock, out height01);
        }
    }
}
