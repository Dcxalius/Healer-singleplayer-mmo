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
            public WorkItem(Action action)
            {
                Action = action;
            }

            public Action Action { get; }
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
        static readonly ConcurrentDictionary<int, Action> completions = new ConcurrentDictionary<int, Action>();
        static int nextCompletionId;

        public static bool IsRunning => running;
        public static WorkerPoolStats Stats => new WorkerPoolStats(
            System.Threading.Volatile.Read(ref pendingCount),
            System.Threading.Volatile.Read(ref peakCount),
            System.Threading.Interlocked.Read(ref totalEnqueued),
            System.Threading.Interlocked.Read(ref totalCompleted),
            System.Threading.Volatile.Read(ref lastWorkMs));

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

            queue.Add(new WorkItem(work));
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
                    item.Action?.Invoke();
                    double elapsedMs = (System.Diagnostics.Stopwatch.GetTimestamp() - startTicks) * 1000d / System.Diagnostics.Stopwatch.Frequency;
                    System.Threading.Volatile.Write(ref lastWorkMs, elapsedMs);
                    System.Threading.Interlocked.Increment(ref totalCompleted);
                }
                catch (Exception ex)
                {
                    DebugManager.Print(ex.ToString());
                }
            }
        }
    }

    internal readonly struct WorkerPoolStats
    {
        public WorkerPoolStats(int pending, int peak, long totalEnqueued, long totalCompleted, double lastWorkMs)
        {
            Pending = pending;
            Peak = peak;
            TotalEnqueued = totalEnqueued;
            TotalCompleted = totalCompleted;
            LastWorkMs = lastWorkMs;
        }

        public int Pending { get; }
        public int Peak { get; }
        public long TotalEnqueued { get; }
        public long TotalCompleted { get; }
        public double LastWorkMs { get; }
    }
}
