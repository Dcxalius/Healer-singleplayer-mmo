using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Guild
{
    internal class OpenCharacterWindow : GFXButton
    {
        public OpenCharacterWindow(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new GfxPath(GfxType.Item, "TestDagger"), aPos, aSize, Color.White)
        {

        }
    }
}
