using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Managers.Saves;
using Project_1.Messaging;
using Project_1.Textures;
using Project_1.Tiles;
using Project_1.UI;

namespace Project_1.Managers
{
    internal static partial class DebugManager
    {
        static Text fpsText;
        static Text frameTimeText;
        static Text totalTimeText;
        static Text mailboxText;
        static Text dispatchText;
        static Text workerText;
        static Text screenshotText;
        static Text simThreadText;
        static Text uiThreadText;
        static Text renderSyncText;
        static AbsoluteScreenPosition debugTextOrigin;
        const int QueueWarningThreshold = 128;
        const int SnapshotStaleDrawWarningThreshold = 5;
        const double ThreadFrameWarningMs = 25d;
        const double SimDriftWarningMs = 6d;
        const double MailboxAgeWarningMs = 50d;
        const double WorkerLatencyWarningMs = 100d;
        const double RenderFrameWarningMs = 25d;
        const double SnapshotAgeWarningMs = 50d;
        const double WarningCooldownMs = 2000d;
        const long CoalescedWarningDeltaThreshold = 64;
        const int OverlaySectionSpacingPx = 6;
        static double nextQueueWarningMs;
        static double nextQueueAgeWarningMs;
        static double nextFailureWarningMs;
        static double nextCoalesceWarningMs;
        static double nextThreadWarningMs;
        static double nextWorkerWarningMs;
        static double nextRenderWarningMs;
        static double nextSnapshotWarningMs;
        static long lastMainFailureCount;
        static long lastUiFailureCount;
        static long lastSimFailureCount;
        static long lastMainCoalescedCount;
        static long lastUiCoalescedCount;
        static long lastSimCoalescedCount;
        static long lastMainDroppedCount;
        static long lastUiDroppedCount;
        static long lastSimDroppedCount;
        static long lastSimOverrunCount;
        static long lastUiOverrunCount;
        static long lastUiTimeoutCount;

        public static void LoadContent()
        {
            ThreadAffinity.AssertMainThread();
            fpsText = new Text("Comfortaa-msdf", Color.Chartreuse);
            frameTimeText = new Text("Comfortaa-msdf", Color.Chartreuse);
            totalTimeText = new Text("Comfortaa-msdf", Color.Chartreuse);
            mailboxText = new Text("Comfortaa-msdf", Color.Chartreuse);
            dispatchText = new Text("Comfortaa-msdf", Color.Chartreuse);
            workerText = new Text("Comfortaa-msdf", Color.Chartreuse);
            screenshotText = new Text("Comfortaa-msdf", Color.Chartreuse);
            simThreadText = new Text("Comfortaa-msdf", Color.Chartreuse);
            uiThreadText = new Text("Comfortaa-msdf", Color.Chartreuse);
            renderSyncText = new Text("Comfortaa-msdf", Color.Chartreuse);
            debugTextOrigin = new AbsoluteScreenPosition(12, 12);
        }

