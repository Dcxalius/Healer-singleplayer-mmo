using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Spawners.Pathing;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.UI;
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
        const int KeyBindMaskBitCount = sizeof(ulong) * 8;

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
            PublishKeyboardSnapshots();
            PublishMouseSnapshot();
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
                CreateReleaseEvent(InputManager.ClickType.Left);
            }

            if (GetMousePress(oldMouseState.RightButton, newMouseState.RightButton))
            {
                CreateClickEvent(InputManager.ClickType.Right);
            }
            else if (GetMouseRelease(oldMouseState.RightButton, newMouseState.RightButton))
            {
                CreateReleaseEvent(InputManager.ClickType.Right);
            }

            if (GetMouseRelease(oldMouseState.MiddleButton, newMouseState.MiddleButton))
            {
                CreateReleaseEvent(InputManager.ClickType.Middle);
            }
        }

        static void CreateClickEvent(InputManager.ClickType aTypeOfClick)
        {
            byte modifiersMask = GetHoldModifierMask();
            ClickEvent clickEvent = new ClickEvent(GetMousePosRelative(), aTypeOfClick, modifiersMask);

            Mailboxes.PublishUiEvent(clickEvent);
        }

        public static void CreateReleaseEvent(InputManager.ClickType aTypeOfRelease)
        {
            byte modifiersMask = GetHoldModifierMask();

            ReleaseEvent releaseEvent = new ReleaseEvent(null, GetMousePosRelative(), aTypeOfRelease, modifiersMask);
            Mailboxes.PublishUiEvent(releaseEvent);
        }

        static void CreateScrollEvent()
        {
            byte modifiersMask = GetHoldModifierMask();

            int amount = Math.Abs(oldMouseState.ScrollWheelValue - newMouseState.ScrollWheelValue) / 120;
            ScrollEvent.Direction direction = oldMouseState.ScrollWheelValue > newMouseState.ScrollWheelValue ? ScrollEvent.Direction.Up : ScrollEvent.Direction.Down;

            ScrollEvent scrollEvent = new ScrollEvent(GetMousePosRelative(), amount, direction, modifiersMask);

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
            Debug.Assert(count <= KeyBindMaskBitCount, $"KeyBindSnapshot currently supports up to {KeyBindMaskBitCount} key listeners.");
            ulong pressedMask = 0;
            ulong heldMask = 0;
            ulong releasedMask = 0;
            if (!UiTextInputManager.IsActive)
            {
                int max = Math.Min(count, KeyBindMaskBitCount);
                for (int i = 0; i < max; i++)
                {
                    KeyBindManager.KeyListner key = (KeyBindManager.KeyListner)i;
                    ulong bit = 1UL << i;
                    if (KeyBindManager.GetPress(key)) pressedMask |= bit;
                    if (KeyBindManager.GetHold(key)) heldMask |= bit;
                    if (KeyBindManager.GetRelease(key)) releasedMask |= bit;
                }
            }

            Mailboxes.PublishUiEvent(new KeyBindSnapshot(pressedMask, heldMask, releasedMask));
        }

        static void PublishMouseSnapshot()
        {
            AbsoluteScreenPosition absolute = GetMousePosAbsolute();
            RelativeScreenPosition relative = GetMousePosRelative();
            Mailboxes.PublishUiEvent(new MouseSnapshot(absolute, relative, scrollWheelValue, scrollDelta));
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
            if (UiTextInputManager.IsActive) { return false; }

            if (!oldKeyboardState.IsKeyDown(key) && newKeyboardState.IsKeyDown(key))
            {
                return true;
            }

            return false;
        }

        public static bool GetHold(Keys key)
        {
            ThreadAffinity.AssertMainThread();
            if (UiTextInputManager.IsActive) { return false; }
            if (oldKeyboardState.IsKeyDown(key) || newKeyboardState.IsKeyDown(key))
            {
                return true;
            }

            return false;
        }

        public static bool GetRelease(Keys key)
        {
            ThreadAffinity.AssertMainThread();
            if (UiTextInputManager.IsActive) { return false; }
            if (oldKeyboardState.IsKeyDown(key) && !newKeyboardState.IsKeyDown(key))
            {
                return true;
            }

            return false;
        }
    }
}
