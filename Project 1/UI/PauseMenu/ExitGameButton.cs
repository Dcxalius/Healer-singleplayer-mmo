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
        public ExitGameButton(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aPos, aSize, Color.Tan)
        {
            ButtonText = "Exit Game";
        }

        public override void HoldReleaseOnMe()
        {
            Game1.Instance.Exit();

            base.HoldReleaseOnMe();
        }
    }
}
