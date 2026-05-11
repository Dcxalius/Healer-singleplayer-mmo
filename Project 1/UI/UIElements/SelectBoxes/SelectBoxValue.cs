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
        public string DisplayText
        {
            get => text.Value;
            set
            {
                if (text.Value == value) return;
                text.Value = value;
                MarkRenderStale();
            }
        }


        protected SelectBox selectBoxParent;
        //public 

        protected Text text;

        protected SelectBoxValue(UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize, string aStartText, SelectBox aSelectBoxParent, UIElement aParent) : base(aParent, aGfx, aPos, aSize)
        {
            selectBoxParent = aSelectBoxParent;
            text = new Text("Comfortaa-msdf", aStartText, Color.Teal);
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


        protected override void DrawSelf(SpriteBatch aBatch)
        {
            Project_1.Managers.ThreadAffinity.AssertMainThread();
            base.DrawSelf(aBatch);

            text.CentredDraw(aBatch, new AbsoluteScreenPosition(AbsolutePos.Center));
        }
    }
}
