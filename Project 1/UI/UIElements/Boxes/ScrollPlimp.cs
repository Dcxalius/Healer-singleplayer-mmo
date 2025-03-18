using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Boxes
{
    internal class ScrollPlimp : UIElement
    {
        bool held;
        public ScrollPlimp(Color aColor, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("ScrollPlimp", aColor), aPos, aSize)
        {
            CapturesClick = true;
        }

        public void SetPosOnBar(float aY)
        {
            Move(new RelativeScreenPosition(RelativePos.X, aY));
        }

        public override void Update()
        {
            base.Update();
            if (!held) return;

            (parent as ScrollBar).GetValueFromMouse();
        }

        protected override void ClickedOnMe(ClickEvent aClick)
        {
            base.ClickedOnMe(aClick);
            held = true;

        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();
            held = false;
        }

        protected override void HoldReleaseAwayFromMe()
        {
            base.HoldReleaseAwayFromMe();
            held = false;
        }
    }
}
