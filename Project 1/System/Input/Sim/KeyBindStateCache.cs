using Project_1.Messaging.Events;
using Project_1.Managers;

namespace Project_1.Input
{
    /// <summary>
    /// Sim-thread keybind snapshot updated via UI-routed events.
    /// </summary>
    internal static class KeyBindStateCache
    {
        static ulong pressedMask;
        static ulong heldMask;
        static ulong releasedMask;
        static int snapshotVersion;
        static int lastFrameVersion = -1;

        public static void Update(KeyBindSnapshot snapshot)
        {
            ThreadAffinity.AssertSimThread();
            snapshotVersion++;
            pressedMask = snapshot.PressedMask;
            heldMask = snapshot.HeldMask;
            releasedMask = snapshot.ReleasedMask;
        }

        public static void BeginFrame()
        {
            ThreadAffinity.AssertSimThread();
            if (lastFrameVersion == snapshotVersion)
            {
                pressedMask = 0;
                releasedMask = 0;
            }
            lastFrameVersion = snapshotVersion;
        }

        public static bool GetPress(KeyBindManager.KeyListner key)
        {
            ThreadAffinity.AssertSimThread();
            int index = (int)key;
            return index >= 0 && index < 64 && ((pressedMask & (1UL << index)) != 0);
        }

        public static bool GetHold(KeyBindManager.KeyListner key)
        {
            ThreadAffinity.AssertSimThread();
            int index = (int)key;
            return index >= 0 && index < 64 && ((heldMask & (1UL << index)) != 0);
        }

        public static bool GetRelease(KeyBindManager.KeyListner key)
        {
            ThreadAffinity.AssertSimThread();
            int index = (int)key;
            return index >= 0 && index < 64 && ((releasedMask & (1UL << index)) != 0);
        }
    }
}
