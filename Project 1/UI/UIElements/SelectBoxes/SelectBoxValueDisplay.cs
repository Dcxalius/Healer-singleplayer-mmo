using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.SelectBoxes
{
    internal class SelectBoxValueDisplay : SelectBoxValue
    {
        public SelectBoxValueDisplay(SelectBox aParent, RelativeScreenPosition aPos, RelativeScreenPosition aSize, string aStartValue) : base(new UITexture("WhiteBackground", Color.WhiteSmoke), aPos, aSize, aStartValue, aParent, aParent)
        {
            capturesClick = false;
        }

        public void SetToNewValue(SelectBoxValue aValueToCopy)
        {
            text.Value = aValueToCopy.DisplayText;
            
        }

        public void SetToNewText(string aText)
        {
            text.Value = aText;
        }

        public override void Rescale()
        {
            base.Rescale();
            
        }

        public override void ClickedOnAndReleasedOnMe()
        {
        }

    }
}
