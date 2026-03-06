using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.DebugTools;
using Project_1.GameObjects;
using Project_1.Input;
using Project_1.Items;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI;
using Project_1.UI.HUD.Managers;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Project_1.GameObjects.Entities.Friendlies.Players;

namespace Project_1.Managers
{
    enum DebugMode
    {
        DebugShapes,
        DebugOverlay,
        Print,
        FalseRandom,
        TileCoords,
        InvCheats,
        Teleport,
        InstantlyContinue,
        Console,
        TeleportStuckThings,
        LearnKill,
        ChatCheats,
        Count
    }

    enum DebugOverlayInfo
    {
        Fps,
        FrameTime,
        TotalTime,
        MailboxQueue,
        MailboxDispatch,
        Worker,
        ScreenshotQueue,
        SimThread,
        UiThread,
        RenderSync,
        Count
    }

    internal static class DebugManager
    {
        sealed class DebugSettingsData
        {
            public int ModeMask { get; set; }
            public int OverlayMask { get; set; }
        }

        static readonly List<DebugShape> debugShapes = new List<DebugShape>();
        static readonly ConcurrentQueue<DebugShape> pendingDebugShapes = new ConcurrentQueue<DebugShape>();
        static volatile bool clearDebugShapesRequested;
        static bool initialized;
        static bool statValidationExecuted;
        static int modeMask;
        static int overlayMask;

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
        const double ShadowPassWarningMs = 8d;
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
        static double nextShadowWarningMs;
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
        static readonly object diagnosticsFileLock = new object();
        static StreamWriter diagnosticsWriter;
        static string diagnosticsCurrentPath;
        static string diagnosticsLastPath;
        static bool diagnosticsSessionInitialized;
        const int StartupDiagnosticsTailLines = 20;
        const string DiagnosticsCurrentFileName = "Diagnostics.current.log";
        const string DiagnosticsLastFileName = "Diagnostics.last.log";
        const string DiagnosticsCrashFileName = "Diagnostics.crash.log";


        static int AllModeMask => (1 << (int)DebugMode.Count) - 1;
        static int AllOverlayMask => (1 << (int)DebugOverlayInfo.Count) - 1;

        public static bool Mode(DebugMode aMode)
        {
            int mask = 1 << (int)aMode;
            return (Volatile.Read(ref modeMask) & mask) != 0;
        }

        public static bool OverlayInfoEnabled(DebugOverlayInfo aInfo)
        {
            int mask = 1 << (int)aInfo;
            return (Volatile.Read(ref overlayMask) & mask) != 0;
        }

        public static void SetMode(DebugMode aMode, bool aEnabled)
        {
            int bit = 1 << (int)aMode;
            while (true)
            {
                int current = Volatile.Read(ref modeMask);
                int updated = aEnabled ? current | bit : current & ~bit;
                if (current == updated) return;
                if (Interlocked.CompareExchange(ref modeMask, updated, current) == current) return;
            }
        }

        public static void SetOverlayInfo(DebugOverlayInfo aInfo, bool aEnabled)
        {
            int bit = 1 << (int)aInfo;
            while (true)
            {
                int current = Volatile.Read(ref overlayMask);
                int updated = aEnabled ? current | bit : current & ~bit;
                if (current == updated) return;
                if (Interlocked.CompareExchange(ref overlayMask, updated, current) == current) return;
            }
        }


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;

            modeMask = 0;
            overlayMask = AllOverlayMask;
            SetMode(DebugMode.DebugShapes, true);
            SetMode(DebugMode.DebugOverlay, true);
            SetMode(DebugMode.FalseRandom, false);
            SetMode(DebugMode.Print, true);
            SetMode(DebugMode.TileCoords, false);
            SetMode(DebugMode.InvCheats, true);
            SetMode(DebugMode.Teleport, true);
            SetMode(DebugMode.InstantlyContinue, true);
            SetMode(DebugMode.Console, true);
            SetMode(DebugMode.TeleportStuckThings, true);
            SetMode(DebugMode.LearnKill, true);
            SetMode(DebugMode.ChatCheats, false);

            ImportSettings();

            if (Mode(DebugMode.Console))
            {
                AllocConsole();
            }

            InitializeDiagnosticsSession();
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            EmitLifecycleSnapshot("STARTUP");
        }

