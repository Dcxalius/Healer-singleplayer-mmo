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
        protected override byte ModifiersMask => modifierMask;
        byte modifierMask;

        UIElement creator;

        public ReleaseEvent(UIElement aCreator, AbsoluteScreenPosition aPos, InputManager.ClickType aButtonReleased, bool[] aModifiers) : this(aCreator, aPos.ToRelativeScreenPosition(), aButtonReleased, aModifiers) { }
        public ReleaseEvent(UIElement aCreator, AbsoluteScreenPosition aPos, InputManager.ClickType aButtonReleased, byte modifiersMask) : this(aCreator, aPos.ToRelativeScreenPosition(), aButtonReleased, modifiersMask) { }

        public ReleaseEvent(UIElement aCreator, RelativeScreenPosition aClickPos, InputManager.ClickType aButtonReleased, bool[] aModifiers)
            : this(aCreator, aClickPos, aButtonReleased, BuildModifiersMask(aModifiers))
        {
        }

        public ReleaseEvent(UIElement aCreator, RelativeScreenPosition aClickPos, InputManager.ClickType aButtonReleased, byte modifiersMask)
        {
            releasePos = aClickPos;

            buttonPressed = aButtonReleased;
            modifierMask = modifiersMask;
            creator = aCreator;
        }
    }
}
