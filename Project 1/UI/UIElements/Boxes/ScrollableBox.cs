using Project_1.Camera;
using Project_1.Input;
using Project_1.Textures;
using Project_1.UI.UIElements.Inventory;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Boxes
{
    internal abstract class ScrollableBox : Box
    {
        ScrollBar scrollBar;
        public ScrollableBox(UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aGfx, aPos, aSize)
        {
            //scrollBar = new ScrollBar(); //TODO: Finish implementation, this should not be added to children so override to move, draw etc needs to be done.
            capturesScroll = true;
            originalYPos = new List<float>();
        }
        protected abstract RelativeScreenPosition Spacing { get; }
        protected bool ToMuchForWindow => originalYPos.Last() + GetChild(ChildCount - 1).RelativeSize.Y + Spacing.Y > RelativeSize.Y; 
        float scrollValue;
        List<float> originalYPos;

        protected abstract float ScrollSpeed { get; }

        protected void Scrolled(ScrollEvent aScrollEvent)
        {
            if (!ToMuchForWindow) return;

            scrollValue += aScrollEvent.DirectionAndSteps * ScrollSpeed;

            CapScroll();
            UpdateLootPosition();


        }

        protected override void KillAllChildren()
        {
            base.KillAllChildren();
            originalYPos.Clear();
        }

        protected override void KillChild(int aIndex)
        {
            base.KillChild(aIndex);
            originalYPos.RemoveAt(aIndex);
        }

        protected override void AddChild(UIElement aUIElement)
        {
            base.AddChild(aUIElement);
            originalYPos.Add(aUIElement.RelativeSize.Y);
        }

        void CapScroll()
        {
            if (scrollValue <= 0)
            {
                scrollValue = 0;
                return;
            }

            float max = originalYPos.Last() + GetChild(ChildCount - 1).RelativeSize.Y + Spacing.Y - RelativeSize.Y;
            if (scrollValue > max)
            {
                scrollValue = max;
            }
        }

        void UpdateLootPosition()
        {
            for (int i = 0; i < ChildCount; i++)
            {
                UIElement child = GetChild(i);
                child.Move(new RelativeScreenPosition(child.RelativePos.X, originalYPos[i] - scrollValue));
            }
        }
    }
}
