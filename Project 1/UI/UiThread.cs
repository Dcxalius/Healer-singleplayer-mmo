using Project_1.Input;
using Project_1.Managers;
using Project_1.Managers.States;
using Project_1.Messaging;
using Project_1.UI.HUD.Managers;
using System.Diagnostics;
using System.Threading;

namespace Project_1.UI
{
    /// <summary>
    /// Background UI worker for dispatching UI events and updating UI state.
    /// </summary>
    internal static class UiThread
    {
        const double frameBudgetMs = 16d;
        static Thread thread;
        static volatile bool running;
        static volatile bool updateRequested;
        static readonly AutoResetEvent pulse = new AutoResetEvent(false);
        static readonly AutoResetEvent completed = new AutoResetEvent(false);
        static readonly long frameBudgetTicks = (long)(Stopwatch.Frequency * (frameBudgetMs / 1000d));
        static long totalFrames;
        static long totalFrameTicks;
        static long lastFrameTicks;
        static long maxFrameTicks;
        static long totalDispatchTicks;
        static long totalUiUpdateTicks;
        static long totalBuildTicks;
        static long lastDispatchTicks;
        static long lastUiUpdateTicks;
        static long lastBuildTicks;
        static long overrunCount;
        static long waitTimeouts;

        public static bool IsRunning => running;
        public static UiThreadStats Stats
        {
            get
            {
                long frames = Interlocked.Read(ref totalFrames);
                long totalTicks = Interlocked.Read(ref totalFrameTicks);
                double avgMs = frames == 0 ? 0d : TicksToMs((double)totalTicks / frames);
                long dispatchTicks = Interlocked.Read(ref totalDispatchTicks);
                long uiUpdateTicks = Interlocked.Read(ref totalUiUpdateTicks);
                long buildTicks = Interlocked.Read(ref totalBuildTicks);
                return new UiThreadStats(
                    frames,
                    Interlocked.Read(ref overrunCount),
                    Interlocked.Read(ref waitTimeouts),
                    TicksToMs(Volatile.Read(ref lastFrameTicks)),
                    avgMs,
                    TicksToMs(Volatile.Read(ref maxFrameTicks)),
                    TicksToMs(Volatile.Read(ref lastDispatchTicks)),
                    TicksToMs(Volatile.Read(ref lastUiUpdateTicks)),
                    TicksToMs(Volatile.Read(ref lastBuildTicks)),
                    frames == 0 ? 0d : TicksToMs((double)dispatchTicks / frames),
                    frames == 0 ? 0d : TicksToMs((double)uiUpdateTicks / frames),
                    frames == 0 ? 0d : TicksToMs((double)buildTicks / frames));
            }
        }

        public static void Start()
        {
            ThreadAffinity.AssertMainThread();
            if (running) return;
            running = true;
            thread = new Thread(Run)
            {
                IsBackground = true,
                Name = "UI Thread"
            };
            thread.Start();
        }

        public static void Stop()
        {
            ThreadAffinity.AssertMainThread();
            running = false;
            pulse.Set();
        }

        public static void Pulse(bool updateHud)
        {
            ThreadAffinity.AssertMainThread();
            if (!running) return;
            updateRequested |= updateHud;
            pulse.Set();
        }

        public static void PulseAndWait(bool updateHud, int timeoutMs = 16)
        {
            ThreadAffinity.AssertMainThread();
            if (!running) return;
            updateRequested |= updateHud;
            pulse.Set();
            if (!completed.WaitOne(timeoutMs))
            {
                Interlocked.Increment(ref waitTimeouts);
            }
        }

        static void Run()
        {
            ThreadAffinity.RegisterUiThread();
            while (running)
            {
                pulse.WaitOne();
                if (!running)
                {
                    completed.Set();
                    break;
                }
                long startTicks = Stopwatch.GetTimestamp();
                bool doUpdate = updateRequested;
                updateRequested = false;
                long dispatchTicks = 0;
                long uiUpdateTicks = 0;
                long buildTicks = 0;
                lock (HUDManager.UiLock)
                {
                    long phaseStart = Stopwatch.GetTimestamp();
                    Mailboxes.Ui.DispatchAll();
                    long afterDispatch = Stopwatch.GetTimestamp();
                    dispatchTicks = afterDispatch - phaseStart;
                    if (doUpdate)
                    {
                        UiTextInputManager.Update();
                        StateManager.UiUpdate();
                        HUDManager.Update();
                    }
                    long afterUiUpdate = Stopwatch.GetTimestamp();
                    uiUpdateTicks = afterUiUpdate - afterDispatch;
                    HUDManager.BuildDrawLists();
                    long afterBuild = Stopwatch.GetTimestamp();
                    buildTicks = afterBuild - afterUiUpdate;
                }
                long elapsedTicks = Stopwatch.GetTimestamp() - startTicks;
                Volatile.Write(ref lastFrameTicks, elapsedTicks);
                Volatile.Write(ref lastDispatchTicks, dispatchTicks);
                Volatile.Write(ref lastUiUpdateTicks, uiUpdateTicks);
                Volatile.Write(ref lastBuildTicks, buildTicks);
                Interlocked.Increment(ref totalFrames);
                Interlocked.Add(ref totalFrameTicks, elapsedTicks);
                Interlocked.Add(ref totalDispatchTicks, dispatchTicks);
                Interlocked.Add(ref totalUiUpdateTicks, uiUpdateTicks);
                Interlocked.Add(ref totalBuildTicks, buildTicks);
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

    internal readonly struct UiThreadStats
    {
        public UiThreadStats(
            long totalFrames,
            long overrunCount,
            long waitTimeouts,
            double lastFrameMs,
            double avgFrameMs,
            double maxFrameMs,
            double lastDispatchMs,
            double lastUiUpdateMs,
            double lastBuildMs,
            double avgDispatchMs,
            double avgUiUpdateMs,
            double avgBuildMs)
        {
            TotalFrames = totalFrames;
            OverrunCount = overrunCount;
            WaitTimeouts = waitTimeouts;
            LastFrameMs = lastFrameMs;
            AvgFrameMs = avgFrameMs;
            MaxFrameMs = maxFrameMs;
            LastDispatchMs = lastDispatchMs;
            LastUiUpdateMs = lastUiUpdateMs;
            LastBuildMs = lastBuildMs;
            AvgDispatchMs = avgDispatchMs;
            AvgUiUpdateMs = avgUiUpdateMs;
            AvgBuildMs = avgBuildMs;
        }

        public long TotalFrames { get; }
        public long OverrunCount { get; }
        public long WaitTimeouts { get; }
        public double LastFrameMs { get; }
        public double AvgFrameMs { get; }
        public double MaxFrameMs { get; }
        public double LastDispatchMs { get; }
        public double LastUiUpdateMs { get; }
        public double LastBuildMs { get; }
        public double AvgDispatchMs { get; }
        public double AvgUiUpdateMs { get; }
        public double AvgBuildMs { get; }
    }
}
