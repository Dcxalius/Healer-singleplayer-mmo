using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.SelectBoxes
{
    internal class SelectBoxValueInt : SelectBoxValue
    {
        int value;

        public SelectBoxValueInt(ref Rectangle aParentPos, string aStartValue, Vector2 aPos, Vector2 aSize) : base(ref aParentPos, SelectBoxValueTypes.Int, null, aStartValue, aPos, aSize)
        {
        }
    }
}
