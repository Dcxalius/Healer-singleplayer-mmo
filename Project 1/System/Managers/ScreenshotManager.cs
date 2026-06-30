using Project_1.Managers.Saves;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Project_1.Managers
{
    internal static class ScreenshotManager
    {
        //TODO: There should be a keybind that screenshots the game and saves it to a Screenshot folder. This system should use the final screen rendered and not only gameplay
        //TODO: The other responsibility is creating screenshots of saved games, this screenshot should just be in game screenshots.
        static readonly object screenshotLock = new object();
        static readonly Queue<Save> pendingScreenshots = new Queue<Save>();
        static int pendingScreenshotCount;
        static int pendingScreenshotPeak;
        static long totalScreenshotsEnqueued;
        static long totalScreenshotsProcessed;
        static double lastScreenshotMs;

        public static ScreenshotQueueStats QueueStats => new ScreenshotQueueStats(
            Volatile.Read(ref pendingScreenshotCount),
            Volatile.Read(ref pendingScreenshotPeak),
            Interlocked.Read(ref totalScreenshotsEnqueued),
            Interlocked.Read(ref totalScreenshotsProcessed),
            Volatile.Read(ref lastScreenshotMs));

        public static void RequestScreenshot(Save save)
        {
            ThreadAffinity.AssertSimThread();
            if (save == null) return;
            lock (screenshotLock)
            {
                pendingScreenshots.Enqueue(save);
            }

            int pending = Interlocked.Increment(ref pendingScreenshotCount);
            Interlocked.Increment(ref totalScreenshotsEnqueued);
            int snapshotPeak;
            while (pending > (snapshotPeak = Volatile.Read(ref pendingScreenshotPeak)))
            {
                if (Interlocked.CompareExchange(ref pendingScreenshotPeak, pending, snapshotPeak) == snapshotPeak)
                {
                    break;
                }
            }
        }

        public static void ProcessPendingScreenshots()
        {
            ThreadAffinity.AssertMainThread();
            while (true)
            {
                Save save;
                lock (screenshotLock)
                {
                    if (pendingScreenshots.Count == 0) return;
                    save = pendingScreenshots.Dequeue();
                }

                Interlocked.Decrement(ref pendingScreenshotCount);
                if (save == null) continue;
                try
                {
                    long startTicks = Stopwatch.GetTimestamp();
                    save.SaveScreenshot();
                    double elapsedMs = (Stopwatch.GetTimestamp() - startTicks) * 1000d / Stopwatch.Frequency;
                    Volatile.Write(ref lastScreenshotMs, elapsedMs);
                    Interlocked.Increment(ref totalScreenshotsProcessed);
                }
                catch (Exception ex) when (SaveManager.TryBuildSaveFailureMessage(ex, out _))
                {
                    //Q: Do we really want to mark it as failed if just the screenshot fails? Perhaps a different fail message and just using a default image sounds cleaner
                    SaveManager.NotifySaveFailed(ex);
                }
            }
        }

        internal static void RemovePendingScreenshots(Save save)
        {
            //Q: I assume we do this so if a save is saved, but before the screenshot is captured, deleted. Is this possible though? Even with tas?
            //Not saying we shouldn't do this, just something to keep in mind and check.
            lock (screenshotLock)
            {
                if (pendingScreenshots.Count == 0) return;

                Queue<Save> remainingScreenshots = new Queue<Save>(pendingScreenshots.Count);
                int removedCount = 0;
                while (pendingScreenshots.Count > 0)
                {
                    Save pendingSave = pendingScreenshots.Dequeue();
                    if (ReferenceEquals(pendingSave, save))
                    {
                        removedCount++;
                        continue;
                    }

                    remainingScreenshots.Enqueue(pendingSave);
                }

                while (remainingScreenshots.Count > 0)
                {
                    pendingScreenshots.Enqueue(remainingScreenshots.Dequeue());
                }

                if (removedCount > 0)
                {
                    Interlocked.Add(ref pendingScreenshotCount, -removedCount);
                }
            }
        }
    }

    internal readonly struct ScreenshotQueueStats
    {
        public ScreenshotQueueStats(int pending, int peak, long totalEnqueued, long totalProcessed, double lastScreenshotMs)
        {
            Pending = pending;
            Peak = peak;
            TotalEnqueued = totalEnqueued;
            TotalProcessed = totalProcessed;
            LastScreenshotMs = lastScreenshotMs;
        }

        public int Pending { get; }
        public int Peak { get; }
        public long TotalEnqueued { get; }
        public long TotalProcessed { get; }
        public double LastScreenshotMs { get; }
    }
}
