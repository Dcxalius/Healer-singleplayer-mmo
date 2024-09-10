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

        public Vector2 ClickPos { get => clickPos; }

        Vector2 clickPos;

        public ClickType ButtonPressed { get => buttonPressed; }

        ClickType buttonPressed;


        public ClickEvent(Point aPos, ClickType aButton)
        {
            clickPos = Camera.TransformAbsoluteToRelativeScreenSpace(aPos);
            buttonPressed = aButton;
        }

        public ClickEvent(Vector2 aClickPos, ClickType aButtonPressed)
        {
            clickPos = aClickPos;
            buttonPressed = aButtonPressed;
        }
    }
}
