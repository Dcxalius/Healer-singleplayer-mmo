using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers.States;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.PauseMenu
{
    internal class OptionMenuButton : Button
    {
        public OptionMenuButton(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aPos, aSize, Color.Blue, "Options Menu")
        {

        }

        public override void HoldReleaseOnMe()
        {
            StateManager.SetState(StateManager.States.OptionMenu);

            base.HoldReleaseOnMe();
        }
    }
}
