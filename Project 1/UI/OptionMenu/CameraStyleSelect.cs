using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.UIElements.SelectBoxes;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.OptionMenu
{
    internal class CameraStyleSelect : SelectBox
    {

        public CameraStyleSelect(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("WhiteBackground", Color.White), (int)Camera.Camera.CurrentCameraSetting, aPos, aSize)
        {
            values = SelectBoxValueOption.CreateArray(this, Enum.GetNames(typeof(CameraSettings.Follow)));

            allValues.AddScrollableElements(values);

            displayValue = new SelectBoxValueDisplay(RelativeScreenPosition.Zero, RelativeScreenPosition.One, values[(int)Camera.Camera.CurrentCameraSetting].DisplayText, this);

            AddChild(displayValue);
        }

        protected override void ActionWhenSelected(int aSelectedValue)
        {
            base.ActionWhenSelected(aSelectedValue);
            CameraSettings.Follow oldSetting = Camera.Camera.CurrentCameraSetting;
            Camera.Camera.CurrentCameraSetting = Enum.Parse<CameraSettings.Follow>(values[aSelectedValue].DisplayText);

            OptionManager.AddActionToDoAtExitOfOptionMenu(() => Camera.Camera.CurrentCameraSetting = oldSetting, () => Camera.Camera.ExportSettings());
        }

        public void SetValueFromOutside(int aSelectedValue) //TODO: Bring this up? Add checks or something??
        {
            base.ActionWhenSelected(aSelectedValue);
        }
    }
}