        public static void Shutdown()
        {
            lock (diagnosticsFileLock)
            {
                if (diagnosticsWriter == null) return;
                try
                {
                    EmitLifecycleSnapshot("SHUTDOWN");
                    diagnosticsWriter.WriteLine($"{DateTime.UtcNow:O} [SESSION] End");
                    diagnosticsWriter.Flush();
                    diagnosticsWriter.Dispose();
                }
                catch (Exception)
                {
                    // Best effort shutdown; never fail app exit on diagnostics persistence.
                }
                finally
                {
                    diagnosticsWriter = null;
                }
            }
        }

        public static void ReportFatalException(Exception ex, string source = "Unhandled")
        {
            if (ex == null) return;

            try
            {
                WriteDiagnosticsLine($"[FATAL] [{source}] {ex}", true);
            }
            catch (Exception)
            {
                // ignore
            }

            try
            {
                string crashPath = BuildDiagnosticsPath(DiagnosticsCrashFileName);
                if (string.IsNullOrWhiteSpace(crashPath)) return;
                string crashDir = Path.GetDirectoryName(crashPath);
                if (!string.IsNullOrWhiteSpace(crashDir))
                {
                    Directory.CreateDirectory(crashDir);
                }
                File.AppendAllText(crashPath, $"{DateTime.UtcNow:O} [{source}] {ex}{Environment.NewLine}{Environment.NewLine}");
            }
            catch (Exception)
            {
                // ignore
            }
        }

        static void ImportSettings()
        {
            ThreadAffinity.AssertMainThread();
            if (TryImportSettings(SaveManager.DebugSettings)) return;
            TryImportSettings(SaveManager.DefaultDebugSettings);
        }

