using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.SelectBoxes
{
    internal class SelectBoxValueInt : SelectBoxValue
    {
        int value;

        public SelectBoxValueInt() : base(SelectBoxValueTypes.Int)
        {
        }
    }
}
