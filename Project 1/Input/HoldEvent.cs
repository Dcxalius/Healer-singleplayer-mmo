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
        
        public double DurationHeld => TimeManager.InstanceTotalFrameTime - heldSince;
        double durationHeld;
        public InputManager.ClickType ClickThatCreated { get => clickEventThatTriggered.ButtonPressed; }
        public Camera.RelativeScreenPosition Offset => offset;
        Camera.RelativeScreenPosition offset;

        ClickEvent clickEventThatTriggered;
        UIElement creatorOfEvent;


        public HoldEvent(ClickEvent aClickEvent, UIElement aOwner)
        {
            heldSince = TimeManager.InstanceTotalFrameTime;
            clickEventThatTriggered = aClickEvent;
            creatorOfEvent = aOwner;
            offset = clickEventThatTriggered.RelativePos - creatorOfEvent.RelativePositionOnScreen;
        }


        public bool IsStillHeld() => InputManager.IsMouseDown(ClickThatCreated);

    }
}
