using Project_1.Textures;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.Camera;

namespace Project_1.UI.UIElements.SelectBoxes
{
    internal class SelectBoxValueOption : SelectBoxValue
    {
        public SelectBoxValueOption(string aStartText, SelectBox aParent) : base(new UITexture("WhiteBackground", Color.Wheat), Camera.RelativeScreenPosition.Zero, Camera.RelativeScreenPosition.Zero, aStartText, aParent)
        {
        }

        public static T[] CreateArray<T>(SelectBox aParent) where T : SelectBoxValueOption, new()
        {
            T[] returnable = new T[(int)CameraSettings.Follow.Count];
            for (int i = 0; i < (int)CameraSettings.Follow.Count; i++)
            {
                //returnable[i] = new T(((CameraSettings.Follow)i), aParent);
                returnable[i] = new T();
                returnable[i].selectBoxParent = aParent;
                returnable[i].DisplayText = ((CameraSettings.Follow)i).ToString();
            }
            return returnable;
        }
    }
}
