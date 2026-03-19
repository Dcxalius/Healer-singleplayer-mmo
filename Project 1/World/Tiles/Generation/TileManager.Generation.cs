namespace Project_1.Tiles
{
    internal static partial class TileManager
    {
        static Chunk CreateStructuredChunk(int chunkId, int[,] tileIds)
        {
            StructureSpawnSystem.ApplyToChunkTiles(chunkId, tileIds);
            Chunk chunk = new Chunk(tileIds, chunkId);
            chunk.Doodads.EnsureStructureDoodads(chunk);
            return chunk;
        }

        static void EnsureStructureDoodadsForLoadedChunk(Chunk chunk)
        {
            if (chunk == null) return;
            chunk.Doodads.EnsureStructureDoodads(chunk);
        }
    }
}
