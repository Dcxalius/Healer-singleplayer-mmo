using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Project_1.Managers.Saves;
using Project_1.Messaging;
using Project_1.UI;

namespace Project_1.Managers
{
    internal static partial class DebugManager
    {
        static readonly object diagnosticsFileLock = new object();
        static StreamWriter diagnosticsWriter;
        static string diagnosticsCurrentPath;
        static string diagnosticsLastPath;
        static bool diagnosticsSessionInitialized;
        const int StartupDiagnosticsTailLines = 20;
        const string DiagnosticsCurrentFileName = "Diagnostics.current.log";
        const string DiagnosticsLastFileName = "Diagnostics.last.log";
        const string DiagnosticsCrashFileName = "Diagnostics.crash.log";

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
                MailboxStats mainStats = MailboxManager.MainStats;
                MailboxStats uiStats = MailboxManager.UiStats;
                MailboxStats simStats = MailboxManager.SimStats;
                WorkerPoolStats workerStats = WorkerPool.Stats;
                SimThreadStats simThreadStats = SimThread.Stats;
                UiThreadStats uiThreadStats = UiThread.Stats;
                RenderSnapshotSyncStats renderSyncStats = RenderSnapshotManager.SyncStats;
                MainRenderStats renderStats = MainRenderTelemetry.Stats;
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
            }
            catch (Exception ex)
            {
                WriteDiagnosticsLine($"[LIFECYCLE] [{phase}] snapshot collection failed: {ex.Message}", Mode(DebugMode.Print));
            }
        }
    }
}
