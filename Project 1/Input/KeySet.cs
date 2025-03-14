using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
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
        public Keys Key => key;
        Keys key;

        public Keys[] Modifiers
        {
            get
            {
                List<Keys> k = new List<Keys>();
                if (modifierKeys[(int)InputManager.HoldModifier.Ctrl] == true) k.Add(Keys.LeftControl);
                if (modifierKeys[(int)InputManager.HoldModifier.Alt] == true) k.Add(Keys.LeftAlt);
                if (modifierKeys[(int)InputManager.HoldModifier.Shift] == true) k.Add(Keys.LeftShift);
                return k.ToArray();
            }
        }
       
        bool[] modifierKeys;

        public KeySet(Keys aKey) : this(aKey, new bool[(int)InputManager.HoldModifier.Count]  { false, false, false }) { }

        public KeySet(Keys aKey, bool[] aModifierKeys)
        {
            Debug.Assert(aKey != Keys.Escape && aKey != Keys.LeftShift && aKey != Keys.RightShift && aKey != Keys.LeftAlt && aKey != Keys.RightAlt && aKey != Keys.LeftControl && aKey != Keys.RightControl && aKey != Keys.LeftWindows && aKey != Keys.RightWindows);
            //TODO: ^ Handle this by checking outside and refusing to assign and create a popup or msg
            key = aKey;
            modifierKeys = aModifierKeys;
        }

        public KeySet(Keys aKey, Keys aModKey) : this(aKey, new Keys[] { aModKey }) { }

        public KeySet(Keys aKey, Keys aModKey, Keys aSecondModkey) : this(aKey, new Keys[] { aModKey, aSecondModkey }) { }

        [JsonConstructor]
        public KeySet(Keys key, Keys[] modifiers) : this(key)
        {
            for (int i = 0; i < modifiers.Length; i++)
            {
                Debug.Assert(modifiers[i] == Keys.LeftShift || modifiers[i] == Keys.RightShift || modifiers[i] == Keys.LeftAlt || modifiers[i] == Keys.RightAlt || modifiers[i] == Keys.LeftControl || modifiers[i] == Keys.RightControl);

                switch (modifiers[i])
                {
                    case Keys.LeftShift:
                    case Keys.RightShift:
                        modifierKeys[(int)InputManager.HoldModifier.Shift] = true;
                        break;
                    case Keys.LeftAlt:
                    case Keys.RightAlt:
                        modifierKeys[(int)InputManager.HoldModifier.Alt] = true;
                        break;
                    case Keys.LeftControl:
                    case Keys.RightControl:
                        modifierKeys[(int)InputManager.HoldModifier.Ctrl] = true;
                        break;
                }
            }
        }

        public bool GetPress => CheckKey(new Func<Keys, bool>(InputManager.GetPress));
        public bool GetHold => CheckKey(new Func<Keys, bool>(InputManager.GetHold));
        public bool GetRelease => CheckKey(new Func<Keys, bool>(InputManager.GetRelease));

        bool CheckKey(Func<Keys, bool> a)
        {
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
