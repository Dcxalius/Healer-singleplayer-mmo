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

        public SelectBoxValueInt(in Rectangle? aParentPos, string aStartValue, Vector2 aPos, Vector2 aSize) : base(in aParentPos, SelectBoxValueTypes.Int, null, aStartValue, aPos, aSize)
        {
        }
    }
}
