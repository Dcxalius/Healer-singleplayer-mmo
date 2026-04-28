using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Project_1.Tiles
{
    internal static class StructureSpawnSystem
    {
        internal readonly struct DoodadSpawn
        {
            public DoodadSpawn(StructureDoodadType type, Point localTile)
            {
                Type = type;
                LocalTile = localTile;
            }

            public StructureDoodadType Type { get; }
            public Point LocalTile { get; }
        }

        internal enum StructureDoodadType
        {
            CookingFire
        }

        readonly struct CampPlacement
        {
            public CampPlacement(Point centerTile)
            {
                CenterTile = centerTile;
            }

            public Point CenterTile { get; }
        }

        public static void ApplyToChunkBlocks(int chunkId, Block[,,] blocks)
        {
            if (blocks == null) return;
            int dirtId = TileFactory.GetTileData("Dirt").ID;
            List<CampPlacement> camps = GetCampPlacements(chunkId);

            for (int i = 0; i < camps.Count; i++)
            {
                StampCamp(blocks, camps[i], dirtId);
            }
        }

        public static DoodadSpawn[] GetDoodadSpawns(int chunkId)
        {
            List<CampPlacement> camps = GetCampPlacements(chunkId);
            DoodadSpawn[] spawns = new DoodadSpawn[camps.Count];
            for (int i = 0; i < camps.Count; i++)
            {
                spawns[i] = new DoodadSpawn(StructureDoodadType.CookingFire, camps[i].CenterTile);
            }
            return spawns;
        }

        static List<CampPlacement> GetCampPlacements(int chunkId)
        {
            Point chunkPos = ChunkAddressing.GetChunkPosition(chunkId);
            int seed = HashCode.Combine(chunkId, chunkPos.X, chunkPos.Y, 14891);
            Random rng = new Random(seed);
            int desiredCampCount = rng.Next(5);
            List<CampPlacement> camps = new List<CampPlacement>(desiredCampCount);
            const int edgeMargin = 8;
            const int minDistanceBetweenCamps = 12;
            const int maxAttemptsPerCamp = 12;

            for (int i = 0; i < desiredCampCount; i++)
            {
                for (int attempt = 0; attempt < maxAttemptsPerCamp; attempt++)
                {
                    Point center = new Point(
                        rng.Next(edgeMargin, Chunk.ChunkSize.X - edgeMargin),
                        rng.Next(edgeMargin, Chunk.ChunkSize.Y - edgeMargin));

                    if (!IsFarEnoughFromExisting(camps, center, minDistanceBetweenCamps)) continue;
                    camps.Add(new CampPlacement(center));
                    break;
                }
            }

            return camps;
        }

        static bool IsFarEnoughFromExisting(List<CampPlacement> camps, Point center, int minDistanceBetweenCamps)
        {
            int minDistanceSquared = minDistanceBetweenCamps * minDistanceBetweenCamps;
            for (int i = 0; i < camps.Count; i++)
            {
                Point existing = camps[i].CenterTile;
                int dx = existing.X - center.X;
                int dy = existing.Y - center.Y;
                if (dx * dx + dy * dy < minDistanceSquared) return false;
            }

            return true;
        }

        static void StampCamp(Block[,,] blocks, CampPlacement camp, int dirtId)
        {
            Point center = camp.CenterTile;
            for (int x = center.X - 3; x <= center.X + 3; x++)
            {
                for (int y = center.Y - 3; y <= center.Y + 3; y++)
                {
                    if (x < 0 || x >= Chunk.ChunkSize.X || y < 0 || y >= Chunk.ChunkSize.Y) continue;

                    int dx = Math.Abs(x - center.X);
                    int dy = Math.Abs(y - center.Y);
                    if (dx == 3 && dy == 3) continue;

                    bool replaced = false;
                    for (int z = Chunk.ChunkHeight - 1; z >= 0; z--)
                    {
                        if (blocks[x, y, z] == null) continue;
                        blocks[x, y, z] = BlockTileBridge.CreateBlockFromTileId(dirtId);
                        replaced = true;
                        break;
                    }

                    if (!replaced)
                    {
                        blocks[x, y, 0] = BlockTileBridge.CreateBlockFromTileId(dirtId);
                    }
                }
            }
        }
    }
}
