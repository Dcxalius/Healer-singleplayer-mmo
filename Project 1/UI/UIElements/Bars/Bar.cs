using Microsoft.Xna.Framework;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Bars
{
    internal class Bar : Box
    {
        BarComponent barComponent;

        public Bar(BarTexture aBarGfx, UITexture aBackgroundGfx, Vector2 aPos, Vector2 aSize) : base(aBackgroundGfx, aPos, aSize)
        {
            barComponent = new BarComponent(aBarGfx, Vector2.Zero, new Vector2(aSize.X, aSize.Y));
        }
    }
}
