using Microsoft.Xna.Framework;
using System.Linq;
using Project_1.Input;
using Project_1.Camera;
using Project_1.Managers;
namespace Project_1.Input
{
    internal class ClickEvent : Modifiable
    {

        public RelativeScreenPosition RelativePos { get => clickPos; }
        public AbsoluteScreenPosition AbsolutePos { get => AbsoluteScreenPosition.FromRelativeScreenPosition(clickPos); }
        public InputManager.ClickType ButtonPressed { get => buttonPressed; }
        public bool[] ModifiersSnapshot => ToModifiersArray(modifierMask);

        RelativeScreenPosition clickPos;

        InputManager.ClickType buttonPressed;

        protected override byte ModifiersMask => modifierMask;
        byte modifierMask;


        public ClickEvent(AbsoluteScreenPosition aPos, InputManager.ClickType aButtonPressed, bool[] aModifiers) : this(aPos.ToRelativeScreenPosition(), aButtonPressed, aModifiers) { }
        public ClickEvent(AbsoluteScreenPosition aPos, InputManager.ClickType aButtonPressed, byte modifiersMask) : this(aPos.ToRelativeScreenPosition(), aButtonPressed, modifiersMask) { }

        public ClickEvent(RelativeScreenPosition aClickPos, InputManager.ClickType aButtonPressed, bool[] aModifiers)
            : this(aClickPos, aButtonPressed, BuildModifiersMask(aModifiers))
        {
        }

        public ClickEvent(RelativeScreenPosition aClickPos, InputManager.ClickType aButtonPressed, byte modifiersMask)
        {
            clickPos = aClickPos;
            //DebugManager.Print("Click: " + clickPos);

            buttonPressed = aButtonPressed;
            modifierMask = modifiersMask;
        }



    }
}
