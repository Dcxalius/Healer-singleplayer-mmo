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

            SelectBoxValueCameraSettings[] setOfValues = SelectBoxValueCameraSettings.CreateArray(this);
            
            values = setOfValues;

            allValues.AddScrollableElements(values);

            displayValue = new SelectBoxValueDisplay(RelativeScreenPosition.Zero, RelativeScreenPosition.One, setOfValues[(int)Camera.Camera.CurrentCameraSetting], this);

            AddChild(displayValue);
        }

        protected override void ActionWhenSelected(int aSelectedValue)
        {
            base.ActionWhenSelected(aSelectedValue);
            CameraSettings.Follow oldSetting = Camera.Camera.CurrentCameraSetting;
            Camera.Camera.CurrentCameraSetting = ((SelectBoxValueCameraSettings)values[aSelectedValue]).CameraSetting;

            OptionManager.AddActionToDoAtExitOfOptionMenu(() => Camera.Camera.CurrentCameraSetting = oldSetting, () => Camera.Camera.ExportSettings());
        }

        public void SetValueFromOutside(int aSelectedValue) //TODO: Bring this up? Add checks or something??
        {
            base.ActionWhenSelected(aSelectedValue);
        }
    }
}
