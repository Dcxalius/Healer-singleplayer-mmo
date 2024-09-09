using Microsoft.Xna.Framework;
using Project_1.Textures;
using Project_1.UI.UIElements.SelectBoxes;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.OptionMenu
{
    internal class CameraStyleSelect : SelectBox
    {
        
        public CameraStyleSelect(Vector2 aPos, Vector2 aSize) : base(new UITexture("WhiteBackground", Color.White), SelectBoxValueCameraSettings.CreateArray(aPos + new Vector2(0, aSize.Y), aSize), (int)Camera.CurrentCameraSetting, aPos, aSize)
        {
        }

        protected override void ActionWhenSelected(int aSelectedValue)
        {
            base.ActionWhenSelected(aSelectedValue);

            Camera.SetCamera(((SelectBoxValueCameraSettings)values[aSelectedValue]).CameraSetting);
        }
    }
}
