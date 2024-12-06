using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
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
        public SelectBoxValueDisplay(SelectBoxValue aTypeToCopy, UITexture aGfx, RelativeScreenPosition aSize) : base(aTypeToCopy.Type, aGfx, aTypeToCopy.DisplayText, RelativeScreenPosition.Zero, aSize)
        {

        }

        public void SetToNewValue(SelectBoxValue aValueToCopy)
        {
            text.Value = aValueToCopy.DisplayText;
            
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
