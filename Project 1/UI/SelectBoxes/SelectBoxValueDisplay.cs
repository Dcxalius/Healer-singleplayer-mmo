using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project_1.UI.SelectBoxes
{
    internal class SelectBoxValueDisplay : SelectBoxValue
    {
        public SelectBoxValueDisplay(SelectBoxValue a, UITexture aGfx) : base(a.Type, aGfx, a.DisplayText, aPos, aSize)
        {
        }
    }
}
