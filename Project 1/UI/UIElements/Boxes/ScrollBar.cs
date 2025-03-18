using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Boxes
{
    internal class ScrollBar : UIElement
    {
        ScrollPlimp scrollBlimp;
        float RelativeSpacing => spacing * RelativeSize.Y;
        const float spacing = 0.0234375f; //(Length of the head part)/(Length of entire bar)
        
        public ScrollBar(Color aColor, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("ScrollBar", aColor), aPos, aSize)
        {
            scrollBlimp = new ScrollPlimp(aColor, RelativeScreenPosition.Zero, new RelativeScreenPosition(aSize.X, aSize.Y / 32)); //32 is from (graphical size of bar)/(graphical side of blimp)
            AddChild(scrollBlimp);
            SetValue(0);
            capturesClick = true;
        }

        public void SetValue(float aValue)
        {
            if (aValue < 0) aValue = 0;
            if (aValue > 1) aValue = 1;

            float total = RelativeSize.Y - RelativeSpacing - RelativeSpacing - scrollBlimp.RelativeSize.Y;
            scrollBlimp.SetPosOnBar(RelativeSpacing + total * aValue);
        }

        public void GetValueFromMouse()
        {
            (parent as ScrollableBox).SetValue((InputManager.GetMousePosAbsolute().Y - AbsolutePos.Y) / (float)AbsolutePos.Size.Y);

        }

        protected override void ClickedOnMe(ClickEvent aClick)
        {
            base.ClickedOnMe(aClick);
            (parent as ScrollableBox).SetValue((aClick.AbsolutePos.Y - AbsolutePos.Y) / (float)AbsolutePos.Size.Y );
        }
    }
}
