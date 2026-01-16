using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Project_1.Messaging.Events;

namespace Project_1.Input
{
    /// <summary>
    /// Sim-thread keyboard state derived from UI-routed snapshots.
    /// </summary>
    internal static class KeyboardStateCache
    {
        static readonly HashSet<Keys> current = new HashSet<Keys>();
        static readonly HashSet<Keys> previous = new HashSet<Keys>();

        public static void Update(KeyboardSnapshot snapshot)
        {
            previous.Clear();
            previous.UnionWith(current);
            current.Clear();
            if (snapshot.DownKeys == null)
            {
                return;
            }
            current.UnionWith(snapshot.DownKeys);
        }

        public static bool GetPress(Keys key) => current.Contains(key) && !previous.Contains(key);

        public static bool GetHold(Keys key) => current.Contains(key);

        public static bool GetRelease(Keys key) => !current.Contains(key) && previous.Contains(key);
    }
}
