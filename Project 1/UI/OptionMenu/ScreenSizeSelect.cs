using Microsoft.Xna.Framework;
using Project_1.Textures;
using Project_1.UI.UIElements.SelectBoxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.OptionMenu
{
    internal class ScreenSizeSelect : SelectBox
    {
        static readonly string[] screenRezes = new string[] { "1500, 900", "1200, 900", "900, 1100", "100, 100"}; //make this pull this data from file
        public ScreenSizeSelect(in Rectangle? aParentPos, Vector2 aPos, Vector2 aSize) : base(in aParentPos, new UITexture("WhiteBackground", Color.White), 0, aPos, aSize)
        {
            SelectBoxValueScreenRez[] setOfValues = SelectBoxValueScreenRez.CreateArray(Pos, screenRezes, new Vector2(0, aSize.Y), aSize);

            values = setOfValues;
            displayValue = new SelectBoxValueDisplay(Pos, setOfValues[(int)Camera.CurrentCameraSetting], new UITexture("WhiteBackground", Color.White), aSize);
        }

        protected override void ActionWhenSelected(int aSelectedValue)
        {
            base.ActionWhenSelected(aSelectedValue);

            Point p = ((SelectBoxValueScreenRez)values[aSelectedValue]).ScreenSize;

            GraphicsManager.SetWindowSize(p, false, false);
        }
    }
}
