using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.UIElements.SelectBoxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.OptionMenu
{
    internal class FullScreenSelect : SelectBox
    {
        public FullScreenSelect(UITexture aGfx, int aStartDisplayValue, RelativeScreenPosition aPos, RelativeScreenPosition aCollapsedSize) : base(aGfx, (int)Camera.Camera.FullScreen, aPos, aCollapsedSize)
        {
            SelectBoxValueStrings[] setOfValues = SelectBoxValueStrings.CreateArray(screenRezes, this);

            values = setOfValues;

            allValues.AddScrollableElements(values);

            displayValue = new SelectBoxValueDisplay(RelativeScreenPosition.Zero, RelativeScreenPosition.One, setOfValues[0], this);

            AddChild(displayValue);
        }
    }
}
