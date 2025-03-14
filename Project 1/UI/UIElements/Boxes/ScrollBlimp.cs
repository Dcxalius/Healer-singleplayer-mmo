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
    internal class ScrollBlimp : UIElement
    {
        public ScrollBlimp(Color aColor, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("ScrollBlimp", aColor), aPos, aSize)
        {
        }
    }
}
