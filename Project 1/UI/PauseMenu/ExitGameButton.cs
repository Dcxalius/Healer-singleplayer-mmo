using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.PauseMenu
{
    internal class ExitGameButton : Button
    {
        public ExitGameButton(RelativeScreenPosition aPos, RelativeScreenPosition aSize, UI.UIElements.UIElement aParent = null) : base(aPos, aSize, Color.Tan, aParent: aParent) //TODO: Modernize
        {
            ButtonText = "Exit Game";
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            Game1.Instance.Exit();

            base.ClickedOnAndReleasedOnMe();
        }
    }
}
