using Microsoft.Xna.Framework;
using Project_1.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.SelectBoxes
{
    internal class SelectBoxValueInt : SelectBoxValue
    {
        //int value;

        public SelectBoxValueInt(string aStartValue, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(SelectBoxValueTypes.Int, null, aStartValue, aPos, aSize)
        {
        }
    }
}
