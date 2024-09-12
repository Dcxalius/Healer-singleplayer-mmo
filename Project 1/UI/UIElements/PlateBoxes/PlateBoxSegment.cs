using Microsoft.Xna.Framework;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.PlateBoxes
{
    internal class PlateBoxSegment : UIElement
    {
        public PlateBoxSegment(ref Rectangle aParentPos, UITexture aGfx, Vector2 aPos, Vector2 aSize) : base(ref aParentPos, aGfx, aPos, aSize)
        {
        }
    }
}
