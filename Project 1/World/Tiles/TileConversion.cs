using Microsoft.Xna.Framework;
using Project_1.Camera;
using System;

namespace Project_1.Tiles
{
    internal static partial class TileManager
    {
        static int Modulo(int x, int y)
        {
            int r = x % y;
            return r < 0 ? r + y : r;
        }

        public static Chunk GetChunk(int aX, int aY) => GetChunk(Chunk.GetChunkId(aX, aY));
        public static Chunk GetChunk(Point aPos) => GetChunk(aPos.X, aPos.Y);
        public static Chunk GetChunk(WorldSpace aSpaceInWorld) => GetChunk(new Point((int)MathF.Floor(aSpaceInWorld.X / Chunk.ChunkSize.X / Tile.Size.X), (int)MathF.Floor(aSpaceInWorld.Y / Chunk.ChunkSize.Y / Tile.Size.Y)));

        public static Chunk GetChunkUnder(WorldSpace aWorldSpace)
        {
            int id = Chunk.GetChunkId((int)MathF.Floor(aWorldSpace.X / Tile.Size.X / Chunk.ChunkSize.X), (int)MathF.Floor(aWorldSpace.Y / Tile.Size.Y / Chunk.ChunkSize.Y));
            return GetChunk(id);
        }

        internal static Point GetGridPos(WorldSpace aWorldSpace)
        {
            return new Point((int)MathF.Floor(aWorldSpace.X / Tile.Size.X), (int)MathF.Floor(aWorldSpace.Y / Tile.Size.Y));
        }

        internal static Tile GetTileAtGrid(Point aGridPos)
        {
            Point chunkPos = new Point((int)MathF.Floor((float)aGridPos.X / Chunk.ChunkSize.X), (int)MathF.Floor((float)aGridPos.Y / Chunk.ChunkSize.Y));
            Chunk chunk = GetChunk(chunkPos);
            if (chunk == null) return null;
            int localX = Modulo(aGridPos.X, Chunk.ChunkSize.X);
            int localY = Modulo(aGridPos.Y, Chunk.ChunkSize.Y);
            return chunk.Tile(localX, localY);
        }
    }
}
