using Project_1.Managers;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Input
{
    internal class HoldEvent
    {
        double heldSince;
        public double durationHeld;
        public InputManager.ClickType ClickThatCreated { get => clickEventThatTriggered.ButtonPressed; }
        ClickEvent clickEventThatTriggered;
        UIElement creatorOfEvent;


        public HoldEvent(double aStartTime, ClickEvent aClickEvent, UIElement aOwner /*might be unnec*/)
        {
            heldSince = aStartTime;
            clickEventThatTriggered = aClickEvent;
            creatorOfEvent = aOwner;
        }


        public bool IsStillHeld()
        {
            if (LeftHeld() || RightHeld()) //Should be change to be mouse indipendant
            {
                durationHeld = TimeManager.TotalFrameTime - heldSince;

                return false;
            }

            return true;
        }

        bool LeftHeld()
        {
            if (clickEventThatTriggered.ButtonPressed != InputManager.ClickType.Left)
            {
                return false;
            }

            return !InputManager.LeftHold && !InputManager.LeftPress;
        }

        bool RightHeld() 
        {
            if (clickEventThatTriggered.ButtonPressed != InputManager.ClickType.Right)
            {
                return false;
            }

            return !InputManager.LeftHold && !InputManager.LeftPress;
        }
    }
}
