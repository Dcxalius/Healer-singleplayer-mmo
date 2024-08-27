using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Content.Input
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
