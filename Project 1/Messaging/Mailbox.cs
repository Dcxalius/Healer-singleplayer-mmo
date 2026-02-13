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
        readonly ConcurrentQueue<DispatchToken> dispatchQueue = new ConcurrentQueue<DispatchToken>();
        readonly ConcurrentDictionary<Type, IChannel> channelsByType = new ConcurrentDictionary<Type, IChannel>();
        readonly ConcurrentDictionary<int, IChannel> channelsById = new ConcurrentDictionary<int, IChannel>();
        readonly ConcurrentDictionary<Type, byte> coalescedTypes = new ConcurrentDictionary<Type, byte>();
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
        long totalCoalesced;
        long totalDropped;
        long totalMessageAgeTicks;
        long maxMessageAgeTicks;
        long lastOldestMessageAgeTicks;

        public Mailbox(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public void RegisterCoalescedType<T>()
        {
            Type messageType = typeof(T);
            if (!coalescedTypes.TryAdd(messageType, 0)) return;
            if (channelsByType.ContainsKey(messageType))
            {
                throw new InvalidOperationException($"Mailbox '{Name}' coalesced type '{messageType.Name}' must be registered before first publish/subscribe.");
            }
        }

        public void Publish<T>(in T message)
        {
            Channel<T> channel = GetOrAddChannel<T>();
            channel.Enqueue(message, out bool enqueueDispatchToken, out bool replacedPendingMessage);
            if (enqueueDispatchToken)
            {
                dispatchQueue.Enqueue(new DispatchToken(channel.Id, Stopwatch.GetTimestamp()));
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
            else if (replacedPendingMessage)
            {
                Interlocked.Increment(ref totalCoalesced);
                Interlocked.Increment(ref totalDropped);
            }
            Interlocked.Increment(ref totalPublished);
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
            long oldestMessageAgeTicks = 0;
            bool any = false;

            while (dispatchQueue.TryDequeue(out DispatchToken token))
            {
                if (!any)
                {
                    any = true;
                    startTicks = Stopwatch.GetTimestamp();
                }

                long messageAgeTicks = Math.Max(0, Stopwatch.GetTimestamp() - token.EnqueueTicks);
                if (messageAgeTicks > oldestMessageAgeTicks)
                {
                    oldestMessageAgeTicks = messageAgeTicks;
                }
                Interlocked.Add(ref totalMessageAgeTicks, messageAgeTicks);
                UpdateMax(ref maxMessageAgeTicks, messageAgeTicks);
                Interlocked.Decrement(ref pendingCount);
                Interlocked.Increment(ref totalDispatchDequeued);
                if (!channelsById.TryGetValue(token.ChannelId, out IChannel channel))
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
                Volatile.Write(ref lastOldestMessageAgeTicks, oldestMessageAgeTicks);
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
                Interlocked.Read(ref totalHandlerFailures),
                Interlocked.Read(ref totalCoalesced),
                Interlocked.Read(ref totalDropped),
                TicksToMs(Volatile.Read(ref lastOldestMessageAgeTicks)),
                ComputeAverageMessageAgeMs(),
                TicksToMs(Volatile.Read(ref maxMessageAgeTicks)));
        }

        Channel<T> GetOrAddChannel<T>()
        {
            IChannel channel = channelsByType.GetOrAdd(typeof(T), _ =>
            {
                int id = Interlocked.Increment(ref nextChannelId);
                bool coalesced = coalescedTypes.ContainsKey(typeof(T));
                Channel<T> created = new Channel<T>(id, coalesced);
                channelsById.TryAdd(id, created);
                return created;
            });
            return (Channel<T>)channel;
        }

        double ComputeAverageMessageAgeMs()
        {
            long dequeued = Interlocked.Read(ref totalDispatchDequeued);
            if (dequeued <= 0) return 0d;
            long totalAge = Interlocked.Read(ref totalMessageAgeTicks);
            return TicksToMs((double)totalAge / dequeued);
        }

        static double TicksToMs(double ticks)
        {
            return ticks * 1000d / Stopwatch.Frequency;
        }

        static void UpdateMax(ref long target, long value)
        {
            long snapshot;
            while (value > (snapshot = Volatile.Read(ref target)))
            {
                if (Interlocked.CompareExchange(ref target, value, snapshot) == snapshot)
                {
                    break;
                }
            }
        }

        interface IChannel
        {
            bool TryDispatchOne(string mailboxName, out bool hadSubscribers, out int handlerInvocations, out int handlerFailures);
        }

        sealed class Channel<T> : IChannel
        {
            readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
            readonly SubscriberList<T> subscribers = new SubscriberList<T>();
            readonly bool coalesced;
            readonly object coalescedGate = new object();
            bool hasPendingCoalescedMessage;
            T latestCoalescedMessage;

            public Channel(int id, bool coalesced)
            {
                Id = id;
                this.coalesced = coalesced;
            }

            public int Id { get; }

            public void Enqueue(in T message, out bool enqueueDispatchToken, out bool replacedPendingMessage)
            {
                if (!coalesced)
                {
                    queue.Enqueue(message);
                    enqueueDispatchToken = true;
                    replacedPendingMessage = false;
                    return;
                }

                lock (coalescedGate)
                {
                    replacedPendingMessage = hasPendingCoalescedMessage;
                    latestCoalescedMessage = message;
                    hasPendingCoalescedMessage = true;
                    enqueueDispatchToken = !replacedPendingMessage;
                }
            }

            public void Subscribe(Action<T> handler)
            {
                subscribers.Add(handler);
            }

            public bool TryDispatchOne(string mailboxName, out bool hadSubscribers, out int handlerInvocations, out int handlerFailures)
            {
                if (!TryDequeueMessage(out T message))
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

            bool TryDequeueMessage(out T message)
            {
                if (!coalesced)
                {
                    return queue.TryDequeue(out message);
                }

                lock (coalescedGate)
                {
                    if (!hasPendingCoalescedMessage)
                    {
                        message = default;
                        return false;
                    }

                    message = latestCoalescedMessage;
                    latestCoalescedMessage = default;
                    hasPendingCoalescedMessage = false;
                    return true;
                }
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

    readonly struct DispatchToken
    {
        public DispatchToken(int channelId, long enqueueTicks)
        {
            ChannelId = channelId;
            EnqueueTicks = enqueueTicks;
        }

        public int ChannelId { get; }
        public long EnqueueTicks { get; }
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
            long totalHandlerFailures,
            long totalCoalesced,
            long totalDropped,
            double lastOldestMessageAgeMs,
            double avgMessageAgeMs,
            double maxMessageAgeMs)
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
            TotalCoalesced = totalCoalesced;
            TotalDropped = totalDropped;
            LastOldestMessageAgeMs = lastOldestMessageAgeMs;
            AvgMessageAgeMs = avgMessageAgeMs;
            MaxMessageAgeMs = maxMessageAgeMs;
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
        public long TotalCoalesced { get; }
        public long TotalDropped { get; }
        public double LastOldestMessageAgeMs { get; }
        public double AvgMessageAgeMs { get; }
        public double MaxMessageAgeMs { get; }
    }
}
