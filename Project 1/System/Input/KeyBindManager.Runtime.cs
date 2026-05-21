using Project_1.Managers;
using System;
using System.Linq;

namespace Project_1.Input
{
    internal static partial class KeyBindManager
    {
        public readonly struct KeyBindingChangedEvent
        {
            public KeyBindingChangedEvent(bool firstButton, KeyListner listner, KeySet key)
            {
                FirstButton = firstButton;
                Listner = listner;
                Key = key;
            }

            public bool FirstButton { get; }
            public KeyListner Listner { get; }
            public KeySet Key { get; }
        }

        public static event Action<KeyBindingChangedEvent> KeyBindingChanged;

        public static bool GetPress(KeyListner aListner)
        {
            ThreadAffinity.AssertMainThread();
            return firstButtons[(int)aListner].GetPress || secondButtons[(int)aListner].GetPress;
        }

        public static bool GetHold(KeyListner aListner)
        {
            ThreadAffinity.AssertMainThread();
            return firstButtons[(int)aListner].GetHold || secondButtons[(int)aListner].GetHold;
        }

        public static bool GetRelease(KeyListner aListner)
        {
            ThreadAffinity.AssertMainThread();
            return firstButtons[(int)aListner].GetRelease || secondButtons[(int)aListner].GetRelease;
        }

        public static void SetKey(bool aFirstButton, KeyListner aListner, KeySet aKey)
        {
            if (aFirstButton)
            {
                firstButtons[(int)aListner] = aKey;
            }
            else
            {
                secondButtons[(int)aListner] = aKey;
            }

            KeyBindingChanged?.Invoke(new KeyBindingChangedEvent(aFirstButton, aListner, aKey));
        }

        public static bool CheckForNoDupeKeys(KeySet aKeySet)
        {
            //TODO: Consider complexing this, what keys can be doubled?

            if (firstButtons.Contains(aKeySet) || secondButtons.Contains(aKeySet)) return false;
            return true;
        }
    }
}
