using System.Diagnostics;
using System.Threading;

namespace Project_1.Managers
{
    internal static class ShadowRenderTelemetry
    {
        static long lastCandidateLights;
        static long lastActiveLights;
        static long lastDroppedLights;
        static long lastTotalSegments;
        static long lastMaxSegmentsPerLight;
        static long lastCappedLights;

        static long totalRenderSamples;
        static long totalRenderTicks;
        static long lastRenderTicks;
        static long maxRenderTicks;

        static long totalDrawCalls;
        static long lastDrawCalls;
        static long maxDrawCalls;

        static long totalRenderedLights;
        static long lastRenderedLights;
        static long maxRenderedLights;

        public static ShadowRenderStats Stats
        {
            get
            {
                long samples = Interlocked.Read(ref totalRenderSamples);
                long activeLights = Volatile.Read(ref lastActiveLights);
                long totalSegments = Volatile.Read(ref lastTotalSegments);
                double avgSegmentsPerLight = activeLights <= 0 ? 0d : (double)totalSegments / activeLights;
                return new ShadowRenderStats(
                    Volatile.Read(ref lastCandidateLights),
                    activeLights,
                    Volatile.Read(ref lastDroppedLights),
                    totalSegments,
                    avgSegmentsPerLight,
                    Volatile.Read(ref lastMaxSegmentsPerLight),
                    Volatile.Read(ref lastCappedLights),
                    samples,
                    TicksToMs(Volatile.Read(ref lastRenderTicks)),
                    samples == 0 ? 0d : TicksToMs((double)Interlocked.Read(ref totalRenderTicks) / samples),
                    TicksToMs(Volatile.Read(ref maxRenderTicks)),
                    Volatile.Read(ref lastDrawCalls),
                    samples == 0 ? 0d : (double)Interlocked.Read(ref totalDrawCalls) / samples,
                    Volatile.Read(ref maxDrawCalls),
                    Volatile.Read(ref lastRenderedLights),
                    samples == 0 ? 0d : (double)Interlocked.Read(ref totalRenderedLights) / samples,
                    Volatile.Read(ref maxRenderedLights));
            }
        }

        public static void RecordBuild(
            int candidateLights,
            int activeLights,
            int droppedLights,
            int totalSegments,
            int maxSegmentsPerLight,
            int cappedLights)
        {
            Volatile.Write(ref lastCandidateLights, candidateLights);
            Volatile.Write(ref lastActiveLights, activeLights);
            Volatile.Write(ref lastDroppedLights, droppedLights);
            Volatile.Write(ref lastTotalSegments, totalSegments);
            Volatile.Write(ref lastMaxSegmentsPerLight, maxSegmentsPerLight);
            Volatile.Write(ref lastCappedLights, cappedLights);
        }

        public static void RecordRender(long renderTicks, int drawCalls, int renderedLights)
        {
            long safeRenderTicks = renderTicks < 0 ? 0 : renderTicks;
            long safeDrawCalls = drawCalls < 0 ? 0 : drawCalls;
            long safeRenderedLights = renderedLights < 0 ? 0 : renderedLights;

            Volatile.Write(ref lastRenderTicks, safeRenderTicks);
            Volatile.Write(ref lastDrawCalls, safeDrawCalls);
            Volatile.Write(ref lastRenderedLights, safeRenderedLights);

            Interlocked.Increment(ref totalRenderSamples);
            Interlocked.Add(ref totalRenderTicks, safeRenderTicks);
            Interlocked.Add(ref totalDrawCalls, safeDrawCalls);
            Interlocked.Add(ref totalRenderedLights, safeRenderedLights);

            UpdateMax(ref maxRenderTicks, safeRenderTicks);
            UpdateMax(ref maxDrawCalls, safeDrawCalls);
            UpdateMax(ref maxRenderedLights, safeRenderedLights);
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
    }

    internal readonly struct ShadowRenderStats
    {
        public ShadowRenderStats(
            long candidateLights,
            long activeLights,
            long droppedLights,
            long totalSegments,
            double avgSegmentsPerLight,
            long maxSegmentsPerLight,
            long cappedLights,
            long renderSamples,
            double lastShadowPassMs,
            double avgShadowPassMs,
            double maxShadowPassMs,
            long lastDrawCalls,
            double avgDrawCalls,
            long maxDrawCalls,
            long lastRenderedLights,
            double avgRenderedLights,
            long maxRenderedLights)
        {
            CandidateLights = candidateLights;
            ActiveLights = activeLights;
            DroppedLights = droppedLights;
            TotalSegments = totalSegments;
            AvgSegmentsPerLight = avgSegmentsPerLight;
            MaxSegmentsPerLight = maxSegmentsPerLight;
            CappedLights = cappedLights;
            RenderSamples = renderSamples;
            LastShadowPassMs = lastShadowPassMs;
            AvgShadowPassMs = avgShadowPassMs;
            MaxShadowPassMs = maxShadowPassMs;
            LastDrawCalls = lastDrawCalls;
            AvgDrawCalls = avgDrawCalls;
            MaxDrawCalls = maxDrawCalls;
            LastRenderedLights = lastRenderedLights;
            AvgRenderedLights = avgRenderedLights;
            MaxRenderedLights = maxRenderedLights;
        }

        public long CandidateLights { get; }
        public long ActiveLights { get; }
        public long DroppedLights { get; }
        public long TotalSegments { get; }
        public double AvgSegmentsPerLight { get; }
        public long MaxSegmentsPerLight { get; }
        public long CappedLights { get; }
        public long RenderSamples { get; }
        public double LastShadowPassMs { get; }
        public double AvgShadowPassMs { get; }
        public double MaxShadowPassMs { get; }
        public long LastDrawCalls { get; }
        public double AvgDrawCalls { get; }
        public long MaxDrawCalls { get; }
        public long LastRenderedLights { get; }
        public double AvgRenderedLights { get; }
        public long MaxRenderedLights { get; }
    }
}
