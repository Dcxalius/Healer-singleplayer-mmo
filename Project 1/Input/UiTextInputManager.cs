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
            ThreadAffinity.AssertUiThread();
            activeInput = input;
            cursorPosition = input?.Input.Length ?? 0;
            isActive = input != null;
        }

        public static void Clear()
        {
            ThreadAffinity.AssertUiThread();
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

                bool shiftHeld = UiKeyboardStateCache.GetHold(Keys.LeftShift) || UiKeyboardStateCache.GetHold(Keys.RightShift);
                if (!TryConvertToCharacter(key, shiftHeld, out char s)) continue;

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

        static bool TryConvertToCharacter(Keys key, bool shiftHeld, out char character)
        {
            character = '\0';

            if (key >= Keys.A && key <= Keys.Z)
            {
                char baseChar = (char)('a' + (key - Keys.A));
                character = shiftHeld ? char.ToUpper(baseChar) : baseChar;
                return true;
            }

            if (key >= Keys.D0 && key <= Keys.D9)
            {
                int digit = key - Keys.D0;
                character = shiftHeld
                    ? digit switch
                    {
                        0 => ')',
                        1 => '!',
                        2 => '@',
                        3 => '#',
                        4 => '$',
                        5 => '%',
                        6 => '^',
                        7 => '&',
                        8 => '*',
                        9 => '(',
                        _ => '\0'
                    }
                    : (char)('0' + digit);
                return character != '\0';
            }

            if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
            {
                int digit = key - Keys.NumPad0;
                character = (char)('0' + digit);
                return true;
            }

            switch (key)
            {
                case Keys.Space:
                    character = ' ';
                    return true;
                case Keys.OemMinus:
                    character = shiftHeld ? '_' : '-';
                    return true;
                case Keys.OemPlus:
                    character = shiftHeld ? '+' : '=';
                    return true;
                case Keys.OemOpenBrackets:
                    character = shiftHeld ? '{' : '[';
                    return true;
                case Keys.OemCloseBrackets:
                    character = shiftHeld ? '}' : ']';
                    return true;
                case Keys.OemPipe:
                case Keys.OemBackslash:
                    character = shiftHeld ? '|' : '\\';
                    return true;
                case Keys.OemSemicolon:
                    character = shiftHeld ? ':' : ';';
                    return true;
                case Keys.OemQuotes:
                    character = shiftHeld ? '"' : '\'';
                    return true;
                case Keys.OemComma:
                    character = shiftHeld ? '<' : ',';
                    return true;
                case Keys.OemPeriod:
                    character = shiftHeld ? '>' : '.';
                    return true;
                case Keys.OemQuestion:
                    character = shiftHeld ? '?' : '/';
                    return true;
                case Keys.OemTilde:
                    character = shiftHeld ? '~' : '`';
                    return true;
                case Keys.Decimal:
                    character = '.';
                    return true;
                default:
                    return false;
            }
        }
    }
}
