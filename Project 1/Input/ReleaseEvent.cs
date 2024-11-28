using Microsoft.Xna.Framework;
using System.Linq;
using Project_1.Input;
using Project_1.UI.UIElements;
namespace Project_1.Input
{
    internal class ReleaseEvent
    {

        public Vector2 RelativePos { get => releasePos; }
        public Point AbsolutePos { get => Camera.Camera.TransformRelativeToAbsoluteScreenSpace(releasePos); }
        public InputManager.ClickType ButtonPressed { get => buttonPressed; }
        public UIElement Parent { get => creator; }

        Vector2 releasePos;

        InputManager.ClickType buttonPressed;

        bool[] modifierHeld;

        UIElement creator;

        public ReleaseEvent(UIElement aCreator, Point aPos, InputManager.ClickType aButtonReleased, bool[] aModifiers)
        {
            releasePos = Camera.Camera.TransformAbsoluteToRelativeScreenSpace(aPos);
            __ClickEvent__(aCreator, aButtonReleased, aModifiers);

        }

        public ReleaseEvent(UIElement aCreator, Vector2 aClickPos, InputManager.ClickType aButtonReleased, bool[] aModifiers)
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
