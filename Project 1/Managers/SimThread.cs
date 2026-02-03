using System.Threading;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Managers.States;

namespace Project_1.Managers
{
    /// <summary>
    /// Background simulation worker for processing main-thread commands and running game updates.
    /// </summary>
    internal static class SimThread
    {
        static Thread thread;
        static volatile bool running;
        static readonly AutoResetEvent pulse = new AutoResetEvent(false);
        static readonly AutoResetEvent completed = new AutoResetEvent(false);

        public static bool IsRunning => running;

        public static void Start()
        {
            ThreadAffinity.AssertMainThread();
            if (running) return;
            running = true;
            Mailboxes.Main.Subscribe<WorkerCompletionReady>(e => WorkerPool.RunCompletion(e.CompletionId));
            thread = new Thread(Run)
            {
                IsBackground = true,
                Name = "Sim Thread"
            };
            thread.Start();
        }

        public static void Stop()
        {
            ThreadAffinity.AssertMainThread();
            running = false;
            pulse.Set();
        }

        public static void PulseAndWait(int timeoutMs = 16)
        {
            ThreadAffinity.AssertMainThread();
            if (!running) return;
            pulse.Set();
            completed.WaitOne(timeoutMs);
        }

        static void Run()
        {
            ThreadAffinity.RegisterSimThread();
            while (running)
            {
                pulse.WaitOne();
                if (!running)
                {
                    completed.Set();
                    break;
                }
                Mailboxes.Main.DispatchAll();
                StateManager.Update();
                Mailboxes.Main.DispatchAll();
                DebugManager.Update();
                completed.Set();
            }
        }
    }
}
