using Project_1.Camera;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Bars
{
    internal class ExpBar : ResourceBar
    {
        static Color backgroundColor = new Color(255, 211, 211, 120);
        public ExpBar(UI.UIElements.UIElement aParent, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, new BarTexture(BarTexture.FillingDirection.Right, Color.MediumPurple), new UITexture("WhiteBackground", backgroundColor), aPos, aSize)
        {
            Value = 0;
            MaxValue = 1;
        }
    }
}
