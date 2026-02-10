using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace Project_1.Messaging
{
    /// <summary>
    /// Thread-safe mailbox for passing messages to a designated thread. Dispatch should be called on the owner thread.
    /// </summary>
    internal sealed class Mailbox
    {
        readonly ConcurrentQueue<int> dispatchQueue = new ConcurrentQueue<int>();
        readonly ConcurrentDictionary<Type, IChannel> channelsByType = new ConcurrentDictionary<Type, IChannel>();
        readonly ConcurrentDictionary<int, IChannel> channelsById = new ConcurrentDictionary<int, IChannel>();
        int nextChannelId;
        int pendingCount;
        int peakCount;
        int lastDispatchCount;
        double lastDispatchMs;
        long totalDispatched;
        long totalPublished;
        long totalDispatchDequeued;
        long totalDispatchMisses;
        long totalWithoutSubscribers;
        long totalHandlerInvocations;
        long totalHandlerFailures;

        public Mailbox(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public void Publish<T>(in T message)
        {
            Channel<T> channel = GetOrAddChannel<T>();
            channel.Enqueue(message);
            dispatchQueue.Enqueue(channel.Id);
            Interlocked.Increment(ref totalPublished);

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
            GetOrAddChannel<T>().Subscribe(handler);
        }

        public void DispatchAll()
        {
            int processed = 0;
            long startTicks = 0;
            bool any = false;

            while (dispatchQueue.TryDequeue(out int channelId))
            {
                if (!any)
                {
                    any = true;
                    startTicks = Stopwatch.GetTimestamp();
                }

                Interlocked.Decrement(ref pendingCount);
                Interlocked.Increment(ref totalDispatchDequeued);
                if (!channelsById.TryGetValue(channelId, out IChannel channel))
                {
                    Interlocked.Increment(ref totalDispatchMisses);
                    continue;
                }
                if (!channel.TryDispatchOne(Name, out bool hadSubscribers, out int handlerInvocations, out int handlerFailures))
                {
                    Interlocked.Increment(ref totalDispatchMisses);
                    continue;
                }
                if (!hadSubscribers)
                {
                    Interlocked.Increment(ref totalWithoutSubscribers);
                    continue;
                }
                if (handlerInvocations > 0)
                {
                    Interlocked.Add(ref totalHandlerInvocations, handlerInvocations);
                }
                if (handlerFailures > 0)
                {
                    Interlocked.Add(ref totalHandlerFailures, handlerFailures);
                }
                processed++;
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
                Interlocked.Read(ref totalDispatched),
                Interlocked.Read(ref totalPublished),
                Interlocked.Read(ref totalDispatchDequeued),
                Interlocked.Read(ref totalDispatchMisses),
                Interlocked.Read(ref totalWithoutSubscribers),
                Interlocked.Read(ref totalHandlerInvocations),
                Interlocked.Read(ref totalHandlerFailures));
        }

        Channel<T> GetOrAddChannel<T>()
        {
            IChannel channel = channelsByType.GetOrAdd(typeof(T), _ =>
            {
                int id = Interlocked.Increment(ref nextChannelId);
                Channel<T> created = new Channel<T>(id);
                channelsById.TryAdd(id, created);
                return created;
            });
            return (Channel<T>)channel;
        }

        interface IChannel
        {
            bool TryDispatchOne(string mailboxName, out bool hadSubscribers, out int handlerInvocations, out int handlerFailures);
        }

        sealed class Channel<T> : IChannel
        {
            readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
            readonly SubscriberList<T> subscribers = new SubscriberList<T>();

            public Channel(int id)
            {
                Id = id;
            }

            public int Id { get; }

            public void Enqueue(in T message)
            {
                queue.Enqueue(message);
            }

            public void Subscribe(Action<T> handler)
            {
                subscribers.Add(handler);
            }

            public bool TryDispatchOne(string mailboxName, out bool hadSubscribers, out int handlerInvocations, out int handlerFailures)
            {
                if (!queue.TryDequeue(out T message))
                {
                    hadSubscribers = false;
                    handlerInvocations = 0;
                    handlerFailures = 0;
                    return false;
                }

                Action<T>[] handlers = subscribers.Snapshot();
                hadSubscribers = handlers.Length > 0;
                if (!hadSubscribers)
                {
                    handlerInvocations = 0;
                    handlerFailures = 0;
                    return true;
                }

                int failures = 0;
                for (int i = 0; i < handlers.Length; i++)
                {
                    try
                    {
                        handlers[i](message);
                    }
                    catch (Exception ex)
                    {
                        failures++;
                        Debug.WriteLine($"Mailbox '{mailboxName}' handler for '{typeof(T).Name}' threw and was skipped: {ex}");
                    }
                }

                handlerInvocations = handlers.Length;
                handlerFailures = failures;
                return true;
            }
        }

        sealed class SubscriberList<T>
        {
            readonly object gate = new object();
            Action<T>[] handlers = Array.Empty<Action<T>>();

            public void Add(Action<T> handler)
            {
                lock (gate)
                {
                    Action<T>[] next = new Action<T>[handlers.Length + 1];
                    Array.Copy(handlers, next, handlers.Length);
                    next[handlers.Length] = handler;
                    Volatile.Write(ref handlers, next);
                }
            }

            public Action<T>[] Snapshot()
            {
                return Volatile.Read(ref handlers);
            }
        }
    }

    internal readonly struct MailboxStats
    {
        public MailboxStats(
            string name,
            int pending,
            int peak,
            int lastDispatchCount,
            double lastDispatchMs,
            long totalDispatched,
            long totalPublished,
            long totalDispatchDequeued,
            long totalDispatchMisses,
            long totalWithoutSubscribers,
            long totalHandlerInvocations,
            long totalHandlerFailures)
        {
            Name = name;
            Pending = pending;
            Peak = peak;
            LastDispatchCount = lastDispatchCount;
            LastDispatchMs = lastDispatchMs;
            TotalDispatched = totalDispatched;
            TotalPublished = totalPublished;
            TotalDispatchDequeued = totalDispatchDequeued;
            TotalDispatchMisses = totalDispatchMisses;
            TotalWithoutSubscribers = totalWithoutSubscribers;
            TotalHandlerInvocations = totalHandlerInvocations;
            TotalHandlerFailures = totalHandlerFailures;
        }

        public string Name { get; }
        public int Pending { get; }
        public int Peak { get; }
        public int LastDispatchCount { get; }
        public double LastDispatchMs { get; }
        public long TotalDispatched { get; }
        public long TotalPublished { get; }
        public long TotalDispatchDequeued { get; }
        public long TotalDispatchMisses { get; }
        public long TotalWithoutSubscribers { get; }
        public long TotalHandlerInvocations { get; }
        public long TotalHandlerFailures { get; }
    }
}
