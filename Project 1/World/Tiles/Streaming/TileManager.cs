using Project_1.GameObjects.Doodads;
using Project_1.Managers;
using Project_1.Managers.Saves;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Project_1.Tiles
{
    internal static partial class TileManager //TODO: This should be renamed to ChunkManager
    {
        //TODO: Each region should have its own manager.
        //TODO: We need a way to track what chunks are loaded and send that info only to the main thread for renders.
        //TODO: Unsure if we should have a seperate update list as well. Allowing for the seperation of loaded and updating chunks and loaded and rendered chunks
        static Dictionary<int, Chunk> chunks;

        //Q: These are all related no?
        static readonly RenderCache<ChunkRenderSnapshot> renderChunks = new RenderCache<ChunkRenderSnapshot>(); //Q: Should this really be here? Makes more sense to host it somewhere on the Main and then just feed from here no?
        //Q: What is a known chunk?
        static readonly HashSet<int> knownChunkIds = new HashSet<int>();
        //Q: What is a current chunk?
        static readonly HashSet<int> currentChunkIds = new HashSet<int>();
        //Q: What are we locking for?
        static readonly ReaderWriterLockSlim chunkLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        //Q: Why are we tracking unpublished chunks? Shouldn't we just publish them as soon as they are ready and then remove them from the unpublished list?
        static readonly ConcurrentDictionary<int, Chunk> unpublishedChunks = new ConcurrentDictionary<int, Chunk>();
        static readonly ConcurrentDictionary<int, ChunkBuildJob> activeChunkBuildJobs = new ConcurrentDictionary<int, ChunkBuildJob>();
        //
        static readonly PathFinder pathFinder = new PathFinder(); //Q: Shouldn't this just be a static class that gets sent the required data when we need to do pathfinding?
        static int chunkBuildEpoch;
        static bool initialized;

        public static CollisionManager CollisionManager;


        sealed class ChunkBuildJob //Q: Should this be in chunks instead?
        {
            public ChunkBuildJob(int epoch)
            {
                Epoch = epoch;
                Completion = new TaskCompletionSource<Chunk>(TaskCreationOptions.RunContinuationsAsynchronously);
            }

            public int Epoch { get; }
            public TaskCompletionSource<Chunk> Completion { get; }
        }

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;
            chunks = new Dictionary<int, Chunk>();
            CollisionManager = new CollisionManager();
        }

        public static Chunk[] GetChunks()
        {
            //Q: Since chunk should always be loaded on load/new game, is the check needed? Shouldn't it just be an assert if we want to be safe?
            ThreadAffinity.AssertSimThread();
            if (chunks == null || chunks.Count == 0) return Array.Empty<Chunk>();
            return Enumerable.ToArray(chunks.Values);
        }

        public static void SaveData(Save aSave)
        {
            ThreadAffinity.AssertSimThread();

            foreach (Chunk chunk in chunks.Values)
            {
                SaveManager.ExportData(aSave.Tiles + "\\" + chunk.Id + ".tilemap", chunk); //TODO: Change file extension to something more appropriate
            }
        }
    }
}
