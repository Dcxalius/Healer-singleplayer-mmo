using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Managers;
using Project_1.Managers.States;
using Project_1.UI;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
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

        public static void Update()
        {
            UpdateStates();
            UpdateScrollWheel();
            CheckButtonPress();
        }

        static void UpdateStates()
        {
            oldKeyboardState = newKeyboardState;
            newKeyboardState = Keyboard.GetState();

            oldMouseState = newMouseState;
            newMouseState = Mouse.GetState();

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

            if (GetMousePress(oldMouseState.RightButton, newMouseState.RightButton))
            {
                CreateClickEvent(InputManager.ClickType.Right);
            }
        }

        static void CreateClickEvent(InputManager.ClickType aTypeOfClick)
        {
            bool[] heldModifiers = CheckHoldModifiers();

            ClickEvent clickEvent = new ClickEvent(GetMousePosRelative(), aTypeOfClick, heldModifiers);
                
            StateManager.Click(clickEvent);
        }

        public static void CreateReleaseEvent(UIElement aCreator, InputManager.ClickType aTypeOfRelease)
        {
            bool[] heldModifiers = CheckHoldModifiers();

            ReleaseEvent releaseEvent = new ReleaseEvent(aCreator, GetMousePosRelative(), aTypeOfRelease, heldModifiers);
            StateManager.Release(releaseEvent);
        }

        static void CreateScrollEvent()
        {
            bool[] heldModifiers = CheckHoldModifiers();

            int amount = Math.Abs(oldMouseState.ScrollWheelValue - newMouseState.ScrollWheelValue) / 120;
            ScrollEvent.Direction direction = oldMouseState.ScrollWheelValue > newMouseState.ScrollWheelValue ? ScrollEvent.Direction.Up : ScrollEvent.Direction.Down;

            ScrollEvent scrollEvent = new ScrollEvent(GetMousePosRelative(), amount, direction, heldModifiers);

            StateManager.Scroll(scrollEvent);
        }

        static bool[] CheckHoldModifiers()
        {
            bool[] heldModifiers = new bool[(int)HoldModifier.Count];
            heldModifiers[(int)HoldModifier.Shift] = GetHold(Keys.LeftShift) || GetHold(Keys.RightShift);
            heldModifiers[(int)HoldModifier.Alt] = GetHold(Keys.LeftAlt) || GetHold(Keys.RightAlt);
            heldModifiers[(int)HoldModifier.Ctrl] = GetHold(Keys.LeftControl) || GetHold(Keys.RightAlt);
            return heldModifiers;
        }

        public static AbsoluteScreenPosition GetMousePosAbsolute()
        {
            AbsoluteScreenPosition mousePos = BoundsCheckOnMouse(new AbsoluteScreenPosition(newMouseState.Position));
            return mousePos; //TODO: Make this handle the mouse being outside screen
        }

        public static RelativeScreenPosition GetMousePosRelative()
        {
            Point mousePoint = GetMousePosAbsolute();
            Point screenSize = Camera.Camera.ScreenSize.ToPoint();

            RelativeScreenPosition mouseVector = new RelativeScreenPosition(mousePoint.X / (float)screenSize.X, mousePoint.Y / (float)screenSize.Y);

            

            return mouseVector;
        }

        static AbsoluteScreenPosition BoundsCheckOnMouse(AbsoluteScreenPosition aMousePos)
        {
            Rectangle bounds = Camera.Camera.ScreenRectangle;
            if (!bounds.Contains(aMousePos.ToPoint()))
            {
                return new AbsoluteScreenPosition(-1);
            }
            return aMousePos;
        }

        public static bool GetMousePress(ButtonState aOldMouseButton, ButtonState aNewMouseButton)
        {

            if (aOldMouseButton != ButtonState.Pressed && aNewMouseButton == ButtonState.Pressed)
            {
                return true;
            }
            return false;
        }


        public static bool GetPress(Keys key)
        {
            if (!oldKeyboardState.IsKeyDown(key) && newKeyboardState.IsKeyDown(key))
            {
                return true;
            }

            return false;
        }

        public static bool GetHold(Keys key)
        {
            if (oldKeyboardState.IsKeyDown(key) || newKeyboardState.IsKeyDown(key))
            {
                return true;
            }

            return false;
        }

        public static bool GetRelease(Keys key)
        {
            if (oldKeyboardState.IsKeyDown(key) && !newKeyboardState.IsKeyDown(key))
            {
                return true;
            }

            return false;
        }
    }
}
