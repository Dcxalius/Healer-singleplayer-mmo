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
        public BorderedBar(BarTexture aBarGfx, UITexture aBackgroundGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize, UIElement aParent = null) : this(Color.White, aBarGfx, aBackgroundGfx, aPos, aSize, aParent) { }
        public BorderedBar(Color aBorderColor, BarTexture aBarGfx, UITexture aBackgroundGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize, UIElement aParent = null) : base(aBarGfx, aBackgroundGfx, aPos, aSize, aParent)
        {
            border = new Border(aBorderColor, RelativeScreenPosition.Zero, aSize, this);
            AddChild(border);
        }
    }
}
