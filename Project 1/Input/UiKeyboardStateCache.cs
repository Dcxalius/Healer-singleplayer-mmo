using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Project_1.Managers;
using Project_1.Messaging.Events;

namespace Project_1.Input
{
    /// <summary>
    /// UI-thread keyboard state derived from UI mailbox snapshots.
    /// </summary>
    internal static class UiKeyboardStateCache
    {
        static readonly HashSet<Keys> current = new HashSet<Keys>();
        static readonly HashSet<Keys> previous = new HashSet<Keys>();
        static Keys[] lastDownKeys = Array.Empty<Keys>();

        public static Keys[] DownKeys
        {
            get
            {
                ThreadAffinity.AssertUiThread();
                return lastDownKeys;
            }
        }

        public static void Update(KeyboardSnapshot snapshot)
        {
            ThreadAffinity.AssertUiThread();
            previous.Clear();
            previous.UnionWith(current);
            current.Clear();

            lastDownKeys = snapshot.DownKeys ?? Array.Empty<Keys>();
            if (lastDownKeys.Length == 0)
            {
                return;
            }
            current.UnionWith(lastDownKeys);
        }

        public static bool GetPress(Keys key)
        {
            ThreadAffinity.AssertUiThread();
            return current.Contains(key) && !previous.Contains(key);
        }

        public static bool GetHold(Keys key)
        {
            ThreadAffinity.AssertUiThread();
            return current.Contains(key);
        }

        public static bool GetRelease(Keys key)
        {
            ThreadAffinity.AssertUiThread();
            return !current.Contains(key) && previous.Contains(key);
        }

        public static bool IsNewlyPressed(Keys key)
        {
            ThreadAffinity.AssertUiThread();
            return current.Contains(key) && !previous.Contains(key);
        }

        public static Keys? GetAnyKey()
        {
            ThreadAffinity.AssertUiThread();
            for (int i = 0; i < lastDownKeys.Length; i++)
            {
                Keys key = lastDownKeys[i];
                if (key == Keys.None) continue;
                if (!previous.Contains(key)) return key;
            }
            return null;
        }

        public static bool[] GetHoldModifiers()
        {
            ThreadAffinity.AssertUiThread();
            bool[] heldModifiers = new bool[(int)InputManager.HoldModifier.Count];
            heldModifiers[(int)InputManager.HoldModifier.Ctrl] = GetHold(Keys.LeftControl) || GetHold(Keys.RightControl);
            heldModifiers[(int)InputManager.HoldModifier.Alt] = GetHold(Keys.LeftAlt) || GetHold(Keys.RightAlt);
            heldModifiers[(int)InputManager.HoldModifier.Shift] = GetHold(Keys.LeftShift) || GetHold(Keys.RightShift);
            return heldModifiers;
        }

        public static bool IsModifier(Keys key) =>
            key == Keys.LeftShift || key == Keys.RightShift ||
            key == Keys.LeftAlt || key == Keys.RightAlt ||
            key == Keys.LeftControl || key == Keys.RightControl;
    }
}