        static bool TryImportSettings(string aPath)
        {
            try
            {
                if (!File.Exists(aPath)) return false;
                string json = File.ReadAllText(aPath);
                DebugSettingsData imported = SaveManager.ImportData<DebugSettingsData>(json);
                if (imported == null) return false;

                int importedModeMask = imported.ModeMask & AllModeMask;
                int importedOverlayMask = imported.OverlayMask & AllOverlayMask;
                if (importedModeMask == 0) importedModeMask = Volatile.Read(ref modeMask);
                if (importedOverlayMask == 0) importedOverlayMask = AllOverlayMask;
                Volatile.Write(ref modeMask, importedModeMask);
                Volatile.Write(ref overlayMask, importedOverlayMask);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void ExportSettings()
        {
            ThreadAffinity.AssertUiThread();
            DebugSettingsData saveData = new DebugSettingsData()
            {
                ModeMask = Volatile.Read(ref modeMask) & AllModeMask,
                OverlayMask = Volatile.Read(ref overlayMask) & AllOverlayMask
            };
            SaveManager.ExportData(SaveManager.DebugSettings, saveData);
        }

        public static void LoadContent()
        {
            ThreadAffinity.AssertMainThread();
            fpsText = new Text("Gloryse", Color.Chartreuse);
            frameTimeText = new Text("Gloryse", Color.Chartreuse);
            totalTimeText = new Text("Gloryse", Color.Chartreuse);
            mailboxText = new Text("Gloryse", Color.Chartreuse);
            dispatchText = new Text("Gloryse", Color.Chartreuse);
            workerText = new Text("Gloryse", Color.Chartreuse);
            screenshotText = new Text("Gloryse", Color.Chartreuse);
            simThreadText = new Text("Gloryse", Color.Chartreuse);
            uiThreadText = new Text("Gloryse", Color.Chartreuse);
            renderSyncText = new Text("Gloryse", Color.Chartreuse);
            debugTextOrigin = new AbsoluteScreenPosition(12, 12);
        }

        public static void Update()
        {
            ThreadAffinity.AssertSimThread();
            RunStatValidationOnce();
            InventoryCheats();
            TeleportPlayer();

            ClearDebugShapes();

            UpdateOverlayText();
        }

        static void RunStatValidationOnce()
        {
            if (statValidationExecuted)
            {
                return;
            }

            if (!StatValidation.TryRun(out string validationResult))
            {
                return;
            }

            statValidationExecuted = true;
            if (!string.IsNullOrWhiteSpace(validationResult))
            {
                Print(validationResult);
            }
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

            var mainStats = Mailboxes.MainStats;
            var uiStats = Mailboxes.UiStats;
            var simStats = Mailboxes.SimStats;
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
            var workerStats = WorkerPool.Stats;
            workerText.Value =
                "Worker Pool\n" +
                $"  Queue depth (pending/peak): {workerStats.Pending,4}/{workerStats.Peak,4}\n" +
                $"  Last ms queue/work/latency: {workerStats.LastQueueWaitMs,6:0.0} / {workerStats.LastWorkMs,6:0.0} / {workerStats.LastLatencyMs,6:0.0}\n" +
                $"  Avg  ms queue/work/latency: {workerStats.AvgQueueWaitMs,6:0.0} / {workerStats.AvgWorkMs,6:0.0} / {workerStats.AvgLatencyMs,6:0.0}\n" +
                $"  Max  ms queue/work/latency: {workerStats.MaxQueueWaitMs,6:0.0} / {workerStats.MaxWorkMs,6:0.0} / {workerStats.MaxLatencyMs,6:0.0}";
            var screenshotStats = SaveManager.ScreenshotQueueStats;
            screenshotText.Value =
                "Screenshot Queue\n" +
                $"  Queue depth (pending/peak): {screenshotStats.Pending,4}/{screenshotStats.Peak,4}\n" +
                $"  Last screenshot save time: {screenshotStats.LastScreenshotMs,6:0.0} ms";
            var simThreadStats = SimThread.Stats;
            simThreadText.Value =
                "Simulation Thread\n" +
                $"  Frame ms last/avg/max: {simThreadStats.LastFrameMs,6:0.0} / {simThreadStats.AvgFrameMs,6:0.0} / {simThreadStats.MaxFrameMs,6:0.0}\n" +
                $"  Drift ms last/avg/max+/min-: {simThreadStats.LastDriftMs,6:0.0} / {simThreadStats.AvgDriftMs,6:0.0} / {simThreadStats.MaxPositiveDriftMs,6:0.0} / {simThreadStats.MinNegativeDriftMs,6:0.0}\n" +
                $"  Catch-up steps (last/max): {simThreadStats.LastCatchUpSteps,4}/{simThreadStats.MaxCatchUpSteps,4}  |  Overruns: {simThreadStats.OverrunCount,6}";
            var uiThreadStats = UiThread.Stats;
            uiThreadText.Value =
                "UI Thread\n" +
                $"  Frame ms last/avg/max: {uiThreadStats.LastFrameMs,6:0.0} / {uiThreadStats.AvgFrameMs,6:0.0} / {uiThreadStats.MaxFrameMs,6:0.0}\n" +
                $"  Phase ms last dispatch/update/build: {uiThreadStats.LastDispatchMs,6:0.0} / {uiThreadStats.LastUiUpdateMs,6:0.0} / {uiThreadStats.LastBuildMs,6:0.0}\n" +
                $"  Phase ms avg  dispatch/update/build: {uiThreadStats.AvgDispatchMs,6:0.0} / {uiThreadStats.AvgUiUpdateMs,6:0.0} / {uiThreadStats.AvgBuildMs,6:0.0}\n" +
                $"  Overruns: {uiThreadStats.OverrunCount,6}  |  Wait timeouts: {uiThreadStats.WaitTimeouts,6}";
            var renderSyncStats = RenderSnapshotManager.SyncStats;
            var renderStats = MainRenderTelemetry.Stats;
            var shadowStats = ShadowRenderTelemetry.Stats;
            renderSyncText.Value =
                "Render Sync\n" +
                $"  Snapshot age ms last/avg/max: {renderSyncStats.LastSnapshotAgeMs,6:0.0} / {renderSyncStats.AvgSnapshotAgeMs,6:0.0} / {renderSyncStats.MaxSnapshotAgeMs,6:0.0}\n" +
                $"  Stale draw streak current/max/total: {renderSyncStats.StaleDrawStreak,6} / {renderSyncStats.MaxStaleDrawStreak,6} / {renderSyncStats.TotalStaleDraws,8}\n" +
                $"  Snapshot build/draw count: {renderSyncStats.TotalBuilds,8} / {renderSyncStats.TotalDraws,8}\n" +
                "Main Render\n" +
                $"  Frame ms last/avg/max: {renderStats.LastFrameMs,6:0.0} / {renderStats.AvgFrameMs,6:0.0} / {renderStats.MaxFrameMs,6:0.0}\n" +
                $"  Phase ms last ui/world/composite: {renderStats.LastUiBuildMs,6:0.0} / {renderStats.LastWorldDrawMs,6:0.0} / {renderStats.LastUiCompositeMs,6:0.0}\n" +
                "Shadows\n" +
                $"  Lights candidate/active/dropped: {shadowStats.CandidateLights,3} / {shadowStats.ActiveLights,3} / {shadowStats.DroppedLights,3}\n" +
                $"  Segments total/avg/max (capped): {shadowStats.TotalSegments,5} / {shadowStats.AvgSegmentsPerLight,5:0.0} / {shadowStats.MaxSegmentsPerLight,4} ({shadowStats.CappedLights,2})\n" +
                $"  Shadow pass ms last/avg/max: {shadowStats.LastShadowPassMs,6:0.0} / {shadowStats.AvgShadowPassMs,6:0.0} / {shadowStats.MaxShadowPassMs,6:0.0}\n" +
                $"  Shadow draw calls last/avg/max: {shadowStats.LastDrawCalls,4} / {shadowStats.AvgDrawCalls,4:0.0} / {shadowStats.MaxDrawCalls,4}";

            EmitDiagnosticsWarnings(mainStats, uiStats, simStats, workerStats, simThreadStats, uiThreadStats, renderSyncStats, renderStats, shadowStats);
        }

        static void EmitDiagnosticsWarnings(
            in MailboxStats mainStats,
            in MailboxStats uiStats,
            in MailboxStats simStats,
            in WorkerPoolStats workerStats,
            in SimThreadStats simThreadStats,
            in UiThreadStats uiThreadStats,
            in RenderSnapshotSyncStats renderSyncStats,
            in MainRenderStats renderStats,
            in ShadowRenderStats shadowStats)
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
            if ((shadowStats.LastShadowPassMs >= ShadowPassWarningMs || shadowStats.DroppedLights > 0) && nowMs >= nextShadowWarningMs)
            {
                Print($"WARN shadow pass ms last/avg/max:{shadowStats.LastShadowPassMs:0.0}/{shadowStats.AvgShadowPassMs:0.0}/{shadowStats.MaxShadowPassMs:0.0} lights candidate/active/dropped:{shadowStats.CandidateLights}/{shadowStats.ActiveLights}/{shadowStats.DroppedLights} segments total/avg/max:{shadowStats.TotalSegments}/{shadowStats.AvgSegmentsPerLight:0.0}/{shadowStats.MaxSegmentsPerLight} capped:{shadowStats.CappedLights} drawCalls last/avg/max:{shadowStats.LastDrawCalls}/{shadowStats.AvgDrawCalls:0.0}/{shadowStats.MaxDrawCalls}");
                nextShadowWarningMs = nowMs + WarningCooldownMs;
            }
            lastSimOverrunCount = simThreadStats.OverrunCount;
            lastUiOverrunCount = uiThreadStats.OverrunCount;
            lastUiTimeoutCount = uiThreadStats.WaitTimeouts;
        }

        public static void AddDebugShape(DebugShape aShape)
        {
            if (!Mode(DebugMode.DebugShapes)) return;
            if (aShape == null) return;
            pendingDebugShapes.Enqueue(aShape);
        }


        public static void Print(object aObject,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
        {
            Print(aObject?.ToString() ?? "null", filePath, lineNumber, memberName);
        }

        public static void Print(string aMsg,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
        {
            if (!Mode(DebugMode.Print)) return;

            string fileName = string.IsNullOrWhiteSpace(filePath) ? "unknown" : Path.GetFileName(filePath);
            WriteDiagnosticsLine($"{fileName}:{lineNumber} {memberName}: {aMsg}", true);
        }

        static void InitializeDiagnosticsSession()
        {
            if (diagnosticsSessionInitialized) return;
            diagnosticsSessionInitialized = true;

            try
            {
                diagnosticsCurrentPath = BuildDiagnosticsPath(DiagnosticsCurrentFileName);
                diagnosticsLastPath = BuildDiagnosticsPath(DiagnosticsLastFileName);
                if (string.IsNullOrWhiteSpace(diagnosticsCurrentPath)) return;

                string diagnosticsDir = Path.GetDirectoryName(diagnosticsCurrentPath);
                if (!string.IsNullOrWhiteSpace(diagnosticsDir))
                {
                    Directory.CreateDirectory(diagnosticsDir);
                }

                if (!string.IsNullOrWhiteSpace(diagnosticsLastPath) && File.Exists(diagnosticsCurrentPath))
                {
                    if (File.Exists(diagnosticsLastPath))
                    {
                        File.Delete(diagnosticsLastPath);
                    }
                    File.Move(diagnosticsCurrentPath, diagnosticsLastPath);
                }

                lock (diagnosticsFileLock)
                {
                    diagnosticsWriter = new StreamWriter(
                        new FileStream(diagnosticsCurrentPath, FileMode.Create, FileAccess.Write, FileShare.Read),
                        new UTF8Encoding(false))
                    {
                        AutoFlush = true
                    };
                    diagnosticsWriter.WriteLine($"{DateTime.UtcNow:O} [SESSION] Start");
                }

                EmitPreviousRunTail();
            }
            catch (Exception ex)
            {
                // Do not let diagnostics setup break startup.
                try
                {
                    Console.WriteLine($"DebugManager diagnostics init failed: {ex.Message}");
                }
                catch (Exception)
                {
                    // ignore
                }
            }
        }

        static void EmitPreviousRunTail()
        {
            if (string.IsNullOrWhiteSpace(diagnosticsLastPath)) return;
            if (!File.Exists(diagnosticsLastPath)) return;

            try
            {
                string[] lines = File.ReadAllLines(diagnosticsLastPath);
                int take = Math.Min(StartupDiagnosticsTailLines, lines.Length);
                if (take <= 0) return;

                WriteDiagnosticsLine($"[STARTUP] Last-run diagnostics tail ({take} lines):", Mode(DebugMode.Print));
                int start = lines.Length - take;
                for (int i = start; i < lines.Length; i++)
                {
                    WriteDiagnosticsLine($"[LASTRUN] {lines[i]}", Mode(DebugMode.Print));
                }
            }
            catch (Exception ex)
            {
                WriteDiagnosticsLine($"[STARTUP] Failed to read last-run diagnostics: {ex.Message}", Mode(DebugMode.Print));
            }
        }

        static string BuildDiagnosticsPath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return null;

            try
            {
                string settingsPath = SaveManager.Settings;
                if (!string.IsNullOrWhiteSpace(settingsPath))
                {
                    return Path.Combine(settingsPath, fileName);
                }
            }
            catch (Exception)
            {
                // ignore and fall back
            }

            try
            {
                return Path.Combine(AppContext.BaseDirectory, fileName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        static void WriteDiagnosticsLine(string line, bool echoConsole)
        {
            if (string.IsNullOrWhiteSpace(line)) return;

            if (echoConsole)
            {
                try
                {
                    Console.WriteLine(line);
                }
                catch (Exception)
                {
                    // ignore
                }
            }

            lock (diagnosticsFileLock)
            {
                try
                {
                    diagnosticsWriter?.WriteLine($"{DateTime.UtcNow:O} {line}");
                }
                catch (Exception)
                {
                    // ignore
                }
            }
        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            Shutdown();
        }

        static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e?.ExceptionObject is Exception ex)
            {
                ReportFatalException(ex, "AppDomain.UnhandledException");
            }
            else
            {
                ReportFatalException(new Exception("Unknown unhandled exception object."), "AppDomain.UnhandledException");
            }
            Shutdown();
        }

        static void EmitLifecycleSnapshot(string phase)
        {
            if (string.IsNullOrWhiteSpace(phase)) phase = "UNKNOWN";

            try
            {
                MailboxStats mainStats = Mailboxes.MainStats;
                MailboxStats uiStats = Mailboxes.UiStats;
                MailboxStats simStats = Mailboxes.SimStats;
                WorkerPoolStats workerStats = WorkerPool.Stats;
                SimThreadStats simThreadStats = SimThread.Stats;
                UiThreadStats uiThreadStats = UiThread.Stats;
                RenderSnapshotSyncStats renderSyncStats = RenderSnapshotManager.SyncStats;
                MainRenderStats renderStats = MainRenderTelemetry.Stats;
                ShadowRenderStats shadowStats = ShadowRenderTelemetry.Stats;
                string saveName = SaveManager.CurrentSave?.Name ?? "<none>";

                WriteDiagnosticsLine(
                    $"[LIFECYCLE] [{phase}] threading ui/sim/workers enabled={ThreadingSettings.UseUiThread}/{ThreadingSettings.UseSimThread}/{ThreadingSettings.UseWorkerThreads} running={UiThread.IsRunning}/{SimThread.IsRunning}/{WorkerPool.IsRunning} save={saveName}",
                    Mode(DebugMode.Print));
                WriteDiagnosticsLine(
                    $"[LIFECYCLE] [{phase}] mailbox pending/peak main:{mainStats.Pending}/{mainStats.Peak} ui:{uiStats.Pending}/{uiStats.Peak} sim:{simStats.Pending}/{simStats.Peak}",
                    Mode(DebugMode.Print));
                WriteDiagnosticsLine(
                    $"[LIFECYCLE] [{phase}] mailbox failures/coalesced main:{mainStats.TotalHandlerFailures}/{mainStats.TotalCoalesced}/{mainStats.TotalDropped} ui:{uiStats.TotalHandlerFailures}/{uiStats.TotalCoalesced}/{uiStats.TotalDropped} sim:{simStats.TotalHandlerFailures}/{simStats.TotalCoalesced}/{simStats.TotalDropped}",
                    Mode(DebugMode.Print));
                WriteDiagnosticsLine(
                    $"[LIFECYCLE] [{phase}] worker pending/peak:{workerStats.Pending}/{workerStats.Peak} latency last/avg/max:{workerStats.LastLatencyMs:0.0}/{workerStats.AvgLatencyMs:0.0}/{workerStats.MaxLatencyMs:0.0}ms",
                    Mode(DebugMode.Print));
                WriteDiagnosticsLine(
                    $"[LIFECYCLE] [{phase}] sim frame/drift/catchup last:{simThreadStats.LastFrameMs:0.0}ms drift:{simThreadStats.LastDriftMs:0.0}ms catchup:{simThreadStats.LastCatchUpSteps} overruns:{simThreadStats.OverrunCount}",
                    Mode(DebugMode.Print));
                WriteDiagnosticsLine(
                    $"[LIFECYCLE] [{phase}] ui frame/dispatch/update/build last:{uiThreadStats.LastFrameMs:0.0}/{uiThreadStats.LastDispatchMs:0.0}/{uiThreadStats.LastUiUpdateMs:0.0}/{uiThreadStats.LastBuildMs:0.0}ms overruns/timeouts:{uiThreadStats.OverrunCount}/{uiThreadStats.WaitTimeouts}",
                    Mode(DebugMode.Print));
                WriteDiagnosticsLine(
                    $"[LIFECYCLE] [{phase}] render snapshot age/stale:{renderSyncStats.LastSnapshotAgeMs:0.0}ms/{renderSyncStats.StaleDrawStreak} render frame last:{renderStats.LastFrameMs:0.0}ms",
                    Mode(DebugMode.Print));
                WriteDiagnosticsLine(
                    $"[LIFECYCLE] [{phase}] shadows lights candidate/active/dropped:{shadowStats.CandidateLights}/{shadowStats.ActiveLights}/{shadowStats.DroppedLights} segments total/avg/max:{shadowStats.TotalSegments}/{shadowStats.AvgSegmentsPerLight:0.0}/{shadowStats.MaxSegmentsPerLight} pass last/avg/max:{shadowStats.LastShadowPassMs:0.0}/{shadowStats.AvgShadowPassMs:0.0}/{shadowStats.MaxShadowPassMs:0.0}ms drawCalls last/avg/max:{shadowStats.LastDrawCalls}/{shadowStats.AvgDrawCalls:0.0}/{shadowStats.MaxDrawCalls}",
                    Mode(DebugMode.Print));
            }
            catch (Exception ex)
            {
                WriteDiagnosticsLine($"[LIFECYCLE] [{phase}] snapshot collection failed: {ex.Message}", Mode(DebugMode.Print));
            }
        }


        static void ClearDebugShapes()
        {
            if (!KeyBindStateCache.GetPress(KeyBindManager.KeyListner.DebugDeleteShapes)) return;
            clearDebugShapesRequested = true;
        }

        static void InventoryCheats()
        {
            if (!Mode(DebugMode.InvCheats)) return;
            SpawnTestGear();
            SpawnHealthPotion();
            SpawnManaPotion();
        }
        static void SpawnTestGear()
        {
            if (!KeyBindStateCache.GetPress(KeyBindManager.KeyListner.DebugTestGear)) return;
            RelativeScreenPosition dialogueBoxSize = new RelativeScreenPosition(0.2f);
            Mailboxes.PublishUiEvent(new Messaging.Events.DialogueOpened(
                "Hello Cheater!\n\nxdd",
                Color.White,
                DialogueBox.LocationOfPopUp.HUDManager.ToDialoguePopupLocation(),
                DialogueBox.PausesGame.Pauses.ToDialoguePauseKind(),
                null,
                new GfxPath(GfxType.UI, "GrayBackground"),
                new RelativeScreenPosition(0.5f) - dialogueBoxSize / 2,
                dialogueBoxSize,
                "Close"));
            
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("ZweiHander"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Axe"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Dagger"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Bow"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Shield"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Helmet"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Amulet of spoons"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Big Shoulders"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Backoff"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Chesty"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Bracers"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Glovy"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Belty"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Panties"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Booti"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("FIRST"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Seocnd"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("thrd"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("FORSTA"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Andra"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("tredg"), 1));

        }

        static void SpawnHealthPotion()
        {
            if (!KeyBindStateCache.GetPress(KeyBindManager.KeyListner.DebugHealthPotion)) return;

            Item hpPot = ItemFactory.CreateItem(ItemFactory.GetItemData("Health Potion"), 1);
            ObjectManager.Player.Inventory.AddItem(hpPot);
            
        }

        static void SpawnManaPotion()
        {
            if (!KeyBindStateCache.GetPress(KeyBindManager.KeyListner.DebugManaPotion)) return;

            Item mpPot = ItemFactory.CreateItem(ItemFactory.GetItemData("Mana Potion"), 1);
            ObjectManager.Player.Inventory.AddItem(mpPot);
        }

        static void TeleportPlayer()
        {
            if (!Mode(DebugMode.Teleport)) return;
            if (!KeyBindStateCache.GetPress(KeyBindManager.KeyListner.DebugTeleport)) return;
            

            ObjectManager.Player.Teleport(WorldSpace.FromRelativeScreenSpace(MouseStateCache.Relative));
        }
        public static void Draw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            bool drawShapes = Mode(DebugMode.DebugShapes);
            bool drawOverlay = Mode(DebugMode.DebugOverlay);

            if (!drawShapes && !drawOverlay) return;

            if (drawShapes)
            {
                if (clearDebugShapesRequested)
                {
                    debugShapes.Clear();
                    clearDebugShapesRequested = false;
                    while (pendingDebugShapes.TryDequeue(out _)) { }
                }
                while (pendingDebugShapes.TryDequeue(out DebugShape pending))
                {
                    debugShapes.Add(pending);
                }
                for (int i = 0; i < debugShapes.Count; i++)
                {
                    debugShapes[i].Draw(aBatch);
                }
            }

            if (drawOverlay)
            {
                AbsoluteScreenPosition cursor = debugTextOrigin;
                DrawOverlayInfo(aBatch, fpsText, DebugOverlayInfo.Fps, ref cursor);
                DrawOverlayInfo(aBatch, frameTimeText, DebugOverlayInfo.FrameTime, ref cursor);
                DrawOverlayInfo(aBatch, totalTimeText, DebugOverlayInfo.TotalTime, ref cursor);
                DrawOverlayInfo(aBatch, mailboxText, DebugOverlayInfo.MailboxQueue, ref cursor);
                DrawOverlayInfo(aBatch, dispatchText, DebugOverlayInfo.MailboxDispatch, ref cursor);
                DrawOverlayInfo(aBatch, workerText, DebugOverlayInfo.Worker, ref cursor);
                DrawOverlayInfo(aBatch, screenshotText, DebugOverlayInfo.ScreenshotQueue, ref cursor);
                DrawOverlayInfo(aBatch, simThreadText, DebugOverlayInfo.SimThread, ref cursor);
                DrawOverlayInfo(aBatch, uiThreadText, DebugOverlayInfo.UiThread, ref cursor);
                DrawOverlayInfo(aBatch, renderSyncText, DebugOverlayInfo.RenderSync, ref cursor);
            }
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
