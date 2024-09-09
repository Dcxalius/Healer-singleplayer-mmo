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
        public ScreenSizeSelect(Vector2 aPos, Vector2 aSize) : base(new UITexture("WhiteBackground", Color.White), SelectBoxValueScreenRez.CreateArray(screenRezes, aPos + new Vector2(0, aSize.Y), aSize), 0, aPos, aSize)
        {

        }

        protected override void ActionWhenSelected(int aSelectedValue)
        {
            base.ActionWhenSelected(aSelectedValue);

            Point p = ((SelectBoxValueScreenRez)values[aSelectedValue]).ScreenSize;

            GraphicsManager.SetWindowSize(p, false, false);
        }
    }
}
