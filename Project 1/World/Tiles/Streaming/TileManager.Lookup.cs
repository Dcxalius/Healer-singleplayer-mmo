using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.DebugTools;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Project_1.Tiles
{
    internal static partial class TileManager
    {
        [ThreadStatic] static Tile[,] surroundingTilesCache;

        static int Modulo(int x, int y)
        {
            int r = x % y;
            return r < 0 ? r + y : r;
        }

        public static Tile GetTile(WorldSpace aSpace)
        {
            Chunk chunk = GetChunk(aSpace);
            Debug.Assert(chunk != null, "GetTile called for an unloaded chunk.");
            if (chunk == null) return null;
            return GetTile(chunk, Modulo((int)MathF.Floor(aSpace.X / Tile.Size.X), Chunk.ChunkSize.X), Modulo((int)MathF.Floor(aSpace.Y / Tile.Size.Y), Chunk.ChunkSize.Y));
        }

        public static Tile GetTile(Chunk aChunk, int aX, int aY)
        {
            Debug.Assert(aChunk != null, "GetTile called with a null chunk.");
            return aChunk?.Tile(aX, aY);
        }

        public static Tile GetTile(int aChunkId, int aX, int aY)
        {
            if (!chunks.TryGetValue(aChunkId, out Chunk chunk))
            {
                Debug.Assert(false, $"GetTile called for missing chunk id {aChunkId}.");
                return null;
            }

            return chunk.Tile(aX, aY);
        }

        public static bool TryGetTile(WorldSpace aSpace, out Tile tile)
        {
            Chunk chunk = GetChunk(aSpace);
            if (chunk == null)
            {
                tile = null;
                return false;
            }

            int x = Modulo((int)MathF.Floor(aSpace.X / Tile.Size.X), Chunk.ChunkSize.X);
            int y = Modulo((int)MathF.Floor(aSpace.Y / Tile.Size.Y), Chunk.ChunkSize.Y);
            tile = chunk.Tile(x, y);
            return tile != null;
        }

        public static Chunk GetChunk(int aId) => chunks.TryGetValue(aId, out Chunk chunk) ? chunk : null;
        public static Chunk GetChunk(int aX, int aY) => GetChunk(Chunk.GetChunkId(aX, aY));
        public static Chunk GetChunk(Point aPos) => GetChunk(aPos.X, aPos.Y);
        public static Chunk GetChunk(WorldSpace aSpaceInWorld) => GetChunk(new Point((int)MathF.Floor(aSpaceInWorld.X / Chunk.ChunkSize.X / Tile.Size.X), (int)MathF.Floor(aSpaceInWorld.Y / Chunk.ChunkSize.Y / Tile.Size.Y)));

        public static Chunk GetChunkUnder(WorldSpace aWorldSpace)
        {
            int id = Chunk.GetChunkId((int)MathF.Floor(aWorldSpace.X / Tile.Size.X / Chunk.ChunkSize.X), (int)MathF.Floor(aWorldSpace.Y / Tile.Size.Y / Chunk.ChunkSize.Y));
            return GetChunk(id);
        }

        public static float GetDragCoeficient(WorldSpace aFeetPos) => GetTile(aFeetPos).DragCoeficient;

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

        public static Tile[,] GetSurroundingTiles(Tile aTile)
        {
            const int sizeOfSquareToCheck = 3;
            Debug.Assert(sizeOfSquareToCheck % 2 == 1);
            Tile[,] tiles = surroundingTilesCache;
            if (tiles == null || tiles.GetLength(0) != sizeOfSquareToCheck || tiles.GetLength(1) != sizeOfSquareToCheck)
            {
                tiles = new Tile[sizeOfSquareToCheck, sizeOfSquareToCheck];
                surroundingTilesCache = tiles;
            }

            Point centreGrid = GetGridPos(aTile.Centre);
            for (int i = 0; i < sizeOfSquareToCheck; i++)
            {
                for (int j = 0; j < sizeOfSquareToCheck; j++)
                {
                    int x = centreGrid.X - sizeOfSquareToCheck / 2 + i;
                    int y = centreGrid.Y - sizeOfSquareToCheck / 2 + j;
                    tiles[i, j] = GetTileAtGrid(new Point(x, y));
                }
            }

            return tiles;
        }

        public static Tile[] GetTilesAroundPosition(WorldSpace aPosition, float aDistance)
        {
            List<Tile> tiles = new List<Tile>();
            Tile midTile = GetTile(aPosition);
            float distanceInTiles = aDistance / Tile.Size.X;
            int tilesAround = (int)(distanceInTiles * Math.PI) * 2;
            Point midGrid = GetGridPos(midTile.Centre);

            for (int i = 0; i < tilesAround; i++)
            {
                int x = (int)Math.Round(midGrid.X + distanceInTiles * Math.Sin(i * 2 * Math.PI / tilesAround));
                int y = (int)Math.Floor(midGrid.Y + distanceInTiles * Math.Cos(i * 2 * Math.PI / tilesAround));
                Tile tile = GetTileAtGrid(new Point(x, y));

                if (tile == null)
                {
                    DebugManager.Print($"Tried to get tile at grid {x},{y} but it returned null.");
                    continue;
                }

                if (!tile.Walkable) continue;
                tiles.Add(tile);
            }

            return tiles.ToArray();
        }
    }
}
