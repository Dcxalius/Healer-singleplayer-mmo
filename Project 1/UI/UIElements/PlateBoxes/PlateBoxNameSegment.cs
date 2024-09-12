using Microsoft.Xna.Framework;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.PlateBoxes
{
    internal class PlateBoxNameSegment : PlateBoxSegment
    {
        string nameToDisplay;
        public PlateBoxNameSegment(ref Rectangle aParentPos, string name, Vector2 aPos, Vector2 aSize) : base(ref aParentPos, new UITexture("WhiteBackground", Color.AliceBlue), aPos, aSize)
        {
            nameToDisplay = name;
        }
    }
}
