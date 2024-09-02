using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.SelectBoxes
{
    internal class SelectBoxValueScreenRez : SelectBoxValue
    {
        int width;
        int height;

        SelectBoxValueScreenRez(string aRez) : base(SelectBoxValueTypes.ScreenRez)
        {
            string[] split = aRez.Split(',');
            
            Debug.Assert(split.Length == 2);
            bool successW;
            bool successL;
            successW = Int32.TryParse(split[0], out width);
            successL = Int32.TryParse(split[1], out height);

            Debug.Assert(successL && successW);
        }
        
        public static SelectBoxValueScreenRez[] CreateArray(string[] aListToCreate)
        {
            SelectBoxValueScreenRez[] returnable = new SelectBoxValueScreenRez[aListToCreate.Length];
            for (int i = 0; i < aListToCreate.Length; i++)
            {
                returnable[i] = new SelectBoxValueScreenRez(aListToCreate[i]);
            }
            return returnable;
        }

        public override string ToString()
        {
            return "[Width: " + width + "Height: " + height + "]";
        }
    }

    
}
