using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Boxes
{
    internal class ButtonBox : Box
    {
        protected Button[] buttons;

        public ButtonBox(Button[] aButtons, UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aGfx, aPos, aSize)
        {
            buttons = aButtons;
            AddChildren(buttons);
        }
    }
}
