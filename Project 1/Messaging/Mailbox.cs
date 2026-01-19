using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Project_1.Messaging
{
    /// <summary>
    /// Thread-safe mailbox for passing messages to a designated thread. Dispatch should be called on the owner thread.
    /// </summary>
    internal sealed class Mailbox
    {
        readonly ConcurrentQueue<object> queue = new ConcurrentQueue<object>();
        readonly Dictionary<Type, List<Action<object>>> subscribers = new Dictionary<Type, List<Action<object>>>();
        readonly object subscriberLock = new object();
        int pendingCount;
        int peakCount;
        int lastDispatchCount;
        double lastDispatchMs;
        long totalDispatched;

        public Mailbox(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public void Publish<T>(T message)
        {
            queue.Enqueue(message!);
            int pending = Interlocked.Increment(ref pendingCount);
            int snapshotPeak;
            while (pending > (snapshotPeak = Volatile.Read(ref peakCount)))
            {
                if (Interlocked.CompareExchange(ref peakCount, pending, snapshotPeak) == snapshotPeak)
                {
                    break;
                }
            }
        }

        public void Subscribe<T>(Action<T> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            Action<object> wrapper = msg => handler((T)msg);

            lock (subscriberLock)
            {
                if (!subscribers.TryGetValue(typeof(T), out var list))
                {
                    list = new List<Action<object>>();
                    subscribers.Add(typeof(T), list);
                }
                list.Add(wrapper);
            }
        }

        public void DispatchAll()
        {
            int processed = 0;
            long startTicks = 0;
            bool any = false;
            while (queue.TryDequeue(out var msg))
            {
                if (!any)
                {
                    any = true;
                    startTicks = Stopwatch.GetTimestamp();
                }
                Interlocked.Decrement(ref pendingCount);
                if (msg == null) continue;
                var type = msg.GetType();
                if (!subscribers.TryGetValue(type, out var list)) continue;

                processed++;
                for (int i = 0; i < list.Count; i++)
                {
                    list[i](msg);
                }
            }

            if (any)
            {
                double elapsedMs = (Stopwatch.GetTimestamp() - startTicks) * 1000d / Stopwatch.Frequency;
                Volatile.Write(ref lastDispatchCount, processed);
                Volatile.Write(ref lastDispatchMs, elapsedMs);
                Interlocked.Add(ref totalDispatched, processed);
            }
        }

        public MailboxStats GetStats()
        {
            return new MailboxStats(Name,
                Volatile.Read(ref pendingCount),
                Volatile.Read(ref peakCount),
                Volatile.Read(ref lastDispatchCount),
                Volatile.Read(ref lastDispatchMs),
                Interlocked.Read(ref totalDispatched));
        }
    }

    internal readonly struct MailboxStats
    {
        public MailboxStats(string name, int pending, int peak, int lastDispatchCount, double lastDispatchMs, long totalDispatched)
        {
            Name = name;
            Pending = pending;
            Peak = peak;
            LastDispatchCount = lastDispatchCount;
            LastDispatchMs = lastDispatchMs;
            TotalDispatched = totalDispatched;
        }

        public string Name { get; }
        public int Pending { get; }
        public int Peak { get; }
        public int LastDispatchCount { get; }
        public double LastDispatchMs { get; }
        public long TotalDispatched { get; }
    }
}
