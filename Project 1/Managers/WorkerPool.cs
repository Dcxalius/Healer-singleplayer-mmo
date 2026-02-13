using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Project_1.Messaging;
using Project_1.Messaging.Events;

namespace Project_1.Managers
{
    internal static class WorkerPool
    {
        sealed class WorkItem
        {
            public WorkItem(Action action, long enqueueTicks)
            {
                Action = action;
                EnqueueTicks = enqueueTicks;
            }

            public Action Action { get; }
            public long EnqueueTicks { get; }
        }

        static readonly object startLock = new object();
        static BlockingCollection<WorkItem> queue;
        static List<Thread> workers;
        static volatile bool running;
        static int pendingCount;
        static int peakCount;
        static long totalEnqueued;
        static long totalCompleted;
        static double lastWorkMs;
        static long totalQueueWaitTicks;
        static long totalWorkTicks;
        static long totalLatencyTicks;
        static long lastQueueWaitTicks;
        static long lastWorkTicks;
        static long lastLatencyTicks;
        static long maxQueueWaitTicks;
        static long maxWorkTicks;
        static long maxLatencyTicks;
        static readonly ConcurrentDictionary<int, Action> completions = new ConcurrentDictionary<int, Action>();
        static int nextCompletionId;

        public static bool IsRunning => running;
        public static WorkerPoolStats Stats
        {
            get
            {
                long completed = System.Threading.Interlocked.Read(ref totalCompleted);
                return new WorkerPoolStats(
                    System.Threading.Volatile.Read(ref pendingCount),
                    System.Threading.Volatile.Read(ref peakCount),
                    System.Threading.Interlocked.Read(ref totalEnqueued),
                    completed,
                    TicksToMs(System.Threading.Volatile.Read(ref lastQueueWaitTicks)),
                    TicksToMs(System.Threading.Volatile.Read(ref lastWorkTicks)),
                    TicksToMs(System.Threading.Volatile.Read(ref lastLatencyTicks)),
                    completed == 0 ? 0d : TicksToMs((double)System.Threading.Interlocked.Read(ref totalQueueWaitTicks) / completed),
                    completed == 0 ? 0d : TicksToMs((double)System.Threading.Interlocked.Read(ref totalWorkTicks) / completed),
                    completed == 0 ? 0d : TicksToMs((double)System.Threading.Interlocked.Read(ref totalLatencyTicks) / completed),
                    TicksToMs(System.Threading.Volatile.Read(ref maxQueueWaitTicks)),
                    TicksToMs(System.Threading.Volatile.Read(ref maxWorkTicks)),
                    TicksToMs(System.Threading.Volatile.Read(ref maxLatencyTicks)));
            }
        }

        public static void Start(int? workerCount = null)
        {
            ThreadAffinity.AssertMainThread();
            lock (startLock)
            {
                if (running) return;
                running = true;
                pendingCount = 0;
                peakCount = 0;
                totalEnqueued = 0;
                totalCompleted = 0;
                lastWorkMs = 0;
                totalQueueWaitTicks = 0;
                totalWorkTicks = 0;
                totalLatencyTicks = 0;
                lastQueueWaitTicks = 0;
                lastWorkTicks = 0;
                lastLatencyTicks = 0;
                maxQueueWaitTicks = 0;
                maxWorkTicks = 0;
                maxLatencyTicks = 0;
                nextCompletionId = 0;
                completions.Clear();

                int count = workerCount ?? Math.Max(1, Environment.ProcessorCount - 1);
                queue = new BlockingCollection<WorkItem>();
                workers = new List<Thread>(count);

                for (int i = 0; i < count; i++)
                {
                    Thread thread = new Thread(Run)
                    {
                        IsBackground = true,
                        Name = $"Worker {i + 1}"
                    };
                    workers.Add(thread);
                    thread.Start();
                }
            }
        }

        public static void Stop()
        {
            ThreadAffinity.AssertMainThread();
            lock (startLock)
            {
                if (!running) return;
                running = false;
                queue.CompleteAdding();
            }

            for (int i = 0; i < workers.Count; i++)
            {
                workers[i].Join();
            }
        }

        public static void Enqueue(Action work)
        {
            if (work == null) return;

            if (!running)
            {
                work();
                return;
            }

            queue.Add(new WorkItem(work, System.Diagnostics.Stopwatch.GetTimestamp()));
            int pending = System.Threading.Interlocked.Increment(ref pendingCount);
            System.Threading.Interlocked.Increment(ref totalEnqueued);
            int snapshotPeak;
            while (pending > (snapshotPeak = System.Threading.Volatile.Read(ref peakCount)))
            {
                if (System.Threading.Interlocked.CompareExchange(ref peakCount, pending, snapshotPeak) == snapshotPeak)
                {
                    break;
                }
            }
        }

