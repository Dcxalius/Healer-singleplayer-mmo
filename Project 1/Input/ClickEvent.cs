using Microsoft.Xna.Framework;
using System.Linq;
using Project_1.Input;
using Project_1.Camera;
namespace Project_1.Input
{
    internal class ClickEvent
    {

        public RelativeScreenPosition RelativePos { get => clickPos; }
        public AbsoluteScreenPosition AbsolutePos { get => AbsoluteScreenPosition.FromRelativeScreenPosition(clickPos); }
        public InputManager.ClickType ButtonPressed { get => buttonPressed; }

        RelativeScreenPosition clickPos;

        InputManager.ClickType buttonPressed;

        bool[] modifierHeld;


        public ClickEvent(AbsoluteScreenPosition aPos, InputManager.ClickType aButtonPressed, bool[] aModifiers)
        {
            clickPos = aPos.ToRelativeScreenPosition();
            __ClickEvent__(aButtonPressed, aModifiers);

        }

        public ClickEvent(RelativeScreenPosition aClickPos, InputManager.ClickType aButtonPressed, bool[] aModifiers)
        {
            clickPos = aClickPos;
            __ClickEvent__(aButtonPressed, aModifiers);
        }

        void __ClickEvent__(InputManager.ClickType aButtonPressed, bool[] aModifiers)
        {
            buttonPressed = aButtonPressed;
            modifierHeld = aModifiers;
        }


        public bool Modifier(InputManager.HoldModifier aHoldModifier) { return modifierHeld[(int)aHoldModifier]; }
        public bool ModifiersOr(InputManager.HoldModifier[] aHoldModifier)
        {
            for (int i = 0; i < aHoldModifier.Length; i++)
            {
                if (modifierHeld[(int)aHoldModifier[i]] == true)
                {
                    return true;
                }
            }
            return false;
        }
        public bool Modifiers(InputManager.HoldModifier[] aHoldModifier)
        {
            for (int i = 0; i < aHoldModifier.Length; i++)
            {
                if (modifierHeld[(int)aHoldModifier[i]] == false)
                {
                    return false;
                }
            }
            return true;

        }
        public bool NoModifiers()
        {
            for (int i = 0; i < modifierHeld.Length; i++)
            {
                if (modifierHeld[i] == true)
                {
                    return false;
                }

            }
            return true;
        }
    }
}
