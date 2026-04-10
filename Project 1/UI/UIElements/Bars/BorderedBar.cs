using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Bars
{
    internal class BorderedBar : Bar
    {
        Border border;
        public BorderedBar(UIElement aParent, BarTexture aBarGfx, UITexture aBackgroundGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : this(aParent, Color.White, aBarGfx, aBackgroundGfx, aPos, aSize) { }
        public BorderedBar(UIElement aParent, Color aBorderColor, BarTexture aBarGfx, UITexture aBackgroundGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, aBarGfx, aBackgroundGfx, aPos, aSize)
        {
            border = new Border(this, aBorderColor, RelativeScreenPosition.Zero, aSize);
        }
    }
}
