using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.UI.OptionMenu;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.SelectBoxes
{
    internal class SelectBoxValueScreenRez : SelectBoxValue
    {
        public Point ScreenSize { get => screenSize; }

        Point screenSize;

        SelectBoxValueScreenRez(string aRez, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(SelectBoxValueTypes.ScreenRez, null, aRez, aPos, aSize)
        {
            string[] split = aRez.Split(',');

            Debug.Assert(split.Length == 2);
            bool successW;
            bool successL;
            successW = int.TryParse(split[0], out screenSize.X);
            successL = int.TryParse(split[1], out screenSize.Y);

            Debug.Assert(successL && successW);
        }

        public static SelectBoxValueScreenRez[] CreateArray(string[] aListToCreate, RelativeScreenPosition aStartPos, RelativeScreenPosition aSize)
        {
            SelectBoxValueScreenRez[] returnable = new SelectBoxValueScreenRez[aListToCreate.Length];
            RelativeScreenPosition pos = aStartPos;
            for (int i = 0; i < aListToCreate.Length; i++)
            {
                returnable[i] = new SelectBoxValueScreenRez(aListToCreate[i], pos, aSize);
                pos.Y += aSize.Y;
            }
            return returnable;
        }

        public override string ToString()
        {
            return "[Width: " + screenSize.X + "Height: " + screenSize.Y + "]";
        }
    }


}
