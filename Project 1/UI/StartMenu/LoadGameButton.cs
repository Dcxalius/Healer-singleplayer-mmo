using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.StartMenu
{
    internal class LoadGameButton : Button
    {
        public LoadGameButton(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aPos, aSize, Color.LightYellow, "Load", Color.Black)
        {
        }
    }
}
