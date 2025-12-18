using System;

namespace Project_1.Managers
{
    /// <summary>
    /// Centralized thread identity/affinity checks to keep GPU and UI work on their intended threads.
    /// </summary>
    internal static class ThreadAffinity
    {
        static int? mainThreadId;
        static int? simThreadId;
        static int? uiThreadId;

        public static void InitMainThread()
        {
            if (mainThreadId.HasValue) return;
            mainThreadId = Environment.CurrentManagedThreadId;
        }

        public static void RegisterSimThread()
        {
            simThreadId = Environment.CurrentManagedThreadId;
        }

        public static void RegisterUiThread()
        {
            uiThreadId = Environment.CurrentManagedThreadId;
        }

        public static void AssertMainThread()
        {
            if (mainThreadId.HasValue && mainThreadId != Environment.CurrentManagedThreadId)
            {
                throw new InvalidOperationException("This operation must run on the main thread.");
            }
        }

        public static void AssertSimThread()
        {
            if (simThreadId.HasValue && simThreadId != Environment.CurrentManagedThreadId)
            {
                throw new InvalidOperationException("This operation must run on the simulation thread.");
            }
        }

        public static void AssertUiThread()
        {
            if (uiThreadId.HasValue && uiThreadId != Environment.CurrentManagedThreadId)
            {
                throw new InvalidOperationException("This operation must run on the UI thread.");
            }
        }
    }
}
