using Microsoft.Xna.Framework;
using System;

namespace Project_1.WorldGeneration
{
    internal sealed class WorldGenerationSettings
    {
        public const int FixedChunkFootprint = 32;
        public const int FixedRegionChunkSpan = 32;

        public int Seed { get; init; }
        public int WorldOrdinal { get; init; } = 1;
        public int ChunkHeight { get; init; } = 32;
        public int TargetContinentCount { get; init; } = 7;
        public int MinWorldLevel { get; init; } = 1;
        public int MaxWorldLevel { get; init; } = 60;
        public int ContinentMinLevelSpan { get; init; } = 5;
        public int ContinentMaxLevelSpan { get; init; } = 15;
        public Point ChunkFootprintInBlocks { get; init; } = new Point(FixedChunkFootprint, FixedChunkFootprint);
        public Point RegionSizeInChunks { get; init; } = new Point(FixedRegionChunkSpan, FixedRegionChunkSpan);
        public int MinOceanLengthInRegions { get; init; } = 3;
        public int MaxOceanLengthInRegions { get; init; } = 9;
        public float OceanOverlayDistanceFactor { get; init; } = 0.35f;
        public float OceanOverlayStrength { get; init; } = 0.4f;
        public int HeightNoiseSeedOffset { get; init; } = 12011;
        public float HeightNoiseScale { get; init; } = 96f;
        public int HeightNoiseOctaves { get; init; } = 4;
        public float HeightNoisePersistence { get; init; } = 0.5f;
        public float HeightNoiseLacunarity { get; init; } = 2f;
        public float OceanHeightFalloffPower { get; init; } = 1f;
        public WorldGenerationRecipe Recipe { get; init; }

        public int RegionBlockWidth => RegionSizeInChunks.X * ChunkFootprintInBlocks.X;
        public int RegionBlockHeight => RegionSizeInChunks.Y * ChunkFootprintInBlocks.Y;

        public void Validate()
        {
            if (WorldOrdinal <= 0) throw new ArgumentOutOfRangeException(nameof(WorldOrdinal));
            if (ChunkHeight <= 0) throw new ArgumentOutOfRangeException(nameof(ChunkHeight));
            if (TargetContinentCount <= 0) throw new ArgumentOutOfRangeException(nameof(TargetContinentCount));
            if (MinWorldLevel < 1) throw new ArgumentOutOfRangeException(nameof(MinWorldLevel));
            if (MaxWorldLevel <= MinWorldLevel) throw new ArgumentOutOfRangeException(nameof(MaxWorldLevel));
            if (ContinentMinLevelSpan < 0) throw new ArgumentOutOfRangeException(nameof(ContinentMinLevelSpan));
            if (ContinentMaxLevelSpan < ContinentMinLevelSpan) throw new ArgumentOutOfRangeException(nameof(ContinentMaxLevelSpan));
            if (ChunkFootprintInBlocks.X != FixedChunkFootprint || ChunkFootprintInBlocks.Y != FixedChunkFootprint)
            {
                throw new ArgumentOutOfRangeException(nameof(ChunkFootprintInBlocks), "Chunks are currently fixed to 32xwhx32 blocks.");
            }
            if (RegionSizeInChunks.X != FixedRegionChunkSpan || RegionSizeInChunks.Y != FixedRegionChunkSpan)
            {
                throw new ArgumentOutOfRangeException(nameof(RegionSizeInChunks), "Regions are currently fixed to 32x32 chunks.");
            }
            if (MinOceanLengthInRegions <= 0) throw new ArgumentOutOfRangeException(nameof(MinOceanLengthInRegions));
            if (MaxOceanLengthInRegions < MinOceanLengthInRegions) throw new ArgumentOutOfRangeException(nameof(MaxOceanLengthInRegions));
            if (OceanOverlayDistanceFactor <= 0f || OceanOverlayDistanceFactor > 1f) throw new ArgumentOutOfRangeException(nameof(OceanOverlayDistanceFactor));
            if (OceanOverlayStrength < 0f || OceanOverlayStrength > 1f) throw new ArgumentOutOfRangeException(nameof(OceanOverlayStrength));
            if (HeightNoiseScale <= 0f) throw new ArgumentOutOfRangeException(nameof(HeightNoiseScale));
            if (HeightNoiseOctaves <= 0) throw new ArgumentOutOfRangeException(nameof(HeightNoiseOctaves));
            if (HeightNoisePersistence <= 0f || HeightNoisePersistence > 1f) throw new ArgumentOutOfRangeException(nameof(HeightNoisePersistence));
            if (HeightNoiseLacunarity < 1f) throw new ArgumentOutOfRangeException(nameof(HeightNoiseLacunarity));
            if (OceanHeightFalloffPower <= 0f) throw new ArgumentOutOfRangeException(nameof(OceanHeightFalloffPower));
        }
    }
}
