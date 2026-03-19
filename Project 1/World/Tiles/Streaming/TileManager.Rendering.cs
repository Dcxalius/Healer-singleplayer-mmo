using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;

namespace Project_1.Tiles
{
    internal static partial class TileManager
    {
        internal static void DrawMinimapSnapshots(SpriteBatch aBatch, WorldSpace aOrigin, AbsoluteScreenPosition aMinimapOffset, AbsoluteScreenPosition aSize)
        {
            ThreadAffinity.AssertMainThread();
            TileRenderCache.FlushMinimapSnapshots();
            renderChunks.ApplyUpdates();
            foreach (ChunkRenderSnapshot snapshot in renderChunks.Values)
            {
                snapshot.MinimapDraw(aBatch, aOrigin, aMinimapOffset, aSize);
            }
        }

        internal static void DrawSnapshots(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            renderChunks.ApplyUpdates();
            foreach (ChunkRenderSnapshot chunk in renderChunks.Values)
            {
                if (!Camera.Camera.WorldspaceBoundsCheck(chunk.WorldRectangle)) continue;
                chunk.Draw(aBatch);
            }
        }

        internal static void BuildRenderSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            chunkSnapshotScratch.Clear();
            foreach (Chunk chunk in chunks.Values)
            {
                if (chunk == null) continue;
                chunkSnapshotScratch.Add(chunk);
            }

            chunkSnapshotScratch.Sort(chunkIdComparer);
            currentChunkIds.Clear();
            for (int i = 0; i < chunkSnapshotScratch.Count; i++)
            {
                Chunk chunk = chunkSnapshotScratch[i];
                TileRenderCache.PublishChunkMinimapSnapshot(chunk);
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
