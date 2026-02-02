using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Project_1.Managers
{
    internal sealed class RenderCache<TSnapshot> where TSnapshot : struct, IRenderSnapshot
    {
        readonly ConcurrentQueue<TSnapshot> pendingUpdates = new ConcurrentQueue<TSnapshot>();
        readonly ConcurrentQueue<int> pendingRemovals = new ConcurrentQueue<int>();
        readonly Dictionary<int, TSnapshot> snapshots = new Dictionary<int, TSnapshot>();
        volatile bool clearRequested;

        public void EnqueueUpdate(in TSnapshot snapshot)
        {
            pendingUpdates.Enqueue(snapshot);
        }

        public void EnqueueRemove(int renderId)
        {
            pendingRemovals.Enqueue(renderId);
        }

        public void RequestClear()
        {
            clearRequested = true;
        }

        public void ApplyUpdates()
        {
            ThreadAffinity.AssertMainThread();
            if (clearRequested)
            {
                snapshots.Clear();
                clearRequested = false;
                while (pendingUpdates.TryDequeue(out _)) { }
                while (pendingRemovals.TryDequeue(out _)) { }
            }

            while (pendingRemovals.TryDequeue(out int id))
            {
                snapshots.Remove(id);
            }
            while (pendingUpdates.TryDequeue(out TSnapshot snapshot))
            {
                snapshots[snapshot.RenderId] = snapshot;
            }
        }

        public IEnumerable<TSnapshot> Values => snapshots.Values;
    }
}
