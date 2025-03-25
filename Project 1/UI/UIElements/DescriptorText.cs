using Microsoft.Xna.Framework;
using Project_1.Textures;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
{
    internal class DescriptorText : Text
    {
        
        float maxX;
        
        public int NameLines { get => nameLines; }
        int nameLines;

        public override string Value 
        {
            get => base.Value; 
            set
            {
                nameLines = 1;
                if (value == null)
                {
                    base.Value = value;
                    return;
                }
                if (font.MeasureString(value).X > maxX)
                {
                    string trim = value.Trim();
                    string[] split = trim.Split(' ');
                    if (split.Length !> 0) value = split[0];
                    string returnable = "";
                    for (int i = 0; i < split.Length; i++)
                    {
                        string partialString = split[i];

                        for (int j = i; j < split.Length; j++)
                        {
                            if (j == split.Length - 1)
                            {
                                returnable += partialString;
                                base.Value = returnable;
                                return;
                            }
                            if (font.MeasureString(partialString).X > maxX)
                            {
                                Debug.Assert(partialString != split[i], "First word was to long.");
                                returnable += partialString + "\n";
                                i = j;
                                nameLines++;
                                break;
                            }
                            partialString += " " + split[j + 1];
                        }

                    }

                    base.Value = returnable;
                    return;
                }

                base.Value = value;
            }
        }

        public DescriptorText(float aMaxX, string aFontName, Color aColor) : base(aFontName, aColor)
        {
            maxX = aMaxX;
            nameLines = 0;
        }
    }
}
