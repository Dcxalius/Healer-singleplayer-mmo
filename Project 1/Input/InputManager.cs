using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Project_1.GameObjects;
using Project_1.Managers;
using Project_1.UI;
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

        public static int ScrolledSinceLastFrame
        {
            get
            {
                return scrolledSinceLastFrame;
            }

            private set => scrolledSinceLastFrame = value;
        }

        static KeyboardState oldKeyboardState = Keyboard.GetState();
        static KeyboardState newKeyboardState = Keyboard.GetState();
        static MouseState newMouseState;
        static MouseState oldMouseState;

        static int scrolledSinceLastFrame;

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
            ScrolledSinceLastFrame = oldMouseState.ScrollWheelValue - newMouseState.ScrollWheelValue;

        }

        static void CheckButtonPress()
        {
            if (GetMousePress(oldMouseState.LeftButton, newMouseState.LeftButton))
            {
                CreateClickEvent(ClickEvent.ClickType.Left);
            }

            if (GetMousePress(oldMouseState.RightButton, newMouseState.RightButton))
            {
                CreateClickEvent(ClickEvent.ClickType.Right);
            }
        }

        static void CreateClickEvent(ClickEvent.ClickType aTypeOfClick)
        {
            bool[] heldModifiers = CheckHoldModifiers();

            ClickEvent clickEvent = new ClickEvent(GetMousePosRelative(), aTypeOfClick, heldModifiers);
            //DebugManager.Print(typeof(InputManager), "Mouse pos = " + GetMousePosRelative());
            bool hitAnUIElement = UIManager.Click(clickEvent);
            if (hitAnUIElement)
            {
                return;
            }
            ObjectManager.Click(clickEvent);
        }

        static bool[] CheckHoldModifiers()
        {
            bool[] heldModifiers = new bool[(int)HoldModifier.Count];
            heldModifiers[(int)HoldModifier.Shift] = GetHold(Keys.LeftShift) || GetHold(Keys.RightShift);
            heldModifiers[(int)HoldModifier.Alt] = GetHold(Keys.LeftAlt) || GetHold(Keys.RightAlt);
            heldModifiers[(int)HoldModifier.Ctrl] = GetHold(Keys.LeftControl) || GetHold(Keys.RightAlt);
            return heldModifiers;
        }

        public static Point GetMousePosAbsolute()
        {
            Point mousePos = BoundsCheckOnMouse(newMouseState.Position);
            return mousePos; //TODO: Make this handle the mouse being outside screen
        }

        public static Vector2 GetMousePosRelative()
        {
            Point mousePoint = GetMousePosAbsolute();
            Point screenSize = Camera.ScreenSize;

            Vector2 mouseVector = new Vector2(mousePoint.X / (float)screenSize.X, mousePoint.Y / (float)screenSize.Y);

            

            return mouseVector;
        }

        static Point BoundsCheckOnMouse(Point aMousePos)
        {
            Rectangle bounds = Camera.ScreenRectangle;
            if (!bounds.Contains(aMousePos))
            {
                return new Point(-1);
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
