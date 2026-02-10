using Project_1.Managers.States;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using System.Diagnostics;
using System.Threading;

namespace Project_1.Managers
{
    /// <summary>
    /// Background simulation worker for processing main-thread commands and running game updates.
    /// </summary>
    internal static class SimThread
    {
        const double frameBudgetMs = 16d;
        static Thread thread;
        static volatile bool running;
        static readonly AutoResetEvent pulse = new AutoResetEvent(false);
        static readonly AutoResetEvent completed = new AutoResetEvent(false);
        static readonly long frameBudgetTicks = (long)(Stopwatch.Frequency * (frameBudgetMs / 1000d));
        static long totalFrames;
        static long totalFrameTicks;
        static long lastFrameTicks;
        static long maxFrameTicks;
        static long overrunCount;
        static long waitTimeouts;

        public static bool IsRunning => running;
        public static SimThreadStats Stats
        {
            get
            {
                long frames = Interlocked.Read(ref totalFrames);
                long totalTicks = Interlocked.Read(ref totalFrameTicks);
                double avgMs = frames == 0 ? 0d : TicksToMs((double)totalTicks / frames);
                return new SimThreadStats(
                    frames,
                    Interlocked.Read(ref overrunCount),
                    Interlocked.Read(ref waitTimeouts),
                    TicksToMs(Volatile.Read(ref lastFrameTicks)),
                    avgMs,
                    TicksToMs(Volatile.Read(ref maxFrameTicks)));
            }
        }

        public static void Start()
        {
            ThreadAffinity.AssertMainThread();
            if (running) return;
            running = true;
            Mailboxes.RegisterSimCommandType<WorkerCompletionReady>();
            Mailboxes.Sim.Subscribe<WorkerCompletionReady>(e => WorkerPool.RunCompletion(e.CompletionId));
            thread = new Thread(Run)
            {
                IsBackground = true,
                Name = "Sim Thread"
            };
            thread.Start();
        }

        public static void Stop()
        {
            ThreadAffinity.AssertMainThread();
            running = false;
            pulse.Set();
        }

        public static void PulseAndWait(int timeoutMs = 16)
        {
            ThreadAffinity.AssertMainThread();
            if (!running) return;
            pulse.Set();
            if (!completed.WaitOne(timeoutMs))
            {
                Interlocked.Increment(ref waitTimeouts);
            }
        }

        static void Run()
        {
            ThreadAffinity.RegisterSimThread();
            while (running)
            {
                pulse.WaitOne();
                if (!running)
                {
                    completed.Set();
                    break;
                }
                long startTicks = Stopwatch.GetTimestamp();
                Mailboxes.Sim.DispatchAll();
                StateManager.Update();
                Mailboxes.Sim.DispatchAll();
                DebugManager.Update();
                long elapsedTicks = Stopwatch.GetTimestamp() - startTicks;
                Volatile.Write(ref lastFrameTicks, elapsedTicks);
                Interlocked.Increment(ref totalFrames);
                Interlocked.Add(ref totalFrameTicks, elapsedTicks);
                if (elapsedTicks > frameBudgetTicks)
                {
                    Interlocked.Increment(ref overrunCount);
                }
                UpdateMax(ref maxFrameTicks, elapsedTicks);
                completed.Set();
            }
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

        static double TicksToMs(double ticks)
        {
            return ticks * 1000d / Stopwatch.Frequency;
        }
    }

    internal readonly struct SimThreadStats
    {
        public SimThreadStats(long totalFrames, long overrunCount, long waitTimeouts, double lastFrameMs, double avgFrameMs, double maxFrameMs)
        {
            TotalFrames = totalFrames;
            OverrunCount = overrunCount;
            WaitTimeouts = waitTimeouts;
            LastFrameMs = lastFrameMs;
            AvgFrameMs = avgFrameMs;
            MaxFrameMs = maxFrameMs;
        }

        public long TotalFrames { get; }
        public long OverrunCount { get; }
        public long WaitTimeouts { get; }
        public double LastFrameMs { get; }
        public double AvgFrameMs { get; }
        public double MaxFrameMs { get; }
    }
}
