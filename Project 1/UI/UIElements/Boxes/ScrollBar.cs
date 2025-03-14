using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Boxes
{
    internal class ScrollBar : UIElement
    {
        ScrollBlimp scrollBlimp;
        public ScrollBar(Color aColor, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("ScrollBar", aColor), aPos, aSize)
        {
            scrollBlimp = new ScrollBlimp(aColor, RelativeScreenPosition.Zero, aSize / 32); //32 is from (graphical size of bar)/(graphical side of blimp)
        }
    }
}
