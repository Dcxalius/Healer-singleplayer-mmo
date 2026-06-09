using System;
using System.Diagnostics;

namespace Project_1.Managers
{
    /// <summary>
    /// Centralized thread identity/affinity checks to keep GPU and UI work on their intended threads.
    /// </summary>
    
    [DebuggerStepThrough]
    internal static class ThreadAffinity
    {
        static int? mainThreadId;
        static int? simThreadId;
        static int? uiThreadId;

        public static bool IsMainThread => mainThreadId.HasValue && mainThreadId == Environment.CurrentManagedThreadId;
        public static bool IsSimThread => simThreadId.HasValue && simThreadId == Environment.CurrentManagedThreadId;
        public static bool IsUiThread => uiThreadId.HasValue && uiThreadId == Environment.CurrentManagedThreadId;

        public static bool IsGameThread
        {
            get
            {
                //Q: Is this purely to catch worker threads touching things that is shouldnt? If so, bad name. If not, still bad name.
                int current = Environment.CurrentManagedThreadId;
                if (mainThreadId.HasValue && mainThreadId.Value == current) return true;
                if (simThreadId.HasValue && simThreadId.Value == current) return true;
                if (uiThreadId.HasValue && uiThreadId.Value == current) return true; 
                return false;
            }
        }

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

        public static void AssertGameThread()
        {
            if (IsGameThread) return;
            throw new InvalidOperationException("This operation must run on a registered game thread (main, sim, or UI).");
        }
    }
}
