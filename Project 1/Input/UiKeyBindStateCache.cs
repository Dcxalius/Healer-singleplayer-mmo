using Project_1.Messaging.Events;
using Project_1.Managers;

namespace Project_1.Input
{
    /// <summary>
    /// UI-thread keybind snapshot updated via UI mailbox events.
    /// </summary>
    internal static class UiKeyBindStateCache
    {
        static bool[] pressed = new bool[(int)KeyBindManager.KeyListner.Count];
        static bool[] held = new bool[(int)KeyBindManager.KeyListner.Count];
        static bool[] released = new bool[(int)KeyBindManager.KeyListner.Count];

        public static void Update(KeyBindSnapshot snapshot)
        {
            ThreadAffinity.AssertUiThread();
            if (snapshot.Pressed != null) pressed = snapshot.Pressed;
            if (snapshot.Held != null) held = snapshot.Held;
            if (snapshot.Released != null) released = snapshot.Released;
        }

        public static bool GetPress(KeyBindManager.KeyListner key)
        {
            ThreadAffinity.AssertUiThread();
            return pressed[(int)key];
        }

        public static bool GetHold(KeyBindManager.KeyListner key)
        {
            ThreadAffinity.AssertUiThread();
            return held[(int)key];
        }

        public static bool GetRelease(KeyBindManager.KeyListner key)
        {
            ThreadAffinity.AssertUiThread();
            return released[(int)key];
        }
    }
}
