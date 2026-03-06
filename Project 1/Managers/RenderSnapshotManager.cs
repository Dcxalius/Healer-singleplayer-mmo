using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities.Corspes;
using Project_1.GameObjects.Entities.Projectiles;
using Project_1.GameObjects.Spawners;
using Project_1.Tiles;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.Threading;

namespace Project_1.Managers
{
    internal static class RenderSnapshotManager
    {
        static long totalBuilds;
        static long totalDraws;
        static long buildVersion;
        static long lastBuildTicks;
        static long lastDrawAgeTicks;
        static long totalDrawAgeTicks;
        static long maxDrawAgeTicks;
        static long lastDrawnBuildVersion = -1;
        static long staleDrawStreak;
        static long maxStaleDrawStreak;
        static long totalStaleDraws;

        public static RenderSnapshotSyncStats SyncStats
        {
            get
            {
                long draws = Interlocked.Read(ref totalDraws);
                long totalAgeTicks = Interlocked.Read(ref totalDrawAgeTicks);
                return new RenderSnapshotSyncStats(
                    Interlocked.Read(ref totalBuilds),
                    draws,
                    TicksToMs(Volatile.Read(ref lastDrawAgeTicks)),
                    draws == 0 ? 0d : TicksToMs((double)totalAgeTicks / draws),
                    TicksToMs(Volatile.Read(ref maxDrawAgeTicks)),
                    Volatile.Read(ref staleDrawStreak),
                    Volatile.Read(ref maxStaleDrawStreak),
                    Interlocked.Read(ref totalStaleDraws),
                    Volatile.Read(ref buildVersion),
                    Volatile.Read(ref lastDrawnBuildVersion));
            }
        }

        public static void BuildGameSnapshots()
        {
            ThreadAffinity.AssertSimThread();
            TileManager.BuildRenderSnapshot();
            ObjectManager.BuildRenderSnapshot();
            ShadowSnapshotManager.BuildSnapshot(ObjectManager.RenderLightSnapshot);
            Camera.Camera.BuildMinimapSnapshot();
            ProjectileManager.BuildRenderSnapshot();
            TileManager.BuildDoodadRenderSnapshots();
            CorpseManager.BuildRenderSnapshot();
            SpawnerManager.BuildRenderSnapshot();
            MinimapSnapshotManager.BuildSnapshot();

            Volatile.Write(ref lastBuildTicks, Stopwatch.GetTimestamp());
            Interlocked.Increment(ref totalBuilds);
            Interlocked.Increment(ref buildVersion);
        }

        public static void DrawGameSnapshots(SpriteBatch batch)
        {
            ThreadAffinity.AssertMainThread();
            long nowTicks = Stopwatch.GetTimestamp();
            long buildTicks = Volatile.Read(ref lastBuildTicks);
            if (buildTicks > 0)
            {
                long ageTicks = nowTicks - buildTicks;
                Volatile.Write(ref lastDrawAgeTicks, ageTicks);
                Interlocked.Add(ref totalDrawAgeTicks, ageTicks);
                UpdateMax(ref maxDrawAgeTicks, ageTicks);
            }

            long currentBuildVersion = Volatile.Read(ref buildVersion);
            long previousBuildVersion = Volatile.Read(ref lastDrawnBuildVersion);
            if (currentBuildVersion > 0 && previousBuildVersion == currentBuildVersion)
            {
                long streak = Interlocked.Increment(ref staleDrawStreak);
                Interlocked.Increment(ref totalStaleDraws);
                UpdateMax(ref maxStaleDrawStreak, streak);
            }
            else
            {
                Volatile.Write(ref staleDrawStreak, 0);
                Volatile.Write(ref lastDrawnBuildVersion, currentBuildVersion);
            }

            TileManager.DrawSnapshots(batch);
            ProjectileManager.DrawSnapshots(batch);
            ObjectManager.DrawSnapshots(batch);
            TileManager.DrawDoodadSnapshots(batch);
            CorpseManager.DrawSnapshots(batch);
            SpawnerManager.DrawSnapshots(batch);
            Interlocked.Increment(ref totalDraws);
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

    internal readonly struct RenderSnapshotSyncStats
    {
        public RenderSnapshotSyncStats(
            long totalBuilds,
            long totalDraws,
            double lastSnapshotAgeMs,
            double avgSnapshotAgeMs,
            double maxSnapshotAgeMs,
            long staleDrawStreak,
            long maxStaleDrawStreak,
            long totalStaleDraws,
            long currentBuildVersion,
            long lastDrawnBuildVersion)
        {
            TotalBuilds = totalBuilds;
            TotalDraws = totalDraws;
            LastSnapshotAgeMs = lastSnapshotAgeMs;
            AvgSnapshotAgeMs = avgSnapshotAgeMs;
            MaxSnapshotAgeMs = maxSnapshotAgeMs;
            StaleDrawStreak = staleDrawStreak;
            MaxStaleDrawStreak = maxStaleDrawStreak;
            TotalStaleDraws = totalStaleDraws;
            CurrentBuildVersion = currentBuildVersion;
            LastDrawnBuildVersion = lastDrawnBuildVersion;
        }

        public long TotalBuilds { get; }
        public long TotalDraws { get; }
        public double LastSnapshotAgeMs { get; }
        public double AvgSnapshotAgeMs { get; }
        public double MaxSnapshotAgeMs { get; }
        public long StaleDrawStreak { get; }
        public long MaxStaleDrawStreak { get; }
        public long TotalStaleDraws { get; }
        public long CurrentBuildVersion { get; }
        public long LastDrawnBuildVersion { get; }
    }
}
