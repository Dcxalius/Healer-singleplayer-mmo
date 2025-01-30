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
        public BorderedBar(BarTexture aBarGfx, UITexture aBackgroundGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : this(Color.White, aBarGfx, aBackgroundGfx, aPos, aSize) { }
        public BorderedBar(Color aBorderColor, BarTexture aBarGfx, UITexture aBackgroundGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aBarGfx, aBackgroundGfx, aPos, aSize)
        {
            border = new Border(aBorderColor, RelativeScreenPosition.Zero, aSize);
            AddChild(border);
        }
    }
}
