using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
{
    internal class Border : Box
    {
        public Border(UIElement aParent, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : this(aParent, Color.White, aPos, aSize) { }

        public Border(UIElement aParent, Color aBorderColor, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, new UITexture("GrayWhiteBorder", aBorderColor), aPos, aSize)
        {
        }

        public override bool ClickedOn(ClickEvent aClick)
        {
            return false;
        }

        public override bool ReleasedOn(ReleaseEvent aRelease)
        {
            return false;
        }

        internal override bool ScrolledOn(ScrollEvent aScrollEvent)
        {
            return false;
        }
    }
}
