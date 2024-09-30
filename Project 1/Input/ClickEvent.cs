using Microsoft.Xna.Framework;
using System.Linq;
using Project_1.Input;
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

        public Point ClickPoint { get => Camera.TransformRelativeToAbsoluteScreenSpace(clickPos); }

        Vector2 clickPos;

        public ClickType ButtonPressed { get => buttonPressed; }

        ClickType buttonPressed;

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
        bool[] modifierHeld;


        public ClickEvent(Point aPos, ClickType aButtonPressed, bool[] aModifiers)
        {
            clickPos = Camera.TransformAbsoluteToRelativeScreenSpace(aPos);
            __ClickEvent__(aButtonPressed, aModifiers);

        }

        public ClickEvent(Vector2 aClickPos, ClickType aButtonPressed, bool[] aModifiers)
        {
            clickPos = aClickPos;
            __ClickEvent__(aButtonPressed, aModifiers);
        }
        
        void __ClickEvent__(ClickType aButtonPressed, bool[] aModifiers)
        {
            buttonPressed = aButtonPressed;
            modifierHeld = aModifiers;
        }
    }
}
