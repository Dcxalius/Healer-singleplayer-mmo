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
        public ExpBar(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new BarTexture(BarTexture.FillingDirection.Right, Color.MediumPurple), new UITexture("WhiteBackground", backgroundColor), aPos, aSize)
        {
        }
    }
}
