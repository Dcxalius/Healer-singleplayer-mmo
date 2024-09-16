using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project_1.UI.UIElements.SelectBoxes
{
    internal class SelectBoxValueDisplay : SelectBoxValue
    {
        public SelectBoxValueDisplay(SelectBoxValue aTypeToCopy, UITexture aGfx, Vector2 aSize) : base(aTypeToCopy.Type, aGfx, aTypeToCopy.DisplayText, Vector2.Zero, aSize)
        {

        }

        public void SetToNewValue(SelectBoxValue aValueToCopy)
        {
            textToDisplay = aValueToCopy.DisplayText;
            textSize = font.MeasureString(textToDisplay);
        }

        public override void Rescale()
        {
            base.Rescale();
            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}
