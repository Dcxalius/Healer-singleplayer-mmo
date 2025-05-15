using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.SelectBoxes
{
    internal abstract class SelectBoxValue : UIElement
    {
        public string DisplayText { get => text.Value; set => text.Value = value; }


        protected SelectBox selectBoxParent;
        //public 

        protected Text text;

        protected SelectBoxValue(UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize, string aStartText, SelectBox aParent) : base(aGfx, aPos, aSize)
        {
            selectBoxParent = aParent;
            text = new Text("Gloryse", aStartText, Color.Teal);
        }

        public override void Rescale()
        {
            base.Rescale();
            
            text.Rescale();
        }

        
        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();

            selectBoxParent.Close(this);
        }


        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);

            text.CentredDraw(aBatch, new AbsoluteScreenPosition(AbsolutePos.Center));
        }
    }
}
