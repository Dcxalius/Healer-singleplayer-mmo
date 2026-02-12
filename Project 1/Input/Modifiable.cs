using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Input
{
    internal class Modifiable
    {
        protected virtual byte ModifiersMask { get; }
        public byte ModifierMask => ModifiersMask;

        public bool Modifier(InputManager.HoldModifier aHoldModifier) => (ModifiersMask & (1 << (int)aHoldModifier)) != 0;
        public bool ModifiersOr(InputManager.HoldModifier[] aHoldModifier)
        {
            for (int i = 0; i < aHoldModifier.Length; i++)
            {
                if (Modifier(aHoldModifier[i]))
                {
                    return true;
                }
            }
            return false;
        }
        public bool Modifiers(InputManager.HoldModifier[] aHoldModifier)
        {
            for (int i = 0; i < aHoldModifier.Length; i++)
            {
                if (!Modifier(aHoldModifier[i]))
                {
                    return false;
                }
            }
            return true;

        }
        public bool NoModifiers() => ModifiersMask == 0;

        protected static byte BuildModifiersMask(bool[] modifiersHeld)
        {
            byte mask = 0;
            if (modifiersHeld == null) return mask;
            if (modifiersHeld.Length > (int)InputManager.HoldModifier.Ctrl && modifiersHeld[(int)InputManager.HoldModifier.Ctrl]) mask |= (byte)(1 << (int)InputManager.HoldModifier.Ctrl);
            if (modifiersHeld.Length > (int)InputManager.HoldModifier.Alt && modifiersHeld[(int)InputManager.HoldModifier.Alt]) mask |= (byte)(1 << (int)InputManager.HoldModifier.Alt);
            if (modifiersHeld.Length > (int)InputManager.HoldModifier.Shift && modifiersHeld[(int)InputManager.HoldModifier.Shift]) mask |= (byte)(1 << (int)InputManager.HoldModifier.Shift);
            return mask;
        }

        protected static bool[] ToModifiersArray(byte modifiersMask)
        {
            bool[] modifiers = new bool[(int)InputManager.HoldModifier.Count];
            modifiers[(int)InputManager.HoldModifier.Ctrl] = (modifiersMask & (1 << (int)InputManager.HoldModifier.Ctrl)) != 0;
            modifiers[(int)InputManager.HoldModifier.Alt] = (modifiersMask & (1 << (int)InputManager.HoldModifier.Alt)) != 0;
            modifiers[(int)InputManager.HoldModifier.Shift] = (modifiersMask & (1 << (int)InputManager.HoldModifier.Shift)) != 0;
            return modifiers;
        }
    }
}
