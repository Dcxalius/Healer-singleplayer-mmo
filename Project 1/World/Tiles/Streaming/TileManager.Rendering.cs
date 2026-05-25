using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using System.Collections.Generic;

namespace Project_1.Tiles
{
    internal static partial class TileManager
    {
        internal static void DrawMinimapSnapshots(SpriteBatch aBatch, WorldSpace aOrigin, AbsoluteScreenPosition aMinimapOffset, AbsoluteScreenPosition aSize)
        {
            //TODO: Move this else where. Minimap should be its own seperate class and all it should ask for is the top blocks in each nearby chunks
            ThreadAffinity.AssertMainThread();
            TileRenderCache.FlushMinimapSnapshots();
            renderChunks.ApplyUpdates();
            foreach (ChunkRenderSnapshot snapshot in renderChunks.Values)
            {
                snapshot.MinimapDraw(aBatch, aOrigin, aMinimapOffset, aSize);
            }
        }

        internal static void DrawSnapshots()
        {
            //Q: Should we create a seperate manager for managing the render side?
            ThreadAffinity.AssertMainThread();
            renderChunks.ApplyUpdates();
            WorldBlockRenderer.PrepareFrame();
            foreach (ChunkRenderSnapshot chunk in renderChunks.Values)
            {
                if (!WorldBlockRenderer.ShouldDrawChunk(chunk.ChunkPosition)) continue;
                chunk.Draw();
            }
        }

        internal static void BuildRenderSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            List<Chunk> chunkSnapshotScratch = new List<Chunk>();
            chunkSnapshotScratch.Clear();
            foreach (Chunk chunk in chunks.Values)
            {
                //TODO: This should trim and only publish chunks that are nearby the player
                if (chunk == null) continue;
                chunkSnapshotScratch.Add(chunk);
            }

            chunkSnapshotScratch.Sort((x, y) =>
            {
                if (ReferenceEquals(x, y)) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                return x.Id.CompareTo(y.Id);
            }); //Q: Does the lack of sorting cause any issues? If not, why are we sorting here?
            currentChunkIds.Clear();
            for (int i = 0; i < chunkSnapshotScratch.Count; i++)
            {
                Chunk chunk = chunkSnapshotScratch[i];
                TileRenderCache.PublishChunkMinimapSnapshot(chunk); //
                ChunkRenderSnapshot snapshot = chunk.BuildRenderSnapshot();
                renderChunks.EnqueueUpdate(snapshot);
                currentChunkIds.Add(snapshot.RenderId);
            }

            PublishRemovals();
        }

        static void PublishRemovals()
        {
            foreach (int id in knownChunkIds)
            {
                if (!currentChunkIds.Contains(id))
                {
                    renderChunks.EnqueueRemove(id);
                }
            }

            knownChunkIds.Clear();
            foreach (int id in currentChunkIds)
            {
                knownChunkIds.Add(id);
            }
        }

        static void ClearRenderCache()
        {
            renderChunks.RequestClear();
            knownChunkIds.Clear();
            currentChunkIds.Clear();
        }
    }
}