        static void UpdateOverlayText()
        {
            if (!Mode(DebugMode.DebugOverlay)) return;

            double deltaSeconds = TimeManager.SecondsSinceLastFrame;
            double fps = deltaSeconds > 0 ? 1.0 / deltaSeconds : 0;
            double frameTimeMs = deltaSeconds * 1000.0;
            TimeSpan totalTime = TimeManager.InstanceTotalFrameTimeAsTimeSpan;

            fpsText.Value = $"FPS: {fps,6:0.0}";
            frameTimeText.Value = $"Frame Time: {frameTimeMs,7:0.00} ms";
            totalTimeText.Value = $"Session Time: {totalTime:hh\\:mm\\:ss}";

            MailboxStats mainStats = MailboxManager.MainStats;
            MailboxStats uiStats = MailboxManager.UiStats;
            MailboxStats simStats = MailboxManager.SimStats;
            mailboxText.Value =
                "Mailbox Queue Depth (pending/peak)\n" +
                $"  Main: {mainStats.Pending,4}/{mainStats.Peak,4}\n" +
                $"  UI  : {uiStats.Pending,4}/{uiStats.Peak,4}\n" +
                $"  Sim : {simStats.Pending,4}/{simStats.Peak,4}";
            dispatchText.Value =
                "Mailbox Dispatch\n" +
                BuildMailboxDispatchText("Main", mainStats) + "\n" +
                BuildMailboxDispatchText("UI", uiStats) + "\n" +
                BuildMailboxDispatchText("Sim", simStats);

            WorkerPoolStats workerStats = WorkerPool.Stats;
            workerText.Value =
                "Worker Pool\n" +
                $"  Queue depth (pending/peak): {workerStats.Pending,4}/{workerStats.Peak,4}\n" +
                $"  Last ms queue/work/latency: {workerStats.LastQueueWaitMs,6:0.0} / {workerStats.LastWorkMs,6:0.0} / {workerStats.LastLatencyMs,6:0.0}\n" +
                $"  Avg  ms queue/work/latency: {workerStats.AvgQueueWaitMs,6:0.0} / {workerStats.AvgWorkMs,6:0.0} / {workerStats.AvgLatencyMs,6:0.0}\n" +
                $"  Max  ms queue/work/latency: {workerStats.MaxQueueWaitMs,6:0.0} / {workerStats.MaxWorkMs,6:0.0} / {workerStats.MaxLatencyMs,6:0.0}";

            ScreenshotQueueStats screenshotStats = SaveManager.ScreenshotQueueStats;
            screenshotText.Value =
                "Screenshot Queue\n" +
                $"  Queue depth (pending/peak): {screenshotStats.Pending,4}/{screenshotStats.Peak,4}\n" +
                $"  Last screenshot save time: {screenshotStats.LastScreenshotMs,6:0.0} ms";

            SimThreadStats simThreadStats = SimThread.Stats;
            simThreadText.Value =
                "Simulation Thread\n" +
                $"  Frame ms last/avg/max: {simThreadStats.LastFrameMs,6:0.0} / {simThreadStats.AvgFrameMs,6:0.0} / {simThreadStats.MaxFrameMs,6:0.0}\n" +
                $"  Drift ms last/avg/max+/min-: {simThreadStats.LastDriftMs,6:0.0} / {simThreadStats.AvgDriftMs,6:0.0} / {simThreadStats.MaxPositiveDriftMs,6:0.0} / {simThreadStats.MinNegativeDriftMs,6:0.0}\n" +
                $"  Catch-up steps (last/max): {simThreadStats.LastCatchUpSteps,4}/{simThreadStats.MaxCatchUpSteps,4}  |  Overruns: {simThreadStats.OverrunCount,6}";

            UiThreadStats uiThreadStats = UiThread.Stats;
            uiThreadText.Value =
                "UI Thread\n" +
                $"  Frame ms last/avg/max: {uiThreadStats.LastFrameMs,6:0.0} / {uiThreadStats.AvgFrameMs,6:0.0} / {uiThreadStats.MaxFrameMs,6:0.0}\n" +
                $"  Phase ms last dispatch/update/build: {uiThreadStats.LastDispatchMs,6:0.0} / {uiThreadStats.LastUiUpdateMs,6:0.0} / {uiThreadStats.LastBuildMs,6:0.0}\n" +
                $"  Phase ms avg  dispatch/update/build: {uiThreadStats.AvgDispatchMs,6:0.0} / {uiThreadStats.AvgUiUpdateMs,6:0.0} / {uiThreadStats.AvgBuildMs,6:0.0}\n" +
                $"  Overruns: {uiThreadStats.OverrunCount,6}  |  Wait timeouts: {uiThreadStats.WaitTimeouts,6}";

            RenderSnapshotSyncStats renderSyncStats = RenderSnapshotManager.SyncStats;
            MainRenderStats renderStats = MainRenderTelemetry.Stats;
            renderSyncText.Value =
                "Render Sync\n" +
                $"  Snapshot age ms last/avg/max: {renderSyncStats.LastSnapshotAgeMs,6:0.0} / {renderSyncStats.AvgSnapshotAgeMs,6:0.0} / {renderSyncStats.MaxSnapshotAgeMs,6:0.0}\n" +
                $"  Stale draw streak current/max/total: {renderSyncStats.StaleDrawStreak,6} / {renderSyncStats.MaxStaleDrawStreak,6} / {renderSyncStats.TotalStaleDraws,8}\n" +
                $"  Snapshot build/draw count: {renderSyncStats.TotalBuilds,8} / {renderSyncStats.TotalDraws,8}\n" +
                "Main Render\n" +
                $"  Frame ms last/avg/max: {renderStats.LastFrameMs,6:0.0} / {renderStats.AvgFrameMs,6:0.0} / {renderStats.MaxFrameMs,6:0.0}\n" +
                $"  Phase ms last ui/world/composite: {renderStats.LastUiBuildMs,6:0.0} / {renderStats.LastWorldDrawMs,6:0.0} / {renderStats.LastUiCompositeMs,6:0.0}";

            if (Mode(DebugMode.ModelPreview))
            {
                renderSyncText.Value += BuildModelPreviewOverlayText();
            }

            EmitDiagnosticsWarnings(mainStats, uiStats, simStats, workerStats, simThreadStats, uiThreadStats, renderSyncStats, renderStats);
        }

