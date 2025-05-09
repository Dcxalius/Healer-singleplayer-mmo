using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.UI.OptionMenu.OptionManager;

namespace Project_1.UI.OptionMenu
{
    internal class KeybindingsList : ScrollableBox
    {
        public KeybindingsList(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(15, new UITexture("GrayBackground", Color.WhiteSmoke), Color.Turquoise, aPos, aSize)
        {
            for (int i = 0; i < (int)KeyBindManager.KeyListner.Count; i++)
            {
                AddScrollableElement(new KeybindingObject((KeyBindManager.KeyListner)i, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero));
            }
        }
    }
}
