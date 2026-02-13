using System.Diagnostics;
using System.Threading;

namespace Project_1.Managers
{
    internal static class MainRenderTelemetry
    {
        static long totalFrames;
        static long totalFrameTicks;
        static long lastFrameTicks;
        static long maxFrameTicks;

        static long totalUiBuildTicks;
        static long lastUiBuildTicks;
        static long maxUiBuildTicks;

        static long totalWorldDrawTicks;
        static long lastWorldDrawTicks;
        static long maxWorldDrawTicks;

        static long totalUiCompositeTicks;
        static long lastUiCompositeTicks;
        static long maxUiCompositeTicks;

        public static MainRenderStats Stats
        {
            get
            {
                long frames = Interlocked.Read(ref totalFrames);
                return new MainRenderStats(
                    frames,
                    TicksToMs(Volatile.Read(ref lastFrameTicks)),
                    frames == 0 ? 0d : TicksToMs((double)Interlocked.Read(ref totalFrameTicks) / frames),
                    TicksToMs(Volatile.Read(ref maxFrameTicks)),
                    TicksToMs(Volatile.Read(ref lastUiBuildTicks)),
                    frames == 0 ? 0d : TicksToMs((double)Interlocked.Read(ref totalUiBuildTicks) / frames),
                    TicksToMs(Volatile.Read(ref maxUiBuildTicks)),
                    TicksToMs(Volatile.Read(ref lastWorldDrawTicks)),
                    frames == 0 ? 0d : TicksToMs((double)Interlocked.Read(ref totalWorldDrawTicks) / frames),
                    TicksToMs(Volatile.Read(ref maxWorldDrawTicks)),
                    TicksToMs(Volatile.Read(ref lastUiCompositeTicks)),
                    frames == 0 ? 0d : TicksToMs((double)Interlocked.Read(ref totalUiCompositeTicks) / frames),
                    TicksToMs(Volatile.Read(ref maxUiCompositeTicks)));
            }
        }

        public static void RecordFrame(long totalTicks, long uiBuildTicks, long worldDrawTicks, long uiCompositeTicks)
        {
            Volatile.Write(ref lastFrameTicks, totalTicks);
            Volatile.Write(ref lastUiBuildTicks, uiBuildTicks);
            Volatile.Write(ref lastWorldDrawTicks, worldDrawTicks);
            Volatile.Write(ref lastUiCompositeTicks, uiCompositeTicks);

            Interlocked.Increment(ref totalFrames);
            Interlocked.Add(ref totalFrameTicks, totalTicks);
            Interlocked.Add(ref totalUiBuildTicks, uiBuildTicks);
            Interlocked.Add(ref totalWorldDrawTicks, worldDrawTicks);
            Interlocked.Add(ref totalUiCompositeTicks, uiCompositeTicks);

            UpdateMax(ref maxFrameTicks, totalTicks);
            UpdateMax(ref maxUiBuildTicks, uiBuildTicks);
            UpdateMax(ref maxWorldDrawTicks, worldDrawTicks);
            UpdateMax(ref maxUiCompositeTicks, uiCompositeTicks);
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

    internal readonly struct MainRenderStats
    {
        public MainRenderStats(
            long totalFrames,
            double lastFrameMs,
            double avgFrameMs,
            double maxFrameMs,
            double lastUiBuildMs,
            double avgUiBuildMs,
            double maxUiBuildMs,
            double lastWorldDrawMs,
            double avgWorldDrawMs,
            double maxWorldDrawMs,
            double lastUiCompositeMs,
            double avgUiCompositeMs,
            double maxUiCompositeMs)
        {
            TotalFrames = totalFrames;
            LastFrameMs = lastFrameMs;
            AvgFrameMs = avgFrameMs;
            MaxFrameMs = maxFrameMs;
            LastUiBuildMs = lastUiBuildMs;
            AvgUiBuildMs = avgUiBuildMs;
            MaxUiBuildMs = maxUiBuildMs;
            LastWorldDrawMs = lastWorldDrawMs;
            AvgWorldDrawMs = avgWorldDrawMs;
            MaxWorldDrawMs = maxWorldDrawMs;
            LastUiCompositeMs = lastUiCompositeMs;
            AvgUiCompositeMs = avgUiCompositeMs;
            MaxUiCompositeMs = maxUiCompositeMs;
        }

        public long TotalFrames { get; }
        public double LastFrameMs { get; }
        public double AvgFrameMs { get; }
        public double MaxFrameMs { get; }
        public double LastUiBuildMs { get; }
        public double AvgUiBuildMs { get; }
        public double MaxUiBuildMs { get; }
        public double LastWorldDrawMs { get; }
        public double AvgWorldDrawMs { get; }
        public double MaxWorldDrawMs { get; }
        public double LastUiCompositeMs { get; }
        public double AvgUiCompositeMs { get; }
        public double MaxUiCompositeMs { get; }
    }
}
