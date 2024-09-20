using Microsoft.Xna.Framework;
using Project_1.Textures;
using Project_1.UI.UIElements.Bars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.PlateBoxes
{
    internal class PlateBoxHealthSegment : PlateBoxSegment
    {
        Bar healthBar;

        public PlateBoxHealthSegment(Vector2 aPos, Vector2 aSize) : base(aGfx, aPos, aSize)
        {
            healthBar = new Bar(new BarTexture(), new UITexture(), aPos, aSize);
        }
    }
}
