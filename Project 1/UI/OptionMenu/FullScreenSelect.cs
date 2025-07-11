using Microsoft.Xna.Framework;
using Project_1.Camera;
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
    internal class FullScreenSelect : SelectBox
    {
        public FullScreenSelect(RelativeScreenPosition aPos, RelativeScreenPosition aCollapsedSize) : base(new UITexture("WhiteBackground", Color.White), (int)Camera.Camera.FullScreen, aPos, aCollapsedSize)
        {
            values = SelectBoxValueOption.CreateArray(this, Enum.GetNames(typeof(CameraSettings.WindowType)));

            allValues.AddScrollableElements(values);

            displayValue = new SelectBoxValueDisplay(RelativeScreenPosition.Zero, RelativeScreenPosition.One, Camera.Camera.FullScreen.ToString(), this);

            AddChild(displayValue);
        }

        protected override void ActionWhenSelected(int aSelectedValue)
        {
            base.ActionWhenSelected(aSelectedValue);

            CameraSettings.WindowType oldFullscreen = Camera.Camera.FullScreen;

            Camera.Camera.FullScreen = Enum.Parse<CameraSettings.WindowType>(values[aSelectedValue].DisplayText);

            OptionManager.AddActionToDoAtExitOfOptionMenu(() => Camera.Camera.FullScreen = oldFullscreen, Camera.Camera.ExportSettings);
        }
    }
}
