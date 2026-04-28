using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Project_1.Managers.Saves;

namespace Project_1.Managers
{
    enum DebugMode
    {
        DebugShapes,
        DebugOverlay,
        ModelPreview,
        Print,
        ShadowMaskPreview,
        ShadowMagentaClear,
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

    internal static partial class DebugManager
    {
        sealed class DebugSettingsData
        {
            public int ModeMask { get; set; }
            public int OverlayMask { get; set; }
        }

        static bool initialized;
        static bool statValidationExecuted;
        static int modeMask;
        static int overlayMask;

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
            ImportSettings();

#if DEBUG
            SetMode(DebugMode.Print, true);
            SetMode(DebugMode.Teleport, true);
            SetMode(DebugMode.InstantlyContinue, true);
            SetMode(DebugMode.Console, true);
            SetMode(DebugMode.LearnKill, true);
            SetMode(DebugMode.ChatCheats, true);
            SetMode(DebugMode.ModelPreview, true);
#endif

            if (Mode(DebugMode.Console))
            {
                AllocConsole();
            }

            InitializeDiagnosticsSession();
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            EmitLifecycleSnapshot("STARTUP");
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
    }
}
