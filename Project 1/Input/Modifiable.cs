using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Input
{
    internal class Modifiable
    {
        protected virtual bool[] ModifiersHeld { get; }

        

        public bool Modifier(InputManager.HoldModifier aHoldModifier) { return ModifiersHeld[(int)aHoldModifier]; }
        public bool ModifiersOr(InputManager.HoldModifier[] aHoldModifier)
        {
            for (int i = 0; i < aHoldModifier.Length; i++)
            {
                if (ModifiersHeld[(int)aHoldModifier[i]] == true)
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
                if (ModifiersHeld[(int)aHoldModifier[i]] == false)
                {
                    return false;
                }
            }
            return true;

        }
        public bool NoModifiers()
        {
            for (int i = 0; i < ModifiersHeld.Length; i++)
            {
                if (ModifiersHeld[i] == true)
                {
                    return false;
                }

            }
            return true;
        }
    }
}