        static string BuildModelPreviewOverlayText()
        {
            var player = ObjectManager.Player;
            if (player == null) return "\nModel Preview\n  Player: <none>";

            WorldSpace3D playerWorldPosition = WorldBlockRenderer.ResolvePreviewWorldPosition(player.FeetPosition);
            string topDirection = WorldBlockRenderer.ResolveCompassName(WorldBlockRenderer.CameraGroundForward);
            string rightDirection = WorldBlockRenderer.ResolveCompassName(WorldBlockRenderer.CameraGroundRight);
            float yawDegrees = MathHelper.ToDegrees(WorldBlockRenderer.CameraYawRadians);

            return
                "\nModel Preview\n" +
                $"  Top/Right: {topDirection} / {rightDirection}\n" +
                $"  Camera yaw/zoom: {yawDegrees,6:0.0} deg / {WorldBlockRenderer.CameraZoom,4:0.00}\n" +
                $"  Player 3D XYZ: {playerWorldPosition.X,6:0.00}, {playerWorldPosition.Y,6:0.00}, {playerWorldPosition.Z,6:0.00}";
        }

        static void EmitDiagnosticsWarnings(
            in MailboxStats mainStats,
            in MailboxStats uiStats,
            in MailboxStats simStats,
            in WorkerPoolStats workerStats,
            in SimThreadStats simThreadStats,
            in UiThreadStats uiThreadStats,
            in RenderSnapshotSyncStats renderSyncStats,
            in MainRenderStats renderStats)
        {
            if (!Mode(DebugMode.Print)) return;

            double nowMs = TimeManager.InstanceTotalFrameTimeAsTimeSpan.TotalMilliseconds;

            if ((mainStats.Pending >= QueueWarningThreshold || uiStats.Pending >= QueueWarningThreshold || simStats.Pending >= QueueWarningThreshold) &&
                nowMs >= nextQueueWarningMs)
            {
                Print($"WARN mailbox backlog main:{mainStats.Pending}/{mainStats.Peak} ui:{uiStats.Pending}/{uiStats.Peak} sim:{simStats.Pending}/{simStats.Peak}");
                nextQueueWarningMs = nowMs + WarningCooldownMs;
            }

            if ((mainStats.LastOldestMessageAgeMs >= MailboxAgeWarningMs ||
                uiStats.LastOldestMessageAgeMs >= MailboxAgeWarningMs ||
                simStats.LastOldestMessageAgeMs >= MailboxAgeWarningMs) &&
                nowMs >= nextQueueAgeWarningMs)
            {
                Print($"WARN mailbox queue age main:{mainStats.LastOldestMessageAgeMs:0.0}/{mainStats.AvgMessageAgeMs:0.0}/{mainStats.MaxMessageAgeMs:0.0}ms ui:{uiStats.LastOldestMessageAgeMs:0.0}/{uiStats.AvgMessageAgeMs:0.0}/{uiStats.MaxMessageAgeMs:0.0}ms sim:{simStats.LastOldestMessageAgeMs:0.0}/{simStats.AvgMessageAgeMs:0.0}/{simStats.MaxMessageAgeMs:0.0}ms");
                nextQueueAgeWarningMs = nowMs + WarningCooldownMs;
            }

            long deltaMainFailures = mainStats.TotalHandlerFailures - lastMainFailureCount;
            long deltaUiFailures = uiStats.TotalHandlerFailures - lastUiFailureCount;
            long deltaSimFailures = simStats.TotalHandlerFailures - lastSimFailureCount;
            bool hasNewFailures = deltaMainFailures > 0 || deltaUiFailures > 0 || deltaSimFailures > 0;
            if (hasNewFailures && nowMs >= nextFailureWarningMs)
            {
                Print($"WARN mailbox handler failures +main:{Math.Max(0, deltaMainFailures)} +ui:{Math.Max(0, deltaUiFailures)} +sim:{Math.Max(0, deltaSimFailures)} totals main:{mainStats.TotalHandlerFailures} ui:{uiStats.TotalHandlerFailures} sim:{simStats.TotalHandlerFailures}");
                nextFailureWarningMs = nowMs + WarningCooldownMs;
            }

            lastMainFailureCount = mainStats.TotalHandlerFailures;
            lastUiFailureCount = uiStats.TotalHandlerFailures;
            lastSimFailureCount = simStats.TotalHandlerFailures;

            long deltaMainCoalesced = mainStats.TotalCoalesced - lastMainCoalescedCount;
            long deltaUiCoalesced = uiStats.TotalCoalesced - lastUiCoalescedCount;
            long deltaSimCoalesced = simStats.TotalCoalesced - lastSimCoalescedCount;
            long deltaMainDropped = mainStats.TotalDropped - lastMainDroppedCount;
            long deltaUiDropped = uiStats.TotalDropped - lastUiDroppedCount;
            long deltaSimDropped = simStats.TotalDropped - lastSimDroppedCount;
            bool coalescedSpike = deltaMainCoalesced >= CoalescedWarningDeltaThreshold ||
                                  deltaUiCoalesced >= CoalescedWarningDeltaThreshold ||
                                  deltaSimCoalesced >= CoalescedWarningDeltaThreshold ||
                                  deltaMainDropped >= CoalescedWarningDeltaThreshold ||
                                  deltaUiDropped >= CoalescedWarningDeltaThreshold ||
                                  deltaSimDropped >= CoalescedWarningDeltaThreshold;
            if (coalescedSpike && nowMs >= nextCoalesceWarningMs)
            {
                Print($"WARN mailbox coalesced/replaced +main:{Math.Max(0, deltaMainCoalesced)}/{Math.Max(0, deltaMainDropped)} +ui:{Math.Max(0, deltaUiCoalesced)}/{Math.Max(0, deltaUiDropped)} +sim:{Math.Max(0, deltaSimCoalesced)}/{Math.Max(0, deltaSimDropped)} totals main:{mainStats.TotalCoalesced}/{mainStats.TotalDropped} ui:{uiStats.TotalCoalesced}/{uiStats.TotalDropped} sim:{simStats.TotalCoalesced}/{simStats.TotalDropped}");
                nextCoalesceWarningMs = nowMs + WarningCooldownMs;
            }

            lastMainCoalescedCount = mainStats.TotalCoalesced;
            lastUiCoalescedCount = uiStats.TotalCoalesced;
            lastSimCoalescedCount = simStats.TotalCoalesced;
            lastMainDroppedCount = mainStats.TotalDropped;
            lastUiDroppedCount = uiStats.TotalDropped;
            lastSimDroppedCount = simStats.TotalDropped;

            long deltaSimOverruns = simThreadStats.OverrunCount - lastSimOverrunCount;
            long deltaUiOverruns = uiThreadStats.OverrunCount - lastUiOverrunCount;
            long deltaUiTimeouts = uiThreadStats.WaitTimeouts - lastUiTimeoutCount;
            bool hasThreadIssues = deltaSimOverruns > 0 || deltaUiOverruns > 0 || deltaUiTimeouts > 0;
            bool slowFrame = simThreadStats.LastFrameMs >= ThreadFrameWarningMs || uiThreadStats.LastFrameMs >= ThreadFrameWarningMs || simThreadStats.LastDriftMs >= SimDriftWarningMs || simThreadStats.LastCatchUpSteps > 0;
            if ((hasThreadIssues || slowFrame) && nowMs >= nextThreadWarningMs)
            {
                Print($"WARN thread timing sim frame:{simThreadStats.LastFrameMs:0.0}/{simThreadStats.AvgFrameMs:0.0}/{simThreadStats.MaxFrameMs:0.0}ms drift:{simThreadStats.LastDriftMs:0.0}/{simThreadStats.AvgDriftMs:0.0}ms catchup:{simThreadStats.LastCatchUpSteps} (+overruns:{Math.Max(0, deltaSimOverruns)}) ui frame:{uiThreadStats.LastFrameMs:0.0}/{uiThreadStats.AvgFrameMs:0.0}/{uiThreadStats.MaxFrameMs:0.0}ms dispatch/update/build:{uiThreadStats.LastDispatchMs:0.0}/{uiThreadStats.LastUiUpdateMs:0.0}/{uiThreadStats.LastBuildMs:0.0}ms (+overruns:{Math.Max(0, deltaUiOverruns)} +timeouts:{Math.Max(0, deltaUiTimeouts)})");
                nextThreadWarningMs = nowMs + WarningCooldownMs;
            }

            bool staleSnapshots = renderSyncStats.LastSnapshotAgeMs >= SnapshotAgeWarningMs ||
                                  renderSyncStats.StaleDrawStreak >= SnapshotStaleDrawWarningThreshold;
            if (staleSnapshots && nowMs >= nextSnapshotWarningMs)
            {
                Print($"WARN snapshot staleness age:{renderSyncStats.LastSnapshotAgeMs:0.0}/{renderSyncStats.AvgSnapshotAgeMs:0.0}/{renderSyncStats.MaxSnapshotAgeMs:0.0}ms staleDrawStreak:{renderSyncStats.StaleDrawStreak}/{renderSyncStats.MaxStaleDrawStreak} builds/draws:{renderSyncStats.TotalBuilds}/{renderSyncStats.TotalDraws}");
                nextSnapshotWarningMs = nowMs + WarningCooldownMs;
            }

            if (workerStats.LastLatencyMs >= WorkerLatencyWarningMs && nowMs >= nextWorkerWarningMs)
            {
                Print($"WARN worker latency queue/work/latency ms last:{workerStats.LastQueueWaitMs:0.0}/{workerStats.LastWorkMs:0.0}/{workerStats.LastLatencyMs:0.0} avg:{workerStats.AvgQueueWaitMs:0.0}/{workerStats.AvgWorkMs:0.0}/{workerStats.AvgLatencyMs:0.0} max:{workerStats.MaxQueueWaitMs:0.0}/{workerStats.MaxWorkMs:0.0}/{workerStats.MaxLatencyMs:0.0}");
                nextWorkerWarningMs = nowMs + WarningCooldownMs;
            }

            if (renderStats.LastFrameMs >= RenderFrameWarningMs && nowMs >= nextRenderWarningMs)
            {
                Print($"WARN render frame total/ui/world/composite ms last:{renderStats.LastFrameMs:0.0}/{renderStats.LastUiBuildMs:0.0}/{renderStats.LastWorldDrawMs:0.0}/{renderStats.LastUiCompositeMs:0.0} avg:{renderStats.AvgFrameMs:0.0}/{renderStats.AvgUiBuildMs:0.0}/{renderStats.AvgWorldDrawMs:0.0}/{renderStats.AvgUiCompositeMs:0.0} max:{renderStats.MaxFrameMs:0.0}/{renderStats.MaxUiBuildMs:0.0}/{renderStats.MaxWorldDrawMs:0.0}/{renderStats.MaxUiCompositeMs:0.0}");
                nextRenderWarningMs = nowMs + WarningCooldownMs;
            }

            lastSimOverrunCount = simThreadStats.OverrunCount;
            lastUiOverrunCount = uiThreadStats.OverrunCount;
            lastUiTimeoutCount = uiThreadStats.WaitTimeouts;
        }

