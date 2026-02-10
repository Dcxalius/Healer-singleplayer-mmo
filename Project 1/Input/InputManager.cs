using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Spawners.Pathing;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.UI;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Input
{
    internal static class InputManager
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

        public static bool IsModifier(Keys aKey) => aKey == Keys.LeftShift || aKey == Keys.RightShift || aKey == Keys.LeftAlt || aKey == Keys.RightAlt || aKey == Keys.LeftControl || aKey == Keys.RightControl;

        public static bool WritingToLabel => inputToWriteTo != null;
        public static InputBox InputToWriteTo 
        {
            get => inputToWriteTo; 
            set 
            { 
                inputToWriteTo = value; 
                cursorPosition = inputToWriteTo == null ? 0 : inputToWriteTo.Input.Length; 
            }
        }
        static InputBox inputToWriteTo;
        public static int CursorPosition => cursorPosition;
        static int cursorPosition;

        public static bool LeftPress
        {
            get
            {
                if (newMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
                {
                    return true;
                }
                return false;
            }
        }

        public static bool LeftHold
        {
            get
            {
                if (newMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Pressed)
                {
                    return true;
                }
                return false;
            }
        }

        public static bool LeftRelease
        {
            get
            {
                if (newMouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
                {
                    return true;

                }
                return false;
            }
        }

        public static bool RightPress
        {
            get
            {
                if (newMouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released)
                {
                    return true;
                }
                return false;
            }
        }

        public static bool RightHold
        {
            get
            {
                if (newMouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Pressed)
                {
                    return true;
                }
                return false;
            }
        }

        public static bool RightRelease
        {
            get
            {
                if (newMouseState.RightButton == ButtonState.Released && oldMouseState.RightButton == ButtonState.Pressed)
                {
                    return true;

                }
                return false;
            }
        }

        public static bool IsMouseDown(ClickType aClickType)
        {
            switch (aClickType)
            {
                case ClickType.Left:
                    return newMouseState.LeftButton == ButtonState.Pressed;

                case ClickType.Middle:
                    return newMouseState.MiddleButton == ButtonState.Pressed;

                case ClickType.Right:
                    return newMouseState.RightButton == ButtonState.Pressed;
                default:
                    throw new NotImplementedException();
            }
        }

        public static Keys? GetAnyKey
        {
            get
            {
                IEnumerable<Keys> a = newKeyboardState.GetPressedKeys().Except(oldKeyboardState.GetPressedKeys());

                if (a.Count() == 0)
                {
                    return null;
                }

                return a.First();
            }
        }

        static KeyboardState oldKeyboardState = Keyboard.GetState();
        static KeyboardState newKeyboardState = Keyboard.GetState();
        static MouseState newMouseState;
        static MouseState oldMouseState;
        static int scrollDelta;
        static int scrollWheelValue;

        static bool isFocused;

        public static void Init(ref bool aIsFocused)
        {
            isFocused = aIsFocused;
            //TODO: Find a way to make the game ignore presses/keys is the window is not in focus
        }

        public static void Update()
        {
            ThreadAffinity.AssertMainThread();
            UpdateStates();
            PublishEscapePressed();
            UpdateScrollWheel();
            CheckButtonPress();
            WriteToLabel();
            PublishKeyboardSnapshots();
            PublishMouseSnapshot();
        }

        private static void WriteToLabel()
        {
            if (!WritingToLabel) return;

            Keys[] keys = newKeyboardState.GetPressedKeys();
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i] == Keys.None) continue;
                if (oldKeyboardState.GetPressedKeys().Contains(keys[i])) continue; //TODO: Add a condition to check if its being hold long enough and then start spamming it with increasing frequency
                if (Remove(keys[i])) continue;
                if (keys[i] == Keys.Enter)
                {
                    InputToWriteTo.Enter();
                    InputToWriteTo = null;
                    return;
                }
                CursorMovement(keys[i]);

                if (!IllegalCharacter(keys[i])) continue;
                
                char s = keys[i].ToString().Last();
                
                if (!(newKeyboardState.IsKeyDown(Keys.LeftShift) || newKeyboardState.IsKeyDown(Keys.RightShift))) s = char.ToLower(s);

                if (!InputToWriteTo.WriteTo(s, cursorPosition)) continue;
                cursorPosition++;
            }
            
        }

        static bool Remove(Keys aPressedKey) //TODO: Change name and make less ugly
        {
            if (aPressedKey != Keys.Back && aPressedKey != Keys.Delete) return false;
            bool ctrlPress = newKeyboardState.IsKeyDown(Keys.LeftControl) || newKeyboardState.IsKeyDown(Keys.RightControl);
            if (aPressedKey == Keys.Delete)
            {
                InputToWriteTo.Delete(ctrlPress, cursorPosition);
                CursorBoundsCheck();
                return true;
            }
            int l = InputToWriteTo.Input.Length;
            InputToWriteTo.Backstep(ctrlPress, cursorPosition);

            if (ctrlPress) cursorPosition -= l - InputToWriteTo.Input.Length;
            else cursorPosition--;
            CursorBoundsCheck();

            return true;
        }

        static void CursorMovement(Keys aPressedKey)
        {
            if (aPressedKey != Keys.Left && aPressedKey != Keys.Right) return;
            if (aPressedKey == Keys.Left) cursorPosition--;
            if (aPressedKey == Keys.Right) cursorPosition++;
            CursorBoundsCheck();
        }

        static void CursorBoundsCheck()
        {
            if (cursorPosition < 0) cursorPosition = 0;
            if (cursorPosition > inputToWriteTo.Input.Length) cursorPosition = inputToWriteTo.Input.Length;
        }

        static bool IllegalCharacter(Keys aPressedKey) => inputToWriteTo.ValidInput(aPressedKey);

        static void UpdateStates()
        {
            oldKeyboardState = newKeyboardState;
            newKeyboardState = Keyboard.GetState();

            oldMouseState = newMouseState;
            newMouseState = Mouse.GetState();
            scrollWheelValue = newMouseState.ScrollWheelValue;
            scrollDelta = newMouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue;

        }

        static void UpdateScrollWheel()
        {
            if (oldMouseState.ScrollWheelValue == newMouseState.ScrollWheelValue) return;

            CreateScrollEvent();
        }

        static void CheckButtonPress()
        {
            if (GetMousePress(oldMouseState.LeftButton, newMouseState.LeftButton))
            {
                CreateClickEvent(InputManager.ClickType.Left);
            }
            else if (GetMouseRelease(oldMouseState.LeftButton, newMouseState.LeftButton))
            {
                CreateReleaseEvent(null, InputManager.ClickType.Left);
            }

            if (GetMousePress(oldMouseState.RightButton, newMouseState.RightButton))
            {
                CreateClickEvent(InputManager.ClickType.Right);
            }
            else if (GetMouseRelease(oldMouseState.RightButton, newMouseState.RightButton))
            {
                CreateReleaseEvent(null, InputManager.ClickType.Right);
            }

            if (GetMouseRelease(oldMouseState.MiddleButton, newMouseState.MiddleButton))
            {
                CreateReleaseEvent(null, InputManager.ClickType.Middle);
            }
        }

        static void CreateClickEvent(InputManager.ClickType aTypeOfClick)
        {
            bool[] heldModifiers = CheckHoldModifiers();

            inputToWriteTo = null;

            ClickEvent clickEvent = new ClickEvent(GetMousePosRelative(), aTypeOfClick, heldModifiers);

            Mailboxes.PublishUiEvent(clickEvent);
        }

        public static void CreateReleaseEvent(UIElement aCreator, InputManager.ClickType aTypeOfRelease)
        {
            bool[] heldModifiers = CheckHoldModifiers();

            ReleaseEvent releaseEvent = new ReleaseEvent(aCreator, GetMousePosRelative(), aTypeOfRelease, heldModifiers);
            Mailboxes.PublishUiEvent(releaseEvent);
        }

        static void CreateScrollEvent()
        {
            bool[] heldModifiers = CheckHoldModifiers();

            int amount = Math.Abs(oldMouseState.ScrollWheelValue - newMouseState.ScrollWheelValue) / 120;
            ScrollEvent.Direction direction = oldMouseState.ScrollWheelValue > newMouseState.ScrollWheelValue ? ScrollEvent.Direction.Up : ScrollEvent.Direction.Down;

            ScrollEvent scrollEvent = new ScrollEvent(GetMousePosRelative(), amount, direction, heldModifiers);

            Mailboxes.PublishUiEvent(scrollEvent);
        }

        static void PublishEscapePressed()
        {
            if (!oldKeyboardState.IsKeyDown(Keys.Escape) && newKeyboardState.IsKeyDown(Keys.Escape))
            {
                Mailboxes.PublishUiEvent(new EscapePressed());
            }
        }

        static void PublishKeyboardSnapshots()
        {
            Keys[] downKeys = newKeyboardState.GetPressedKeys();
            Mailboxes.PublishUiEvent(new KeyboardSnapshot(downKeys));

            int count = (int)KeyBindManager.KeyListner.Count;
            bool[] pressed = new bool[count];
            bool[] held = new bool[count];
            bool[] released = new bool[count];
            if (!UiTextInputManager.IsActive)
            {
                for (int i = 0; i < count; i++)
                {
                    KeyBindManager.KeyListner key = (KeyBindManager.KeyListner)i;
                    pressed[i] = KeyBindManager.GetPress(key);
                    held[i] = KeyBindManager.GetHold(key);
                    released[i] = KeyBindManager.GetRelease(key);
                }
            }

            Mailboxes.PublishUiEvent(new KeyBindSnapshot(pressed, held, released));
        }

        static void PublishMouseSnapshot()
        {
            AbsoluteScreenPosition absolute = GetMousePosAbsolute();
            RelativeScreenPosition relative = GetMousePosRelative();
            Mailboxes.PublishUiEvent(new MouseSnapshot(absolute, relative, scrollWheelValue, scrollDelta));
        }

        public static bool[] CheckHoldModifiers()
        {
            bool[] heldModifiers = new bool[(int)HoldModifier.Count];
            heldModifiers[(int)HoldModifier.Ctrl] = GetHold(Keys.LeftControl) || GetHold(Keys.RightControl);
            heldModifiers[(int)HoldModifier.Alt] = GetHold(Keys.LeftAlt) || GetHold(Keys.RightAlt);
            heldModifiers[(int)HoldModifier.Shift] = GetHold(Keys.LeftShift) || GetHold(Keys.RightShift);
            return heldModifiers;
        }

        public static AbsoluteScreenPosition GetMousePosAbsolute()
        {
            ThreadAffinity.AssertMainThread();
            AbsoluteScreenPosition mousePos = BoundsCheckOnMouse(new AbsoluteScreenPosition(newMouseState.Position));
            return mousePos; //TODO: Make this handle the mouse being outside screen
        }

        public static RelativeScreenPosition GetMousePosRelative()
        {
            ThreadAffinity.AssertMainThread();
            Point mousePoint = GetMousePosAbsolute();
            Point screenSize = Camera.Camera.WindowSize.ToPoint();

            RelativeScreenPosition mouseVector = new RelativeScreenPosition(mousePoint.X / (float)screenSize.X, mousePoint.Y / (float)screenSize.Y);

            //DebugManager.Print(Camera.Camera.ScreenRectangle.ToString());
            //DebugManager.Print(mousePoint.ToString());


            return mouseVector;
        }

        static AbsoluteScreenPosition BoundsCheckOnMouse(AbsoluteScreenPosition aMousePos)
        {
            Rectangle bounds = Camera.Camera.ScreenRectangle;
            int clampedX = Math.Clamp(aMousePos.X, bounds.Left, bounds.Right - 1);
            int clampedY = Math.Clamp(aMousePos.Y, bounds.Top, bounds.Bottom - 1);
            return new AbsoluteScreenPosition(clampedX, clampedY);
        }

        public static bool GetMousePress(ButtonState aOldMouseButton, ButtonState aNewMouseButton)
        {

            if (aOldMouseButton != ButtonState.Pressed && aNewMouseButton == ButtonState.Pressed)
            {
                return true;
            }
            return false;
        }

        public static bool GetMouseRelease(ButtonState aOldMouseButton, ButtonState aNewMouseButton)
        {
            if (aOldMouseButton == ButtonState.Pressed && aNewMouseButton != ButtonState.Pressed)
            {
                return true;
            }
            return false;
        }


        public static bool GetPress(Keys key)
        {
            ThreadAffinity.AssertMainThread();
            if (WritingToLabel) { return false; }

            if (!oldKeyboardState.IsKeyDown(key) && newKeyboardState.IsKeyDown(key))
            {
                return true;
            }

            return false;
        }

        public static bool GetHold(Keys key)
        {
            ThreadAffinity.AssertMainThread();
            if (WritingToLabel) { return false; }
            if (oldKeyboardState.IsKeyDown(key) || newKeyboardState.IsKeyDown(key))
            {
                return true;
            }

            return false;
        }

        public static bool GetRelease(Keys key)
        {
            ThreadAffinity.AssertMainThread();
            if (WritingToLabel) { return false; }
            if (oldKeyboardState.IsKeyDown(key) && !newKeyboardState.IsKeyDown(key))
            {
                return true;
            }

            return false;
        }
    }
}
