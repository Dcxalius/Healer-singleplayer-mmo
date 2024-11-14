using Microsoft.Xna.Framework;
using Project_1.Textures;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
{
    internal class DescriptorText : Text
    {
        float maxX;

        public override string Value 
        {
            get => base.Value; 
            set
            {
                if (value == null)
                {
                    base.Value = value;
                    return;
                }
                if ((font.MeasureString(value) * Camera.Zoom).X > maxX)
                {
                    //value.Substring(0, font.)
                    //TODO: Make this string find the last instance of ' ' and replace itself with a \n
                }
            }
        }

        public DescriptorText(string aFontName, Color aColor) : base(aFontName, aColor)
        {
        }
    }
}
