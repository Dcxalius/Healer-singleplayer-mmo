using System;
using Microsoft.Xna.Framework.Input;
using Project_1.Managers;
using Project_1.UI.UIElements.Boxes;

namespace Project_1.Input
{
    /// <summary>
    /// UI-thread text input handler for InputBox widgets.
    /// </summary>
    internal static class UiTextInputManager
    {
        static InputBox activeInput;
        static int cursorPosition;
        static volatile bool isActive;

        public static bool IsActive => isActive;
        public static InputBox ActiveInput => activeInput;
        public static int CursorPosition => cursorPosition;

        public static void Begin(InputBox input)
        {
            activeInput = input;
            cursorPosition = input?.Input.Length ?? 0;
            isActive = input != null;
        }

        public static void Clear()
        {
            activeInput = null;
            cursorPosition = 0;
            isActive = false;
        }

        public static void Update()
        {
            ThreadAffinity.AssertUiThread();
            if (activeInput == null) return;

            Keys[] downKeys = UiKeyboardStateCache.DownKeys;
            for (int i = 0; i < downKeys.Length; i++)
            {
                Keys key = downKeys[i];
                if (key == Keys.None || !UiKeyboardStateCache.IsNewlyPressed(key)) continue;
                if (HandleRemove(key)) continue;
                if (key == Keys.Enter)
                {
                    activeInput.Enter();
                    Clear();
                    return;
                }

                HandleCursorMovement(key);

                if (!activeInput.ValidInput(key)) continue;

                string keyName = key.ToString();
                char s = keyName[keyName.Length - 1];
                bool shiftHeld = UiKeyboardStateCache.GetHold(Keys.LeftShift) || UiKeyboardStateCache.GetHold(Keys.RightShift);
                if (!shiftHeld) s = char.ToLower(s);

                if (!activeInput.WriteTo(s, cursorPosition)) continue;
                cursorPosition++;
            }
        }

        static bool HandleRemove(Keys key)
        {
            if (key != Keys.Back && key != Keys.Delete) return false;
            bool ctrlHeld = UiKeyboardStateCache.GetHold(Keys.LeftControl) || UiKeyboardStateCache.GetHold(Keys.RightControl);
            if (key == Keys.Delete)
            {
                activeInput.Delete(ctrlHeld, cursorPosition);
                CursorBoundsCheck();
                return true;
            }
            int length = activeInput.Input.Length;
            activeInput.Backstep(ctrlHeld, cursorPosition);
            if (ctrlHeld) cursorPosition -= length - activeInput.Input.Length;
            else cursorPosition--;
            CursorBoundsCheck();
            return true;
        }

        static void HandleCursorMovement(Keys key)
        {
            if (key != Keys.Left && key != Keys.Right) return;
            if (key == Keys.Left) cursorPosition--;
            if (key == Keys.Right) cursorPosition++;
            CursorBoundsCheck();
        }

        static void CursorBoundsCheck()
        {
            if (cursorPosition < 0) cursorPosition = 0;
            int max = activeInput.Input.Length;
            if (cursorPosition > max) cursorPosition = max;
        }
    }
}
