using Microsoft.Xna.Framework;

namespace Project_1.Input
{
    internal class ClickEvent
    {
        public enum ClickType
        {
            Left,
            Middle,
            Right
        }

        public Point ClickPos { get => clickPos; }

        Point clickPos;

        public ClickType ButtonPressed { get => buttonPressed; }

        ClickType buttonPressed;


        public ClickEvent(Point aPos, ClickType aButton)
        {
            clickPos = aPos;
            buttonPressed = aButton;
        }
    }
}
