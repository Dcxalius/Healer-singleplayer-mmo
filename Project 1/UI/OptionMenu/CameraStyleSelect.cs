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
        public static CameraStyleSelect instance;

        public CameraStyleSelect(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("WhiteBackground", Color.White), (int)Camera.Camera.CurrentCameraSetting, aPos, aSize)
        {
            instance = this;

            SelectBoxValueCameraSettings[] setOfValues = SelectBoxValueCameraSettings.CreateArray(new RelativeScreenPosition(0, aSize.Y), aSize);
            displayValue = new SelectBoxValueDisplay(setOfValues[(int)Camera.Camera.CurrentCameraSetting], new UITexture("WhiteBackground", Color.White), aSize);

            values = setOfValues;

        }

        protected override void ActionWhenSelected(int aSelectedValue)
        {
            base.ActionWhenSelected(aSelectedValue);

            Camera.Camera.SetCamera(((SelectBoxValueCameraSettings)values[aSelectedValue]).CameraSetting);
        }

        public void SetValueFromOutside(int aSelectedValue) //TODO: Bring this up? Add checks or something??
        {
            base.ActionWhenSelected(aSelectedValue);
        }
    }
}
