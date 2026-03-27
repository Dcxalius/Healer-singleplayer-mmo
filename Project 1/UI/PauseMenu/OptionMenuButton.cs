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
        public OptionMenuButton(RelativeScreenPosition aPos, RelativeScreenPosition aSize, UI.UIElements.UIElement aParent = null) : base(aPos, aSize, Color.Blue, "Options Menu", aParent: aParent)
        {

        }

        public override void ClickedOnAndReleasedOnMe()
        {
            StateManager.RequestStateChange(StateManager.States.OptionMenu);

            base.ClickedOnAndReleasedOnMe();
        }
    }
}
