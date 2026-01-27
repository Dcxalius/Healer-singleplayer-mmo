using System;
using Project_1.Messaging.Events;
using Project_1.Managers;

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
        static int snapshotVersion;
        static int lastFrameVersion = -1;

        public static void Update(KeyBindSnapshot snapshot)
        {
            ThreadAffinity.AssertSimThread();
            snapshotVersion++;
            if (snapshot.Pressed != null) CopyInto(ref pressed, snapshot.Pressed);
            if (snapshot.Held != null) CopyInto(ref held, snapshot.Held);
            if (snapshot.Released != null) CopyInto(ref released, snapshot.Released);
        }

        public static void BeginFrame()
        {
            ThreadAffinity.AssertSimThread();
            if (lastFrameVersion == snapshotVersion)
            {
                if (pressed.Length > 0) Array.Clear(pressed, 0, pressed.Length);
                if (released.Length > 0) Array.Clear(released, 0, released.Length);
            }
            lastFrameVersion = snapshotVersion;
        }

        public static bool GetPress(KeyBindManager.KeyListner key)
        {
            ThreadAffinity.AssertSimThread();
            return pressed[(int)key];
        }

        public static bool GetHold(KeyBindManager.KeyListner key)
        {
            ThreadAffinity.AssertSimThread();
            return held[(int)key];
        }

        public static bool GetRelease(KeyBindManager.KeyListner key)
        {
            ThreadAffinity.AssertSimThread();
            return released[(int)key];
        }

        static void CopyInto(ref bool[] destination, bool[] source)
        {
            if (destination == null || destination.Length != source.Length)
            {
                destination = new bool[source.Length];
            }
            Array.Copy(source, destination, source.Length);
        }
    }
}
