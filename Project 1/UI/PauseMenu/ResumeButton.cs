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
    internal class ResumeButton : Button
    {
        public ResumeButton(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aPos, aSize, Color.PaleVioletRed, "Resume Game")
        {
            
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();

            StateManager.SetState(StateManager.States.Game);
        }
    }
}