        public static void Enqueue<T>(Func<T> work, Action<T> onComplete)
        {
            Enqueue(() =>
            {
                T result = work != null ? work() : default;
                if (onComplete != null)
                {
                    int completionId = Interlocked.Increment(ref nextCompletionId);
                    completions[completionId] = () => onComplete(result);
                    Mailboxes.PublishSimCommand(new WorkerCompletionReady(completionId));
                }
            });
        }

        public static void RunCompletion(int completionId)
        {
            ThreadAffinity.AssertSimThread();
            if (!completions.TryRemove(completionId, out Action completion)) return;
            completion?.Invoke();
        }

        static void Run()
        {
            foreach (WorkItem item in queue.GetConsumingEnumerable())
            {
                try
                {
                    System.Threading.Interlocked.Decrement(ref pendingCount);
                    long startTicks = System.Diagnostics.Stopwatch.GetTimestamp();
                    long queueWaitTicks = Math.Max(0, startTicks - item.EnqueueTicks);
                    item.Action?.Invoke();
                    long endTicks = System.Diagnostics.Stopwatch.GetTimestamp();
                    long workTicks = Math.Max(0, endTicks - startTicks);
                    long latencyTicks = Math.Max(0, endTicks - item.EnqueueTicks);

                    System.Threading.Volatile.Write(ref lastQueueWaitTicks, queueWaitTicks);
                    System.Threading.Volatile.Write(ref lastWorkTicks, workTicks);
                    System.Threading.Volatile.Write(ref lastLatencyTicks, latencyTicks);
                    System.Threading.Interlocked.Add(ref totalQueueWaitTicks, queueWaitTicks);
                    System.Threading.Interlocked.Add(ref totalWorkTicks, workTicks);
                    System.Threading.Interlocked.Add(ref totalLatencyTicks, latencyTicks);
                    UpdateMax(ref maxQueueWaitTicks, queueWaitTicks);
                    UpdateMax(ref maxWorkTicks, workTicks);
                    UpdateMax(ref maxLatencyTicks, latencyTicks);

                    double elapsedMs = TicksToMs(workTicks);
                    System.Threading.Volatile.Write(ref lastWorkMs, elapsedMs);
                    System.Threading.Interlocked.Increment(ref totalCompleted);
                }
                catch (Exception ex)
                {
                    DebugManager.Print(ex.ToString());
                }
            }
        }

        static void UpdateMax(ref long target, long value)
        {
            long snapshot;
            while (value > (snapshot = System.Threading.Volatile.Read(ref target)))
            {
                if (System.Threading.Interlocked.CompareExchange(ref target, value, snapshot) == snapshot)
                {
                    break;
                }
            }
        }

        static double TicksToMs(double ticks)
        {
            return ticks * 1000d / System.Diagnostics.Stopwatch.Frequency;
        }
    }

    internal readonly struct WorkerPoolStats
    {
        public WorkerPoolStats(
            int pending,
            int peak,
            long totalEnqueued,
            long totalCompleted,
            double lastQueueWaitMs,
            double lastWorkMs,
            double lastLatencyMs,
            double avgQueueWaitMs,
            double avgWorkMs,
            double avgLatencyMs,
            double maxQueueWaitMs,
            double maxWorkMs,
            double maxLatencyMs)
        {
            Pending = pending;
            Peak = peak;
            TotalEnqueued = totalEnqueued;
            TotalCompleted = totalCompleted;
            LastQueueWaitMs = lastQueueWaitMs;
            LastWorkMs = lastWorkMs;
            LastLatencyMs = lastLatencyMs;
            AvgQueueWaitMs = avgQueueWaitMs;
            AvgWorkMs = avgWorkMs;
            AvgLatencyMs = avgLatencyMs;
            MaxQueueWaitMs = maxQueueWaitMs;
            MaxWorkMs = maxWorkMs;
            MaxLatencyMs = maxLatencyMs;
        }

        public int Pending { get; }
        public int Peak { get; }
        public long TotalEnqueued { get; }
        public long TotalCompleted { get; }
        public double LastQueueWaitMs { get; }
        public double LastWorkMs { get; }
        public double LastLatencyMs { get; }
        public double AvgQueueWaitMs { get; }
        public double AvgWorkMs { get; }
        public double AvgLatencyMs { get; }
        public double MaxQueueWaitMs { get; }
        public double MaxWorkMs { get; }
        public double MaxLatencyMs { get; }
    }
}
