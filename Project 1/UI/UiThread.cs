using Project_1.Input;
using Project_1.Managers;
using Project_1.Managers.States;
using Project_1.Messaging;
using Project_1.UI.HUD.Managers;
using System.Threading;

namespace Project_1.UI
{
    /// <summary>
    /// Background UI worker for dispatching UI events and updating UI state.
    /// </summary>
    internal static class UiThread
    {
        static Thread thread;
        static volatile bool running;
        static volatile bool updateRequested;
        static readonly AutoResetEvent pulse = new AutoResetEvent(false);
        static readonly AutoResetEvent completed = new AutoResetEvent(false);

        public static bool IsRunning => running;

        public static void Start()
        {
            if (running) return;
            running = true;
            thread = new Thread(Run)
            {
                IsBackground = true,
                Name = "UI Thread"
            };
            thread.Start();
        }

        public static void Stop()
        {
            running = false;
            pulse.Set();
        }

        public static void Pulse(bool updateHud)
        {
            if (!running) return;
            updateRequested |= updateHud;
            pulse.Set();
        }

        public static void PulseAndWait(bool updateHud, int timeoutMs = 16)
        {
            if (!running) return;
            updateRequested |= updateHud;
            pulse.Set();
            completed.WaitOne(timeoutMs);
        }

        static void Run()
        {
            ThreadAffinity.RegisterUiThread();
            while (running)
            {
                pulse.WaitOne();
                if (!running)
                {
                    completed.Set();
                    break;
                }
                bool doUpdate = updateRequested;
                updateRequested = false;
                lock (HUDManager.UiLock)
                {
                    Mailboxes.Ui.DispatchAll();
                    if (doUpdate)
                    {
                        UiTextInputManager.Update();
                        StateManager.UiUpdate();
                        HUDManager.Update();
                    }
                    HUDManager.BuildDrawLists();
                }
                completed.Set();
            }
        }
    }
}
