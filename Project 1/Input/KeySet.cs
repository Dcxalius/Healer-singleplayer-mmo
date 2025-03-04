using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Input
{
    internal struct KeySet
    {
        Keys key;
       
        bool[] modifierKeys;

        public KeySet(Keys aKey) : this(aKey, new bool[(int)InputManager.HoldModifier.Count]  { false, false, false }) { }

        public KeySet(Keys aKey, bool[] aModifierKeys)
        {
            Debug.Assert(aKey != Keys.Escape && aKey != Keys.LeftShift && aKey != Keys.RightShift && aKey != Keys.LeftAlt && aKey != Keys.RightAlt && aKey != Keys.LeftControl && aKey != Keys.RightControl && aKey != Keys.LeftWindows && aKey != Keys.RightWindows);
            //TODO: ^ Handle this by checking outside and refusing to assign and create a popup or msg
            key = aKey;
            modifierKeys = aModifierKeys;
        }

        public bool GetPress => CheckKey(new Func<Keys, bool>(InputManager.GetPress));
        public bool GetHold => CheckKey(new Func<Keys, bool>(InputManager.GetHold));
        public bool GetRelease => CheckKey(new Func<Keys, bool>(InputManager.GetRelease));

        bool CheckKey(Func<Keys, bool> a)
        {
            if (modifierKeys.Length == 0) return a.Invoke(key);
            if (GetModifiers) return a.Invoke(key);

            return false;
        }

        bool GetModifiers
        {
            get
            {
                bool[] currentlyPressedModifiers = InputManager.CheckHoldModifiers();

                for (int i = 0; i < modifierKeys.Length; i++)
                {
                    if (modifierKeys[i] != currentlyPressedModifiers[i]) return false;
                }
                return true;
            }
        }

        public static implicit operator KeySet(Keys aKey) { return new KeySet(aKey); }
    }
}
