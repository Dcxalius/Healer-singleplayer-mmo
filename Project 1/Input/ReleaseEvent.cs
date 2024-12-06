using Microsoft.Xna.Framework;
using System.Linq;
using Project_1.Input;
using Project_1.UI.UIElements;
using Project_1.Camera;
namespace Project_1.Input
{
    internal class ReleaseEvent
    {

        public RelativeScreenPosition RelativePos { get => releasePos; }
        public AbsoluteScreenPosition AbsolutePos { get => releasePos.ToAbsoluteScreenPos(); }
        public InputManager.ClickType ButtonPressed { get => buttonPressed; }
        public UIElement Parent { get => creator; }

        RelativeScreenPosition releasePos;

        InputManager.ClickType buttonPressed;

        bool[] modifierHeld;

        UIElement creator;

        public ReleaseEvent(UIElement aCreator, AbsoluteScreenPosition aPos, InputManager.ClickType aButtonReleased, bool[] aModifiers)
        {
            releasePos = aPos.ToRelativeScreenPosition();
            __ClickEvent__(aCreator, aButtonReleased, aModifiers);

        }

        public ReleaseEvent(UIElement aCreator, RelativeScreenPosition aClickPos, InputManager.ClickType aButtonReleased, bool[] aModifiers)
        {
            releasePos = aClickPos;
            __ClickEvent__(aCreator, aButtonReleased, aModifiers);
        }

        void __ClickEvent__(UIElement aCreator, InputManager.ClickType aButtonReleased, bool[] aModifiers)
        {
            buttonPressed = aButtonReleased;
            modifierHeld = aModifiers;
            creator = aCreator;
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
