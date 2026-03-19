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
