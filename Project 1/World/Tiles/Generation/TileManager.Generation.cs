namespace Project_1.Tiles
{
    internal static partial class TileManager
    {
        static Chunk CreateStructuredChunk(int chunkId, Block[,,] blocks)
        {
            StructureSpawnSystem.ApplyToChunkBlocks(chunkId, blocks);
            Chunk chunk = new Chunk(blocks, chunkId);
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
