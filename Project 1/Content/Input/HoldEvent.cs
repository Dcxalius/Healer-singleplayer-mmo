using Project_1.Managers;
using Project_1.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Content.Input
{
    internal class HoldEvent
    {
        TimeSpan heldSince;
        public TimeSpan durationHeld;
        ClickEvent clickEventThatTriggered;
        UIElement creatorOfEvent;

        public HoldEvent(TimeSpan aStartTime, ClickEvent aClickEvent /*might be unnec*/, UIElement aOwner /*this one too xdd*/)
        {
            heldSince = aStartTime;
            clickEventThatTriggered = aClickEvent;
            creatorOfEvent = aOwner;
        }

        public bool IsStillHeld()
        {
            if (!InputManager.LeftHold && !InputManager.LeftPress) //Should be change to be mouse indipendant
            {
                durationHeld = TimeManager.gt.TotalGameTime - heldSince;

                return false;
            }

            return true;
        }
    }
}
