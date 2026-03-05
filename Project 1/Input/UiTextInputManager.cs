using System;
using System.Collections.Concurrent;
using Microsoft.Xna.Framework;
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
        static readonly ConcurrentQueue<char> pendingCharacters = new ConcurrentQueue<char>();
        static readonly EventHandler<TextInputEventArgs> textInputHandler = HandleTextInput;
        static InputBox activeInput;
        static int cursorPosition;
        static volatile bool isActive;
        static bool textInputRegistered;

        public static bool IsActive => isActive;
        public static InputBox ActiveInput => activeInput;
        public static int CursorPosition => cursorPosition;

        public static void Begin(InputBox input)
        {
            ThreadAffinity.AssertUiThread();
            activeInput = input;
            cursorPosition = input?.Input.Length ?? 0;
            isActive = input != null;
            ClearPendingCharacters();
            if (isActive)
            {
                EnsureTextInputRegistration();
            }
            else
            {
                RemoveTextInputRegistration();
            }
        }

        public static void Clear()
        {
            ThreadAffinity.AssertUiThread();
            activeInput = null;
            cursorPosition = 0;
            isActive = false;
            ClearPendingCharacters();
            RemoveTextInputRegistration();
        }

        public static void Update()
        {
            ThreadAffinity.AssertUiThread();
            if (activeInput == null) return;

            ProcessControlKeys();
            if (activeInput == null) return;

            while (pendingCharacters.TryDequeue(out char c))
            {
                if (activeInput == null) return;
                if (char.IsControl(c)) continue;
                if (!activeInput.ValidInput(c)) continue;
                if (!activeInput.WriteTo(c, cursorPosition)) continue;
                cursorPosition++;
            }
        }

        static void ProcessControlKeys()
        {
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

        static void EnsureTextInputRegistration()
        {
            if (textInputRegistered) return;
            if (Game1.Instance is not Game1 game) return;
            game.RegisterToTextInput(textInputHandler);
            textInputRegistered = true;
        }

        static void RemoveTextInputRegistration()
        {
            if (!textInputRegistered) return;
            if (Game1.Instance is not Game1 game) return;
            game.UnregisterFromTextInput();
            textInputRegistered = false;
        }

        static void ClearPendingCharacters()
        {
            while (pendingCharacters.TryDequeue(out _))
            {
            }
        }

        static void HandleTextInput(object sender, TextInputEventArgs e)
        {
            if (!isActive || activeInput == null) return;
            char c = e.Character;
            if (c == '\0') return;
            pendingCharacters.Enqueue(c);
        }
    }
}
