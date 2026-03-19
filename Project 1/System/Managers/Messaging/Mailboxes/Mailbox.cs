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
        readonly ConcurrentQueue<DispatchToken> dispatchQueue = new ConcurrentQueue<DispatchToken>();
        readonly ConcurrentDictionary<Type, IMailboxChannel> channelsByType = new ConcurrentDictionary<Type, IMailboxChannel>();
        readonly ConcurrentDictionary<int, IMailboxChannel> channelsById = new ConcurrentDictionary<int, IMailboxChannel>();
        readonly ConcurrentDictionary<Type, byte> coalescedTypes = new ConcurrentDictionary<Type, byte>();
        readonly ConcurrentDictionary<Type, long> handlerFailuresByType = new ConcurrentDictionary<Type, long>();
        readonly ConcurrentDictionary<Type, long> coalescedByType = new ConcurrentDictionary<Type, long>();
        int handlerFailureTraceBudget = 32;
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
            MailboxChannel<T> channel = GetOrAddChannel<T>();
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
                coalescedByType.AddOrUpdate(typeof(T), 1, static (_, current) => current + 1);
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
                if (!channelsById.TryGetValue(token.ChannelId, out IMailboxChannel channel))
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
                    long delta = handlerFailures;
                    handlerFailuresByType.AddOrUpdate(channel.MessageType, delta, (_, current) => current + delta);
                    if (Interlocked.Decrement(ref handlerFailureTraceBudget) >= 0)
                    {
                        string trace = $"Mailbox '{Name}' counted handlerFail={handlerFailures} type='{channel.MessageType.Name}' handlers={handlerInvocations}.";
                        Console.WriteLine(trace);
                        Debug.WriteLine(trace);
                    }
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
            (string topHandlerFailureType, long topHandlerFailureCount) = GetTopTypeAndCount(handlerFailuresByType);
            (string topCoalescedType, long topCoalescedCount) = GetTopTypeAndCount(coalescedByType);
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
                topHandlerFailureType,
                topHandlerFailureCount,
                topCoalescedType,
                topCoalescedCount,
                TicksToMs(Volatile.Read(ref lastOldestMessageAgeTicks)),
                ComputeAverageMessageAgeMs(),
                TicksToMs(Volatile.Read(ref maxMessageAgeTicks)));
        }

        MailboxChannel<T> GetOrAddChannel<T>()
        {
            IMailboxChannel channel = channelsByType.GetOrAdd(typeof(T), _ =>
            {
                int id = Interlocked.Increment(ref nextChannelId);
                bool coalesced = coalescedTypes.ContainsKey(typeof(T));
                MailboxChannel<T> created = new MailboxChannel<T>(id, coalesced);
                channelsById.TryAdd(id, created);
                return created;
            });
            return (MailboxChannel<T>)channel;
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

        static (string TypeName, long Count) GetTopTypeAndCount(ConcurrentDictionary<Type, long> counts)
        {
            string typeName = "-";
            long maxCount = 0;
            foreach (KeyValuePair<Type, long> kvp in counts)
            {
                long count = kvp.Value;
                if (count <= maxCount) continue;
                maxCount = count;
                typeName = kvp.Key?.Name ?? "?";
            }

            return (typeName, maxCount);
        }
    }
}
