using Microsoft.Xna.Framework;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.SelectBoxes
{
    internal class SelectBoxValueCameraSettings : SelectBoxValue
    {
        public Camera.CameraSettings CameraSetting { get => cameraSetting; }

        Camera.CameraSettings cameraSetting;
        public SelectBoxValueCameraSettings(ref Rectangle aParentPos, Camera.CameraSettings aCameraSetting, Vector2 aPos, Vector2 aSize) : base(ref aParentPos, SelectBoxValueTypes.CameraSetting, null, aCameraSetting.ToString(), aPos, aSize)
        {
            cameraSetting = aCameraSetting;
        }

        public static SelectBoxValueCameraSettings[] CreateArray(ref Rectangle aParentPos, Vector2 aStartPos, Vector2 aSize)
        {
            SelectBoxValueCameraSettings[] returnable = new SelectBoxValueCameraSettings[(int)Camera.CameraSettings.Count];
            Vector2 pos = aStartPos;
            for (int i = 0; i < (int)Camera.CameraSettings.Count; i++)
            {
                returnable[i] = new SelectBoxValueCameraSettings(ref aParentPos, ((Camera.CameraSettings)i), pos, aSize);
                pos.Y += aSize.Y;
            }
            return returnable;
        }

    }
}
