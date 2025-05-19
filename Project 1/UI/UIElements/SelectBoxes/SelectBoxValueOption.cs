using Project_1.Textures;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.Camera;

namespace Project_1.UI.UIElements.SelectBoxes
{
    internal class SelectBoxValueOption : SelectBoxValue
    {
        public SelectBoxValueOption(SelectBox aParent, string aString) : base(new UITexture("WhiteBackground", Color.Wheat), RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, aString, aParent)
        {
        }

        public static SelectBoxValueOption[] CreateArray(SelectBox aParent, string[] aDisplayValue)
        {
            SelectBoxValueOption[] returnable = new SelectBoxValueOption[aDisplayValue.Count()];
            for (int i = 0; i < aDisplayValue.Count(); i++)
            {
                //returnable[i] = new T(((CameraSettings.Follow)i), aParent);
                returnable[i] = new SelectBoxValueOption(aParent, aDisplayValue[i]);
                returnable[i].selectBoxParent = aParent;
                returnable[i].DisplayText = aDisplayValue[i];
            }
            return returnable;
        }
    }
}
