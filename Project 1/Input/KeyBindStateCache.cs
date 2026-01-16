using Project_1.Messaging.Events;

namespace Project_1.Input
{
    /// <summary>
    /// Sim-thread keybind snapshot updated via UI-routed events.
    /// </summary>
    internal static class KeyBindStateCache
    {
        static bool[] pressed = new bool[(int)KeyBindManager.KeyListner.Count];
        static bool[] held = new bool[(int)KeyBindManager.KeyListner.Count];
        static bool[] released = new bool[(int)KeyBindManager.KeyListner.Count];

        public static void Update(KeyBindSnapshot snapshot)
        {
            if (snapshot.Pressed != null) pressed = snapshot.Pressed;
            if (snapshot.Held != null) held = snapshot.Held;
            if (snapshot.Released != null) released = snapshot.Released;
        }

        public static bool GetPress(KeyBindManager.KeyListner key) => pressed[(int)key];

        public static bool GetHold(KeyBindManager.KeyListner key) => held[(int)key];

        public static bool GetRelease(KeyBindManager.KeyListner key) => released[(int)key];
    }
}
