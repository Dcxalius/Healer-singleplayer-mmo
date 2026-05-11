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
    internal class BarComponent : UIElement
    {
        public new Color Color
        {
            get => gfx.Color;
            set
            {
                if (gfx.Color == value) return;
                gfx.Color = value;
                MarkRenderStale();
            }
        }

        public BarComponent(UIElement aParent, BarTexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, aGfx, aPos, aSize)
        {
            capturesClick = false;
        }

        

        public void UpdateBar(float aNewValue, float aMaxX)
        {
            if (RelativeSize.X == aNewValue) return;
            ((BarTexture)gfx).Filled = aNewValue;
            RelativeScreenPosition v = RelativeSize;
            v.X = aNewValue;
            Resize(v);
            MarkRenderStale();
        }
    }
}
