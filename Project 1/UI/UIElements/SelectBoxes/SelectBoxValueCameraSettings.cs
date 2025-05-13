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
    internal class SelectBoxValueCameraSettings : SelectBoxValue
    {
        public CameraSettings.Follow CameraSetting { get => cameraSetting; }

        CameraSettings.Follow cameraSetting;
        public SelectBoxValueCameraSettings(CameraSettings.Follow aCameraSetting, RelativeScreenPosition aPos, RelativeScreenPosition aSize, SelectBox aParent) : base(SelectBoxValueTypes.CameraSetting, null, aCameraSetting.ToString(), aPos, aSize, aParent)
        {
            cameraSetting = aCameraSetting;
        }

        public static SelectBoxValueCameraSettings[] CreateArray(RelativeScreenPosition aStartPos, RelativeScreenPosition aSize, SelectBox aParent)
        {
            SelectBoxValueCameraSettings[] returnable = new SelectBoxValueCameraSettings[(int)CameraSettings.Follow.Count];
            RelativeScreenPosition pos = aStartPos;
            for (int i = 0; i < (int)CameraSettings.Follow.Count; i++)
            {
                returnable[i] = new SelectBoxValueCameraSettings(((CameraSettings.Follow)i), pos, aSize, aParent);
                pos.Y += aSize.Y;
            }
            return returnable;
        }

    }
}
