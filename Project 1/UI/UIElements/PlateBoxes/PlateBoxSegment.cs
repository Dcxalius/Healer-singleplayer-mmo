using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.PlateBoxes
{
    internal abstract class PlateBoxSegment : UIElement
    {
        protected string Text { get => text.Value; set => text.Value = value; }

        Text text;

        public PlateBoxSegment(UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aGfx, aPos, aSize)
        {
            text = new Text("Gloryse", Color.Black);
            
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);

            text.CentredDraw(aBatch, AbsolutePos.Center.ToVector2());
        }
    }
}
