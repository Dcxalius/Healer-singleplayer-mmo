using Microsoft.Xna.Framework;
using System.Linq;
using Project_1.Input;
using Project_1.UI.UIElements;
using Project_1.Camera;
namespace Project_1.Input
{
    internal class ReleaseEvent : Modifiable
    {

        public RelativeScreenPosition RelativePos { get => releasePos; }
        public AbsoluteScreenPosition AbsolutePos { get => releasePos.ToAbsoluteScreenPos(); }
        public InputManager.ClickType ButtonPressed { get => buttonPressed; }
        public UIElement Creator { get => creator; }

        RelativeScreenPosition releasePos;

        InputManager.ClickType buttonPressed;
        protected override bool[] ModifiersHeld => modifierHeld;
        bool[] modifierHeld;

        UIElement creator;

        public ReleaseEvent(UIElement aCreator, AbsoluteScreenPosition aPos, InputManager.ClickType aButtonReleased, bool[] aModifiers) : this(aCreator, aPos.ToRelativeScreenPosition(), aButtonReleased, aModifiers) { }

        public ReleaseEvent(UIElement aCreator, RelativeScreenPosition aClickPos, InputManager.ClickType aButtonReleased, bool[] aModifiers)
        {
            releasePos = aClickPos;

            buttonPressed = aButtonReleased;
            modifierHeld = aModifiers;
            creator = aCreator;
        }
    }
}
