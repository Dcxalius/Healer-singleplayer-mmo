using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.SelectBoxes
{
    internal class SelectBoxValueCameraSettings : SelectBoxValueOption
    {
        public CameraSettings.Follow CameraSetting { get => cameraSetting; }

        CameraSettings.Follow cameraSetting;
        public SelectBoxValueCameraSettings(CameraSettings.Follow aCameraSetting, SelectBox aParent) : base(aCameraSetting.ToString(), aParent)
        {
            cameraSetting = aCameraSetting;
        }

        public static SelectBoxValueCameraSettings[] CreateArray(SelectBox aParent)
        {
            SelectBoxValueCameraSettings[] returnable = new SelectBoxValueCameraSettings[(int)CameraSettings.Follow.Count];
            for (int i = 0; i < (int)CameraSettings.Follow.Count; i++)
            {
                returnable[i] = new SelectBoxValueCameraSettings(((CameraSettings.Follow)i), aParent);
            }
            return returnable;
        }

    }
}
