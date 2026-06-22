using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project_1.UI.UIElements
{
    internal class ScrollablePopupMenu<T> : ScrollableBox<T>, IClipChild where T : UIElement
    {
        public enum Side
        {
            Top, Bottom, Left, Right
        }

        ScrollablePopupMenuItem<T> selected;

        public ScrollablePopupMenu(UIElement aParent, float visibleElements, UITexture aGfx, Color aBarColor, Side aSide, float aSideOffset, RelativeScreenPosition aSize) : base(null, visibleElements, aGfx, aBarColor, RelativeScreenPosition.Zero, aSize)
        {
            switch (aSide)
            {
                case Side.Top:
                    aParent.ClipTop(this, aSideOffset);
                    break;
                case Side.Bottom:
                    aParent.ClipBottom(this, aSideOffset);
                    break;
                case Side.Left:
                    aParent.ClipLeft(this, aSideOffset); 
                    break;
                case Side.Right:
                    aParent.ClipRight(this, aSideOffset);
                    break;
            }
        }


        internal interface ScrollablePopupMenuItem<T> where T : UIElement
        {
            ScrollablePopupMenu<T> Parent { get; }

            public virtual void SetToMe()
            {
                Parent.selected = (ScrollablePopupMenu<T>.ScrollablePopupMenuItem<T>)(this as ScrollablePopupMenuItem<T>);
            }
        }
    }
}
