using Microsoft.Xna.Framework;
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
        public new Color Color { get => gfx.Color; set => gfx.Color = value; }

        public BarComponent(BarTexture aGfx, Vector2 aPos, Vector2 aSize) : base(aGfx, aPos, aSize)
        {

        }

        

        public void UpdateBar(float aNewValue, float aMaxX)
        {
            ((BarTexture)gfx).Filled = aNewValue;
            Vector2 v = RelativeSize;
            v.X = aMaxX * aNewValue;
            Resize(v);
        }
    }
}
