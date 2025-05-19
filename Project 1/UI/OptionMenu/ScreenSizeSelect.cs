using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.UIElements.SelectBoxes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.OptionMenu
{
    internal class ScreenSizeSelect : SelectBox
    {
        static readonly string[] screenRezes = new string[] { "1500, 900", "1200, 900", "900, 1100", "100, 100", "5000, 5000", "1337, 420"}; //TODO: Make this pull this data from file
        public ScreenSizeSelect(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("WhiteBackground", Color.White), 0, aPos, aSize)
        {
            values  = SelectBoxValueOption.CreateArray(this, screenRezes);

            allValues.AddScrollableElements(values);

            Point p = Camera.Camera.WindowSizeAsPoint;
            string s = p.X + ", " + p.Y;

            displayValue = new SelectBoxValueDisplay(RelativeScreenPosition.Zero, RelativeScreenPosition.One, s, this); //TODO: Get actual value currently in use and not 0

            AddChild(displayValue);
            //displayValue = new SelectBoxValueDisplay(Pos, setOfValues[(int)Camera.CurrentCameraSetting], new UITexture("WhiteBackground", Color.White), aSize);
        }

        protected override void ActionWhenSelected(int aSelectedValue)
        {
            base.ActionWhenSelected(aSelectedValue);

            Point oldSize = Camera.Camera.WindowSizeAsPoint;            

            Point size;
            string[] split = values[aSelectedValue].DisplayText.Split(',');
            bool successW = int.TryParse(split[0], out size.X);
            bool successL = int.TryParse(split[1], out size.Y);

            Debug.Assert(successL && successW);
            //TODO: Check valid size
            Camera.Camera.WindowSizeAsPoint = size;

            OptionManager.AddActionToDoAtExitOfOptionMenu(() => Camera.Camera.WindowSizeAsPoint = oldSize, Camera.Camera.ExportSettings);
        }
    }
}
