using System;

namespace Project_1.Managers
{
    /// <summary>
    /// Central toggles for thread usage with optional environment overrides.
    /// </summary>
    internal static class ThreadingSettings
    {
        public static bool UseUiThread { get; private set; } = true;
        public static bool UseSimThread { get; private set; } = true;
        public static bool UseWorkerThreads { get; private set; } = true;

        public static void Init()
        {
            bool singleThread = IsEnvTrue("P1_SINGLE_THREAD");
            if (singleThread)
            {
                UseUiThread = false;
                UseSimThread = false;
                UseWorkerThreads = false;
                return;
            }

            if (IsEnvTrue("P1_DISABLE_UI_THREAD")) UseUiThread = false;
            if (IsEnvTrue("P1_DISABLE_SIM_THREAD")) UseSimThread = false;
            if (IsEnvTrue("P1_DISABLE_WORKER_THREADS")) UseWorkerThreads = false;
        }

        static bool IsEnvTrue(string name)
        {
            string value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(value)) return false;
            return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
    }
}
