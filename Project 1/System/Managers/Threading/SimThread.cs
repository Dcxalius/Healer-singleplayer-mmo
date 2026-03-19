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
        const int maxCatchUpStepsPerLoop = 3;
        static Thread thread;
        static volatile bool running;
        static readonly AutoResetEvent pulse = new AutoResetEvent(false);
        static readonly long frameBudgetTicks = (long)(Stopwatch.Frequency * (frameBudgetMs / 1000d));
        static readonly long maxAccumulatedTicks = frameBudgetTicks * maxCatchUpStepsPerLoop;
        static long totalFrames;
        static long totalFrameTicks;
        static long lastFrameTicks;
        static long maxFrameTicks;
        static long totalDriftTicks;
        static long lastDriftTicks;
        static long maxPositiveDriftTicks;
        static long minNegativeDriftTicks;
        static long totalCatchUpSteps;
        static long lastCatchUpSteps;
        static long maxCatchUpSteps;
        static long overrunCount;

        public static bool IsRunning => running;
        public static SimThreadStats Stats
        {
            get
            {
                long frames = Interlocked.Read(ref totalFrames);
                long totalTicks = Interlocked.Read(ref totalFrameTicks);
                double avgMs = frames == 0 ? 0d : TicksToMs((double)totalTicks / frames);
                long totalDrift = Interlocked.Read(ref totalDriftTicks);
                return new SimThreadStats(
                    frames,
                    Interlocked.Read(ref overrunCount),
                    TicksToMs(Volatile.Read(ref lastFrameTicks)),
                    avgMs,
                    TicksToMs(Volatile.Read(ref maxFrameTicks)),
                    TicksToMs(Volatile.Read(ref lastDriftTicks)),
                    frames == 0 ? 0d : TicksToMs((double)totalDrift / frames),
                    TicksToMs(Volatile.Read(ref maxPositiveDriftTicks)),
                    TicksToMs(Volatile.Read(ref minNegativeDriftTicks)),
                    Volatile.Read(ref lastCatchUpSteps),
                    Interlocked.Read(ref totalCatchUpSteps),
                    Volatile.Read(ref maxCatchUpSteps));
            }
        }

        public static void Start()
        {
            ThreadAffinity.AssertMainThread();
            if (running) return;
            running = true;
            MailboxManager.RegisterSimCommandType<WorkerCompletionReady>();
            MailboxManager.Sim.Subscribe<WorkerCompletionReady>(e => WorkerPool.RunCompletion(e.CompletionId));
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
            Thread simThread = thread;
            if (simThread != null && simThread.IsAlive)
            {
                simThread.Join();
            }
            thread = null;
        }

        static void Run()
        {
            ThreadAffinity.RegisterSimThread();
            long previousTicks = Stopwatch.GetTimestamp();
            long accumulatorTicks = 0;
            while (running)
            {
                long nowTicks = Stopwatch.GetTimestamp();
                long deltaTicks = nowTicks - previousTicks;
                previousTicks = nowTicks;
                if (deltaTicks < 0) deltaTicks = 0;
                if (deltaTicks > maxAccumulatedTicks) deltaTicks = maxAccumulatedTicks;
                accumulatorTicks += deltaTicks;
                if (accumulatorTicks > maxAccumulatedTicks)
                {
                    accumulatorTicks = maxAccumulatedTicks;
                }

                int stepsExecuted = 0;
                while (running && accumulatorTicks >= frameBudgetTicks && stepsExecuted < maxCatchUpStepsPerLoop)
                {
                    RunOneStep();
                    accumulatorTicks -= frameBudgetTicks;
                    stepsExecuted++;
                }

                long catchUpSteps = stepsExecuted > 1 ? stepsExecuted - 1 : 0;
                Volatile.Write(ref lastCatchUpSteps, catchUpSteps);
                Interlocked.Add(ref totalCatchUpSteps, catchUpSteps);
                if (catchUpSteps > 0)
                {
                    UpdateMax(ref maxCatchUpSteps, catchUpSteps);
                }

                if (!running)
                {
                    break;
                }

                long waitTicks = frameBudgetTicks - accumulatorTicks;
                if (waitTicks <= 0)
                {
                    Thread.Yield();
                    continue;
                }

                int waitMs = (int)(waitTicks * 1000d / Stopwatch.Frequency);
                if (waitMs > 0)
                {
                    pulse.WaitOne(waitMs);
                }
                else
                {
                    Thread.Yield();
                }
            }
        }

        static void RunOneStep()
        {
            long startTicks = Stopwatch.GetTimestamp();
            MailboxManager.Sim.DispatchAll();
            StateManager.Update();
            MailboxManager.Sim.DispatchAll();
            DebugManager.Update();
            long elapsedTicks = Stopwatch.GetTimestamp() - startTicks;
            long driftTicks = elapsedTicks - frameBudgetTicks;
            Volatile.Write(ref lastFrameTicks, elapsedTicks);
            Volatile.Write(ref lastDriftTicks, driftTicks);
            Interlocked.Increment(ref totalFrames);
            Interlocked.Add(ref totalFrameTicks, elapsedTicks);
            Interlocked.Add(ref totalDriftTicks, driftTicks);
            if (elapsedTicks > frameBudgetTicks)
            {
                Interlocked.Increment(ref overrunCount);
            }
            if (driftTicks > 0)
            {
                UpdateMax(ref maxPositiveDriftTicks, driftTicks);
            }
            else if (driftTicks < 0)
            {
                UpdateMin(ref minNegativeDriftTicks, driftTicks);
            }
            UpdateMax(ref maxFrameTicks, elapsedTicks);
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

        static void UpdateMin(ref long target, long value)
        {
            long snapshot;
            while (value < (snapshot = Volatile.Read(ref target)))
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
        public SimThreadStats(
            long totalFrames,
            long overrunCount,
            double lastFrameMs,
            double avgFrameMs,
            double maxFrameMs,
            double lastDriftMs,
            double avgDriftMs,
            double maxPositiveDriftMs,
            double minNegativeDriftMs,
            long lastCatchUpSteps,
            long totalCatchUpSteps,
            long maxCatchUpSteps)
        {
            TotalFrames = totalFrames;
            OverrunCount = overrunCount;
            LastFrameMs = lastFrameMs;
            AvgFrameMs = avgFrameMs;
            MaxFrameMs = maxFrameMs;
            LastDriftMs = lastDriftMs;
            AvgDriftMs = avgDriftMs;
            MaxPositiveDriftMs = maxPositiveDriftMs;
            MinNegativeDriftMs = minNegativeDriftMs;
            LastCatchUpSteps = lastCatchUpSteps;
            TotalCatchUpSteps = totalCatchUpSteps;
            MaxCatchUpSteps = maxCatchUpSteps;
        }

        public long TotalFrames { get; }
        public long OverrunCount { get; }
        public double LastFrameMs { get; }
        public double AvgFrameMs { get; }
        public double MaxFrameMs { get; }
        public double LastDriftMs { get; }
        public double AvgDriftMs { get; }
        public double MaxPositiveDriftMs { get; }
        public double MinNegativeDriftMs { get; }
        public long LastCatchUpSteps { get; }
        public long TotalCatchUpSteps { get; }
        public long MaxCatchUpSteps { get; }
    }
}
