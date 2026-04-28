using Project_1.GameObjects.Doodads;
using Project_1.Managers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Project_1.Tiles
{
    internal static partial class TileManager
    {
        static Dictionary<int, Chunk> chunks;
        static readonly RenderCache<ChunkRenderSnapshot> renderChunks = new RenderCache<ChunkRenderSnapshot>();
        static readonly HashSet<int> knownChunkIds = new HashSet<int>();
        static readonly HashSet<int> currentChunkIds = new HashSet<int>();
        static readonly ReaderWriterLockSlim chunkLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        static readonly ConcurrentDictionary<int, Chunk> unpublishedChunks = new ConcurrentDictionary<int, Chunk>();
        static readonly ConcurrentDictionary<int, ChunkBuildJob> activeChunkBuildJobs = new ConcurrentDictionary<int, ChunkBuildJob>();
        static readonly List<Chunk> chunkSnapshotScratch = new List<Chunk>();
        static readonly ChunkIdComparer chunkIdComparer = new ChunkIdComparer();
        static readonly PathFinder pathFinder = new PathFinder();
        static int chunkBuildEpoch;
        static bool initialized;

        public static CollisionManager CollisionManager;

        sealed class ChunkIdComparer : IComparer<Chunk>
        {
            public int Compare(Chunk x, Chunk y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                return x.Id.CompareTo(y.Id);
            }
        }

        sealed class ChunkBuildJob
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

        public static Chunk[] GetChunksSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            if (chunks == null || chunks.Count == 0) return Array.Empty<Chunk>();
            return Enumerable.ToArray(chunks.Values);
        }
    }
}