        static void DrawOverlayInfo(SpriteBatch aBatch, Text aText, DebugOverlayInfo aInfo, ref AbsoluteScreenPosition aCursor)
        {
            if (!OverlayInfoEnabled(aInfo)) return;
            aText.TopLeftDraw(aBatch, aCursor);
            aCursor += new AbsoluteScreenPosition(0, (int)Math.Ceiling(aText.Offset.Y) + OverlaySectionSpacingPx);
        }

        static string BuildMailboxDispatchText(string aName, in MailboxStats aStats)
        {
            return
                $"  [{aName}] [lastDispatchMsgs: {aStats.LastDispatchCount}] [lastDispatchMs: {aStats.LastDispatchMs:0.0}] [oldestAgeMs: {aStats.LastOldestMessageAgeMs:0.0}]\n" +
                $"       [published: {aStats.TotalPublished}] [handled: {aStats.TotalDispatched}]\n" +
                $"       [exceptions raised: {aStats.TotalHandlerFailures}] [noSub: {aStats.TotalWithoutSubscribers}] [misses: {aStats.TotalDispatchMisses}]\n" +
                $"       [coalesced: {aStats.TotalCoalesced}] [replaced: {aStats.TotalDropped}]\n" +
                $"       [topFail: {aStats.TopHandlerFailureType}:{aStats.TopHandlerFailureCount}] [topCoalesced: {aStats.TopCoalescedType}:{aStats.TopCoalescedCount}]";
        }
    }
}
