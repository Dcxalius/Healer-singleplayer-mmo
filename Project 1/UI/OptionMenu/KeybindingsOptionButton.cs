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
        public KeybindingsOptionButton(RelativeScreenPosition aPos, RelativeScreenPosition aSize, UI.UIElements.UIElement aParent = null) : base(aPos, aSize, Color.White, "Keybindings", Color.Black, aParent)
        {

        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();

            OptionManager.SetScreen(OptionManager.OptionScreen.Keybindings);
        }
    }

}
