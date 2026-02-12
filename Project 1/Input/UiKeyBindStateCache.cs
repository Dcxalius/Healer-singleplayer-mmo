using Project_1.Messaging.Events;
using Project_1.Managers;

namespace Project_1.Input
{
    /// <summary>
    /// UI-thread keybind snapshot updated via UI mailbox events.
    /// </summary>
    internal static class UiKeyBindStateCache
    {
        static ulong pressedMask;
        static ulong heldMask;
        static ulong releasedMask;

        public static void Update(KeyBindSnapshot snapshot)
        {
            ThreadAffinity.AssertUiThread();
            pressedMask = snapshot.PressedMask;
            heldMask = snapshot.HeldMask;
            releasedMask = snapshot.ReleasedMask;
        }

        public static bool GetPress(KeyBindManager.KeyListner key)
        {
            ThreadAffinity.AssertUiThread();
            int index = (int)key;
            return index >= 0 && index < 64 && ((pressedMask & (1UL << index)) != 0);
        }

        public static bool GetHold(KeyBindManager.KeyListner key)
        {
            ThreadAffinity.AssertUiThread();
            int index = (int)key;
            return index >= 0 && index < 64 && ((heldMask & (1UL << index)) != 0);
        }

        public static bool GetRelease(KeyBindManager.KeyListner key)
        {
            ThreadAffinity.AssertUiThread();
            int index = (int)key;
            return index >= 0 && index < 64 && ((releasedMask & (1UL << index)) != 0);
        }
    }
}
