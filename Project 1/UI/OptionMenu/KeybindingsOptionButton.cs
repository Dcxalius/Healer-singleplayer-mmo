using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.OptionMenu
{
    internal class KeybindingsOptionButton : Button
    {
        public KeybindingsOptionButton(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aPos, aSize, Color.White, "Keybindings", Color.Black)
        {

        }

        public override void HoldReleaseOnMe()
        {
            base.HoldReleaseOnMe();

            OptionManager.SetScreen(OptionManager.OptionScreen.Keybindings);
        }
    }

}
