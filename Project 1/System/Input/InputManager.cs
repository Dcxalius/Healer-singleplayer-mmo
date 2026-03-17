using Microsoft.Xna.Framework.Input;
using Project_1.Managers;
using System;

namespace Project_1.Input
{
    internal static partial class InputManager
    {
        public enum HoldModifier
        {
            Ctrl,
            Alt,
            Shift,
            Count
        }

        public enum ClickType
        {
            Left,
            Middle,
            Right
        }

        public static bool IsModifier(Keys aKey) =>
            aKey == Keys.LeftShift ||
            aKey == Keys.RightShift ||
            aKey == Keys.LeftAlt ||
            aKey == Keys.RightAlt ||
            aKey == Keys.LeftControl ||
            aKey == Keys.RightControl;

        public static bool IsMouseDown(ClickType aClickType)
        {
            return aClickType switch
            {
                ClickType.Left => newMouseState.LeftButton == ButtonState.Pressed,
                ClickType.Middle => newMouseState.MiddleButton == ButtonState.Pressed,
                ClickType.Right => newMouseState.RightButton == ButtonState.Pressed,
                _ => throw new NotImplementedException()
            };
        }

        static KeyboardState oldKeyboardState = Keyboard.GetState();
        static KeyboardState newKeyboardState = Keyboard.GetState();
        static MouseState newMouseState;
        static MouseState oldMouseState;
        static int scrollDelta;
        static int scrollWheelValue;
        const int KeyBindMaskBitCount = sizeof(ulong) * 8;

        static bool isFocused;

        public static void Init(ref bool aIsFocused)
        {
            isFocused = aIsFocused;
        }

        public static void Update()
        {
            ThreadAffinity.AssertMainThread();
            UpdateStates();
            PublishEscapePressed();
            UpdateScrollWheel();
            CheckButtonPress();
            PublishKeyboardSnapshots();
            PublishMouseSnapshot();
        }
    }
}
