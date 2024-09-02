using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Project_1.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Content.Input
{
    internal static class InputManager
    {
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
            oldKeyboardState = newKeyboardState;
            newKeyboardState = Keyboard.GetState();

            oldMouseState = newMouseState;
            newMouseState = Mouse.GetState();

            ScrolledSinceLastFrame = oldMouseState.ScrollWheelValue - newMouseState.ScrollWheelValue;

            if (GetMousePress(oldMouseState.LeftButton, newMouseState.LeftButton))
            {
                ClickEvent clickEvent = new ClickEvent(GetMousePosAbsolute(), ClickEvent.ClickType.Left);
                UIManager.Click(clickEvent);
            }
        }

        public static Point GetMousePosAbsolute()
        {
            return newMouseState.Position;
        }

        public static Vector2 GetMousePosRelative()
        {
            Point mousePoint = GetMousePosAbsolute();
            Point screenSize = Camera.devScreenBorder;

            Vector2 mouseVector = new Vector2(mousePoint.X / (float)screenSize.X, mousePoint.Y / (float)screenSize.Y);

            return mouseVector;
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
