using Microsoft.Xna.Framework.Input;
using Project_1.Managers;
using System.Collections.Generic;
using System.Linq;

namespace Project_1.Input
{
    internal static partial class InputManager
    {
        public static bool LeftPress => newMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released;
        public static bool LeftHold => newMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Pressed;
        public static bool LeftRelease => newMouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed;

        public static bool RightPress => newMouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released;
        public static bool RightHold => newMouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Pressed;
        public static bool RightRelease => newMouseState.RightButton == ButtonState.Released && oldMouseState.RightButton == ButtonState.Pressed;

        public static Keys? GetAnyKey
        {
            get
            {
                IEnumerable<Keys> pressedKeys = newKeyboardState.GetPressedKeys().Except(oldKeyboardState.GetPressedKeys());
                if (!pressedKeys.Any())
                {
                    return null;
                }

                return pressedKeys.First();
            }
        }

        static void UpdateStates()
        {
            oldKeyboardState = newKeyboardState;
            newKeyboardState = Keyboard.GetState();

            oldMouseState = newMouseState;
            newMouseState = Mouse.GetState();
            scrollWheelValue = newMouseState.ScrollWheelValue;
            scrollDelta = newMouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue;
        }

        public static bool GetPress(Keys key)
        {
            ThreadAffinity.AssertMainThread();
            if (UiTextInputManager.IsActive) return false;
            return !oldKeyboardState.IsKeyDown(key) && newKeyboardState.IsKeyDown(key);
        }

        public static bool GetHold(Keys key)
        {
            ThreadAffinity.AssertMainThread();
            if (UiTextInputManager.IsActive) return false;
            return oldKeyboardState.IsKeyDown(key) || newKeyboardState.IsKeyDown(key);
        }

        public static bool GetRelease(Keys key)
        {
            ThreadAffinity.AssertMainThread();
            if (UiTextInputManager.IsActive) return false;
            return oldKeyboardState.IsKeyDown(key) && !newKeyboardState.IsKeyDown(key);
        }

        public static bool IsHoldModifierHeld(HoldModifier modifier)
        {
            ThreadAffinity.AssertMainThread();
            return modifier switch
            {
                HoldModifier.Ctrl => GetHold(Keys.LeftControl) || GetHold(Keys.RightControl),
                HoldModifier.Alt => GetHold(Keys.LeftAlt) || GetHold(Keys.RightAlt),
                HoldModifier.Shift => GetHold(Keys.LeftShift) || GetHold(Keys.RightShift),
                _ => false
            };
        }

        public static byte GetHoldModifierMask()
        {
            ThreadAffinity.AssertMainThread();
            byte mask = 0;
            if (IsHoldModifierHeld(HoldModifier.Ctrl)) mask |= (byte)(1 << (int)HoldModifier.Ctrl);
            if (IsHoldModifierHeld(HoldModifier.Alt)) mask |= (byte)(1 << (int)HoldModifier.Alt);
            if (IsHoldModifierHeld(HoldModifier.Shift)) mask |= (byte)(1 << (int)HoldModifier.Shift);
            return mask;
        }
    }
}
