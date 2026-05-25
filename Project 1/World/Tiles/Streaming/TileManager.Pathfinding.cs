using Project_1.Camera;
using Project_1.Managers;
using System;

namespace Project_1.Tiles
{
    internal static partial class TileManager
    {
        static Path GeneratePath(WorldSpace aStartPosition, WorldSpace aTargetPosition, WorldSpace aSize)
        {
            //TODO: Could we bypass locking by creating a copy of the current close chunks that is maintained by pathfinder and only updated when chunks are added/removed? This would allow pathfinding to run without locking the chunk data, but it would require careful management of the copy to ensure it stays up to date.
            chunkLock.EnterReadLock();
            try
            {
                return pathFinder.GeneratePath(aStartPosition, aTargetPosition, aSize);
            }
            finally
            {
                chunkLock.ExitReadLock();
            }
        }

        public static void RequestPath(WorldSpace aStartPosition, WorldSpace aTargetPosition, WorldSpace aSize, Action<Path> onComplete)
        {
            ThreadAffinity.AssertSimThread();
            if (onComplete == null) return;

            void Complete(Path path)
            {
                ThreadAffinity.AssertSimThread();
                onComplete(path);
            }

            if (!WorkerPool.IsRunning)
            {
                Complete(GeneratePath(aStartPosition, aTargetPosition, aSize));
                return;
            }

            WorkerPool.Enqueue(() => GeneratePath(aStartPosition, aTargetPosition, aSize), Complete);
        }
    }
}
