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
        ScrollPlimp scrollPlimp;
        float RelativeSpacing => spacing * RelativeSize.Y;
        const float spacing = 0.0234375f; //(Length of the head part)/(Length of entire bar)
        
        public ScrollBar(Color aColor, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("ScrollBar", aColor), aPos, aSize)
        {
            scrollPlimp = new ScrollPlimp(aColor, RelativeScreenPosition.Zero, new RelativeScreenPosition(1, 1f / 32f)); //32 is from (graphical size of bar)/(graphical side of blimp)
            AddChild(scrollPlimp);
            SetValue(0);
            capturesClick = true;
        }

        public void SetValue(float aValue)
        {
            if (aValue < 0) aValue = 0;
            if (aValue > 1) aValue = 1;

            float total = 1f - RelativeSpacing - RelativeSpacing - scrollPlimp.RelativeSize.Y;
            scrollPlimp.SetPosOnBar(RelativeSpacing + total * aValue);
        }

        public void GetValueFromMouse()
        {
            float top = (InputManager.GetMousePosAbsolute().Y - AbsolutePos.Y - spacing * AbsolutePos.Size.Y);
            float bottom = AbsolutePos.Size.Y - (spacing * AbsolutePos.Size.Y) * 2;
            (parent as ScrollableBox).SetValue(top   / bottom);

        }

        protected override void ClickedOnMe(ClickEvent aClick)
        {
            base.ClickedOnMe(aClick);
            float top = (InputManager.GetMousePosAbsolute().Y - AbsolutePos.Y - spacing * AbsolutePos.Size.Y);
            float bottom = AbsolutePos.Size.Y - (spacing * AbsolutePos.Size.Y) * 2;
            (parent as ScrollableBox).SetValue(top / bottom);
        }
    }
}
