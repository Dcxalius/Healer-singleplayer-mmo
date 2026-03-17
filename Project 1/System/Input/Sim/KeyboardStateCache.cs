using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Project_1.Managers;
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
        static int snapshotVersion;
        static int lastFrameVersion = -1;

        public static void Update(KeyboardSnapshot snapshot)
        {
            ThreadAffinity.AssertSimThread();
            snapshotVersion++;
            previous.Clear();
            previous.UnionWith(current);
            current.Clear();
            if (snapshot.DownKeys == null)
            {
                return;
            }
            current.UnionWith(snapshot.DownKeys);
        }

        public static void BeginFrame()
        {
            ThreadAffinity.AssertSimThread();
            if (lastFrameVersion == snapshotVersion)
            {
                previous.Clear();
                previous.UnionWith(current);
            }
            lastFrameVersion = snapshotVersion;
        }

        public static bool GetPress(Keys key)
        {
            ThreadAffinity.AssertSimThread();
            return current.Contains(key) && !previous.Contains(key);
        }

        public static bool GetHold(Keys key)
        {
            ThreadAffinity.AssertSimThread();
            return current.Contains(key);
        }

        public static bool GetRelease(Keys key)
        {
            ThreadAffinity.AssertSimThread();
            return !current.Contains(key) && previous.Contains(key);
        }
    }
}
