namespace Project_1.Tiles
{
    internal static partial class TileManager
    {
        static Chunk CreateStructuredChunk(int chunkId, Block[,,] blocks)
        {
            //TODO: The structure generation system should be internal to the chunk generation process OR done afterwards. Doing it after can create headaches with chunk gen ordering causing different structure placements on different runs with the same seed
            StructureSpawnSystem.ApplyToChunkBlocks(chunkId, blocks);
            Chunk chunk = new Chunk(blocks, chunkId);
            chunk.Doodads.EnsureStructureDoodads(chunk);
            return chunk;
        }

        static void EnsureStructureDoodadsForLoadedChunk(Chunk chunk)
        {
            //Q: What is the purpose of this method? I think it might be an old hack related to the fact that structure doodads were added after the fact and not properly integrated into chunk generation, but I want to make sure before I remove it
            //If new structures have been added since the time the chunk was generated we just have to live with it for now. In the future new structures should be built by a faction system, so we shouldn't have to worry about missing features for long-term players
            if (chunk == null) return;
            chunk.Doodads.EnsureStructureDoodads(chunk);
        }
    }
}
