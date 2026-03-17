using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;

namespace Project_1.Input
{
    internal static partial class InputManager
    {
        static void UpdateScrollWheel()
        {
            if (oldMouseState.ScrollWheelValue == newMouseState.ScrollWheelValue) return;
            CreateScrollEvent();
        }

        static void CheckButtonPress()
        {
            if (GetMousePress(oldMouseState.LeftButton, newMouseState.LeftButton))
            {
                CreateClickEvent(ClickType.Left);
            }
            else if (GetMouseRelease(oldMouseState.LeftButton, newMouseState.LeftButton))
            {
                CreateReleaseEvent(ClickType.Left);
            }

            if (GetMousePress(oldMouseState.RightButton, newMouseState.RightButton))
            {
                CreateClickEvent(ClickType.Right);
            }
            else if (GetMouseRelease(oldMouseState.RightButton, newMouseState.RightButton))
            {
                CreateReleaseEvent(ClickType.Right);
            }

            if (GetMouseRelease(oldMouseState.MiddleButton, newMouseState.MiddleButton))
            {
                CreateReleaseEvent(ClickType.Middle);
            }
        }

        static void CreateClickEvent(ClickType aTypeOfClick)
        {
            byte modifiersMask = GetHoldModifierMask();
            ClickEvent clickEvent = new ClickEvent(GetMousePosRelative(), aTypeOfClick, modifiersMask);
            Mailboxes.PublishUiEvent(clickEvent);
        }

        public static void CreateReleaseEvent(ClickType aTypeOfRelease)
        {
            byte modifiersMask = GetHoldModifierMask();
            ReleaseEvent releaseEvent = new ReleaseEvent(null, GetMousePosRelative(), aTypeOfRelease, modifiersMask);
            Mailboxes.PublishUiEvent(releaseEvent);
        }

        static void CreateScrollEvent()
        {
            byte modifiersMask = GetHoldModifierMask();
            int amount = System.Math.Abs(oldMouseState.ScrollWheelValue - newMouseState.ScrollWheelValue) / 120;
            ScrollEvent.Direction direction = oldMouseState.ScrollWheelValue > newMouseState.ScrollWheelValue
                ? ScrollEvent.Direction.Up
                : ScrollEvent.Direction.Down;

            ScrollEvent scrollEvent = new ScrollEvent(GetMousePosRelative(), amount, direction, modifiersMask);
            Mailboxes.PublishUiEvent(scrollEvent);
        }

        static void PublishMouseSnapshot()
        {
            AbsoluteScreenPosition absolute = GetMousePosAbsolute();
            RelativeScreenPosition relative = GetMousePosRelative();
            Mailboxes.PublishUiEvent(new MouseSnapshot(absolute, relative, scrollWheelValue, scrollDelta));
        }

        public static AbsoluteScreenPosition GetMousePosAbsolute()
        {
            ThreadAffinity.AssertMainThread();
            return BoundsCheckOnMouse(new AbsoluteScreenPosition(newMouseState.Position));
        }

        public static RelativeScreenPosition GetMousePosRelative()
        {
            ThreadAffinity.AssertMainThread();
            Point mousePoint = GetMousePosAbsolute();
            Point screenSize = Camera.Camera.WindowSize.ToPoint();
            return new RelativeScreenPosition(mousePoint.X / (float)screenSize.X, mousePoint.Y / (float)screenSize.Y);
        }

        static AbsoluteScreenPosition BoundsCheckOnMouse(AbsoluteScreenPosition aMousePos)
        {
            Rectangle bounds = Camera.Camera.ScreenRectangle;
            int clampedX = System.Math.Clamp(aMousePos.X, bounds.Left, bounds.Right - 1);
            int clampedY = System.Math.Clamp(aMousePos.Y, bounds.Top, bounds.Bottom - 1);
            return new AbsoluteScreenPosition(clampedX, clampedY);
        }

        public static bool GetMousePress(ButtonState aOldMouseButton, ButtonState aNewMouseButton)
        {
            return aOldMouseButton != ButtonState.Pressed && aNewMouseButton == ButtonState.Pressed;
        }

        public static bool GetMouseRelease(ButtonState aOldMouseButton, ButtonState aNewMouseButton)
        {
            return aOldMouseButton == ButtonState.Pressed && aNewMouseButton != ButtonState.Pressed;
        }
    }
}
