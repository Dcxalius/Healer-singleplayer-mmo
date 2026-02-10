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
                return new UiThreadStats(
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
                lock (HUDManager.UiLock)
                {
                    Mailboxes.Ui.DispatchAll();
                    if (doUpdate)
                    {
                        UiTextInputManager.Update();
                        StateManager.UiUpdate();
                        HUDManager.Update();
                    }
                    HUDManager.BuildDrawLists();
                }
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

    internal readonly struct UiThreadStats
    {
        public UiThreadStats(long totalFrames, long overrunCount, long waitTimeouts, double lastFrameMs, double avgFrameMs, double maxFrameMs)
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
