using Project_1.Camera;
using Project_1.Managers;
using System;

namespace Project_1.Tiles
{
    internal static partial class TileManager
    {
        static Path GeneratePathThreadSafe(WorldSpace aStartPosition, WorldSpace aTargetPosition, WorldSpace aSize)
        {
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
                Complete(GeneratePathThreadSafe(aStartPosition, aTargetPosition, aSize));
                return;
            }

            WorkerPool.Enqueue(() => GeneratePathThreadSafe(aStartPosition, aTargetPosition, aSize), Complete);
        }
    }
}
