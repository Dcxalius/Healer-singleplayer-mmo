using Microsoft.Xna.Framework;
using System.Linq;
using Project_1.Input;
using Project_1.Camera;
namespace Project_1.Input
{
    internal class ClickEvent : Modifiable
    {

        public RelativeScreenPosition RelativePos { get => clickPos; }
        public AbsoluteScreenPosition AbsolutePos { get => AbsoluteScreenPosition.FromRelativeScreenPosition(clickPos); }
        public InputManager.ClickType ButtonPressed { get => buttonPressed; }

        RelativeScreenPosition clickPos;

        InputManager.ClickType buttonPressed;

        protected override bool[] ModifiersHeld => modifierHeld;
        bool[] modifierHeld;


        public ClickEvent(AbsoluteScreenPosition aPos, InputManager.ClickType aButtonPressed, bool[] aModifiers) : this(aPos.ToRelativeScreenPosition(), aButtonPressed, aModifiers) { }

        public ClickEvent(RelativeScreenPosition aClickPos, InputManager.ClickType aButtonPressed, bool[] aModifiers)
        {
            clickPos = aClickPos;

            buttonPressed = aButtonPressed;
            modifierHeld = aModifiers;
        }



    }
}
