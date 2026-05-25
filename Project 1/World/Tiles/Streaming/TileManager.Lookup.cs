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

        static int Modulo(int x, int y) //TODO: Move this to a math utils class
        {
            int r = x % y;
            return r < 0 ? r + y : r;
        }

        public static Tile GetTile(WorldSpace aSpace) //TODO: This should be renamed to GetBlockUnder 
        {
            Chunk chunk = GetChunkUnder(aSpace);
            Debug.Assert(chunk != null, "GetTile called for an unloaded chunk.");
            if (chunk == null) return null;
            return GetTile(chunk, Modulo((int)MathF.Floor(aSpace.X / Tile.Size.X), Chunk.ChunkSize.X), Modulo((int)MathF.Floor(aSpace.Y / Tile.Size.Y), Chunk.ChunkSize.Y));
        }

        public static Tile GetTile(Chunk aChunk, int aX, int aY) //TODO: Rename
        {
            Debug.Assert(aChunk != null, "GetTile called with a null chunk.");
            return aChunk?.Tile(aX, aY);
        }

        public static Tile GetTile(int aChunkId, int aX, int aY)//TODO: Rename
        {
            if (!chunks.TryGetValue(aChunkId, out Chunk chunk))
            {
                Debug.Assert(false, $"GetTile called for missing chunk id {aChunkId}.");
                return null;
            }

            return chunk.Tile(aX, aY);
        }

        public static bool TryGetTile(WorldSpace aSpace, out Tile tile)//TODO: Rename
        {
            Chunk chunk = GetChunkUnder(aSpace);
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

        /// <summary>
        /// Returns the chunk with a given id, or null if it is not loaded. Note that this method is not thread safe and should only be called from the sim thread.
        /// </summary>
        /// <param name="aId"></param>
        /// <returns></returns>
        public static Chunk GetChunk(int aId) => chunks.TryGetValue(aId, out Chunk chunk) ? chunk : null;
        /// <inheritdoc cref="GetChunk(int)"/>
        public static Chunk GetChunk(int aX, int aY) => GetChunk(ChunkAddressing.GetChunkId(aX, aY));
        /// <summary>
        /// Gets the chunk at a given grid position, or null if it is not loaded. Note that this method is not thread safe and should only be called from the sim thread.
        /// </summary>
        /// <param name="aPos"></param>
        /// <returns></returns>
        public static Chunk GetChunkAtGridPos(Point aPos) => GetChunk(aPos.X, aPos.Y);

        /// <summary>
        /// Gets the chunk given a world space position, or null if it is not loaded. Note that this method is not thread safe and should only be called from the sim thread.
        /// </summary>
        /// <param name="aWorldSpace"></param>
        /// <returns></returns>
        public static Chunk GetChunkUnder(WorldSpace aWorldSpace)
        {
            //TODO: There should probably be a method to use a 3d position to get a chunk as well
            int id = ChunkAddressing.GetChunkId((int)MathF.Floor(aWorldSpace.X / Tile.Size.X / Chunk.ChunkSize.X), (int)MathF.Floor(aWorldSpace.Y / Tile.Size.Y / Chunk.ChunkSize.Y));
            return GetChunk(id);
        }

        public static float GetDragCoeficient(WorldSpace aFeetPos) => GetTile(aFeetPos).DragCoeficient; //TODO: Very old, should at least be renamed if not removed and replaced with something more general like GetTileProperty

        internal static Point GetGridPos(WorldSpace aWorldSpace) //TODO: Should probably be somewhere else
        {
            return new Point((int)MathF.Floor(aWorldSpace.X / Tile.Size.X), (int)MathF.Floor(aWorldSpace.Y / Tile.Size.Y));
        }

        internal static Tile GetTileAtGrid(Point aGridPos) 
        {
            //TODO: Old and probably deprecated. Using the other methods above instead
            //The method is currently the only way to use a only a point instead of two ints, but it should be remade and put in the above chain of methods if it is still needed
            Point chunkPos = new Point((int)MathF.Floor((float)aGridPos.X / Chunk.ChunkSize.X), (int)MathF.Floor((float)aGridPos.Y / Chunk.ChunkSize.Y));
            Chunk chunk = GetChunkAtGridPos(chunkPos);
            if (chunk == null) return null;
            int localX = Modulo(aGridPos.X, Chunk.ChunkSize.X);
            int localY = Modulo(aGridPos.Y, Chunk.ChunkSize.Y);
            return chunk.Tile(localX, localY);
        }

        public static Tile[,] GetSurroundingTiles(Tile aTile) //TODO: Rename
        {
            const int sizeOfSquareToCheck = 3; //TODO: This should probably be a parameter
            Debug.Assert(sizeOfSquareToCheck % 2 == 1); //TODO: And this should either throw an exception or we should change the reference frame to be in one direction instead of a square. (3 => 1, 5 => 2, etc)
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
            //TODO: We should probably make clear the diff between this and the above one. The above one uses a filled square and this is a hollow circle, but the names don't make that clear. 
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
