using Microsoft.Xna.Framework;
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

        public CameraStyleSelect(ref Rectangle aParentPos, Vector2 aPos, Vector2 aSize) : base(ref aParentPos, new UITexture("WhiteBackground", Color.White), (int)Camera.CurrentCameraSetting, aPos, aSize)
        {
            instance = this;

            SelectBoxValueCameraSettings[] setOfValues = SelectBoxValueCameraSettings.CreateArray(ref Pos, new Vector2(0, aSize.Y), aSize);
            displayValue = new SelectBoxValueDisplay(ref Pos, setOfValues[(int)Camera.CurrentCameraSetting], new UITexture("WhiteBackground", Color.White), aSize);

            values = setOfValues;

        }

        protected override void ActionWhenSelected(int aSelectedValue)
        {
            base.ActionWhenSelected(aSelectedValue);

            Camera.SetCamera(((SelectBoxValueCameraSettings)values[aSelectedValue]).CameraSetting);
        }

        public void SetValueFromOutside(int aSelectedValue) //TODO: Bring this up? Add checks or something??
        {
            base.ActionWhenSelected(aSelectedValue);
        }
    }
}
