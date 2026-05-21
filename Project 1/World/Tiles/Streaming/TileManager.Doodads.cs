using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects.Doodads;
using Project_1.Managers;

namespace Project_1.Tiles
{
    internal static partial class TileManager
    {
        static void UpdateChunkDoodads()
        {
            //TODO: Rather than doing this, each chunk should keep track of its own doodads and update should be called on chunk basis
            chunkLock.EnterReadLock();
            try
            {
                foreach (Chunk chunk in chunks.Values)
                {
                    chunk?.Doodads?.Update();
                }
            }
            finally
            {
                chunkLock.ExitReadLock();
            }
        }

        public static bool TryGetDoodadAt(WorldSpace worldPos, out Doodad doodad)
        {
            ThreadAffinity.AssertSimThread();
            doodad = null;
            //Q: Rather than locking, since the main thread is the one that resolves click, shouldn't this data just be sent as part of the render snapshot and then the main thread can resolve the click without needing to lock?
            chunkLock.EnterReadLock();
            try
            {
                foreach (Chunk chunk in chunks.Values)
                {
                    if (chunk?.Doodads == null) continue;
                    if (!chunk.WorldRectangle.Contains(worldPos.ToPoint())) continue;
                    if (chunk.Doodads.TryGetDoodadAt(worldPos, out doodad)) return true;
                }
            }
            finally
            {
                chunkLock.ExitReadLock();
            }

            return false;
        }

        public static bool TryGetDoodadByRenderId(int renderId, out Doodad doodad)
        {
            //Q: Rather than locking, since the main thread is the one that resolves click, shouldn't this data just be sent as part of the render snapshot and then the main thread can resolve the click without needing to lock?
            ThreadAffinity.AssertSimThread();
            doodad = null;
            chunkLock.EnterReadLock();
            try
            {
                foreach (Chunk chunk in chunks.Values)
                {
                    if (chunk?.Doodads == null) continue;
                    if (chunk.Doodads.TryGetDoodadByRenderId(renderId, out doodad)) return true;
                }
            }
            finally
            {
                chunkLock.ExitReadLock();
            }

            return false;
        }

        internal static void DrawDoodadSnapshots(SpriteBatch aBatch)
        {
            //Q: Should this be inside chunks? Very possible it should not, either way this should probably not be in TileManager
            ThreadAffinity.AssertMainThread();
            chunkLock.EnterReadLock();
            try
            {
                foreach (Chunk chunk in chunks.Values)
                {
                    chunk?.Doodads?.DrawSnapshots(aBatch);
                }
            }
            finally
            {
                chunkLock.ExitReadLock();
            }
        }

        internal static void BuildDoodadRenderSnapshots()
        {
            ThreadAffinity.AssertSimThread();
            chunkLock.EnterReadLock();
            try
            {
                foreach (Chunk chunk in chunks.Values)
                {
                    chunk?.Doodads?.BuildRenderSnapshot();
                }
            }
            finally
            {
                chunkLock.ExitReadLock();
            }
        }
    }
}
