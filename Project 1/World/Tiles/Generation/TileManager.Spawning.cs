using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers;
using System;

namespace Project_1.Tiles
{
    internal static partial class TileManager
    {
        public static WorldSpace FindSpawnPointNearChunkLevel(int targetAverageLevel, int maxSearchRadius = 96)
        {
            ThreadAffinity.AssertSimThread();
            int targetLevel = Math.Clamp(targetAverageLevel, 1, 60);
            int radius = Math.Max(0, maxSearchRadius);
            Point origin = Point.Zero;

            int chunkId = FindNearestChunkIdByAverageLevel(origin, targetLevel, radius);
            Chunk chunk = EnsureChunkLoaded(chunkId);
            return FindBestSpawnInChunk(chunk);
        }

        static int FindNearestChunkIdByAverageLevel(Point origin, int targetAverageLevel, int maxRadius)
        {
            for (int radius = 0; radius <= maxRadius; radius++)
            {
                if (radius == 0)
                {
                    if (Chunk.GetAverageLevelForChunkPosition(origin) == targetAverageLevel)
                    {
                        return Chunk.GetChunkId(origin);
                    }
                    continue;
                }

                int minX = origin.X - radius;
                int maxX = origin.X + radius;
                int minY = origin.Y - radius;
                int maxY = origin.Y + radius;

                for (int x = minX; x <= maxX; x++)
                {
                    Point top = new Point(x, minY);
                    if (Chunk.GetAverageLevelForChunkPosition(top) == targetAverageLevel) return Chunk.GetChunkId(top);

                    Point bottom = new Point(x, maxY);
                    if (Chunk.GetAverageLevelForChunkPosition(bottom) == targetAverageLevel) return Chunk.GetChunkId(bottom);
                }

                for (int y = minY + 1; y <= maxY - 1; y++)
                {
                    Point left = new Point(minX, y);
                    if (Chunk.GetAverageLevelForChunkPosition(left) == targetAverageLevel) return Chunk.GetChunkId(left);

                    Point right = new Point(maxX, y);
                    if (Chunk.GetAverageLevelForChunkPosition(right) == targetAverageLevel) return Chunk.GetChunkId(right);
                }
            }

            return Chunk.GetChunkId(origin);
        }

        static WorldSpace FindBestSpawnInChunk(Chunk chunk)
        {
            int centerX = Chunk.ChunkSize.X / 2;
            int centerY = Chunk.ChunkSize.Y / 2;
            Tile centerTile = chunk.Tile(centerX, centerY);
            if (centerTile != null && centerTile.Walkable)
            {
                return centerTile.Centre;
            }

            for (int y = 0; y < Chunk.ChunkSize.Y; y++)
            {
                for (int x = 0; x < Chunk.ChunkSize.X; x++)
                {
                    Tile tile = chunk.Tile(x, y);
                    if (tile == null || !tile.Walkable) continue;
                    return tile.Centre;
                }
            }

            return chunk.Position + new WorldSpace(Chunk.ChunkSize.X * Tile.Size.X * 0.5f, Chunk.ChunkSize.Y * Tile.Size.Y * 0.5f);
        }
    }
}
