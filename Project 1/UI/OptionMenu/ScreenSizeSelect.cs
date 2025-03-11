using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers;
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
        public ScreenSizeSelect(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("WhiteBackground", Color.White), 0, aPos, aSize)
        {
            SelectBoxValueScreenRez[] setOfValues = SelectBoxValueScreenRez.CreateArray(screenRezes, new RelativeScreenPosition(0, aSize.Y), aSize);

            values = setOfValues;
            displayValue = new SelectBoxValueDisplay(setOfValues[(int)0], null, aSize);
            AddChild(displayValue);
            AddChildren(values);
            for (int i = 0; i < values.Length; i++)
            {
                values[i].Visible = false;
            }
            
            //displayValue = new SelectBoxValueDisplay(Pos, setOfValues[(int)Camera.CurrentCameraSetting], new UITexture("WhiteBackground", Color.White), aSize);
        }

        protected override void ActionWhenSelected(int aSelectedValue)
        {
            base.ActionWhenSelected(aSelectedValue);

            Point p = ((SelectBoxValueScreenRez)values[aSelectedValue]).ScreenSize;

            GraphicsManager.SetWindowSize(p, false, false);
        }
    }
}
