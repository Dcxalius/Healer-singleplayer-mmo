using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Boxes
{
    internal abstract class ScrollableBox : Box
    {

        public const float WidthOfBar = 0.03f;
        public const float WidthOfSpacing = 0.005f;

        public ScrollableBox(UIElement aParent, UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, aGfx, aPos, aSize)
        {
        }

        public abstract void SetScrollValue(float v);
    }

    internal class ScrollableBox<T> : ScrollableBox where T : UIElement
    {
        public T this[int index]
        {
            get => scrollableElements[index];
            set
            {
                scrollableElements[index] = value;
                scrollableElements[index].Resize(elementSize);
                scrollableElements[index].Move(new RelativeScreenPosition(spacing.X, elementSize.Y * index + spacing.Y * (index + 1)));
                originalYPos[index] = scrollableElements[index].RelativePos.Y;
            }
        }

        ScrollBar scrollBar;
        public int ScrollableElementsCount => scrollableElements.Count;
        List<T> scrollableElements;
        protected bool TooMuchForWindow
        {
            get
            {
                //TODO: Hide plimp if this returns false;
                if (scrollableElements.Count == 0) return false; 
                return originalYPos.Last() + elementSize.Y + Spacing.Y > RelativeSize.Y;
            }
        }

        float scrollValue;
        List<float> originalYPos;
        protected RelativeScreenPosition Spacing
        {
            get => spacing;
            set => spacing = value;
        }
        RelativeScreenPosition spacing;

        protected float ScrollSpeed
        {
            get => scrollSpeed;
            set => scrollSpeed = value;
        }
        float scrollSpeed = 0.1f;
        float MaxScroll => scrollableElements.Count == 0 ? 0f : originalYPos.Last() + scrollableElements.Last().RelativeSize.Y + Spacing.Y - 1f;
        public RelativeScreenPosition ElementSize
        {
            get => elementSize;
            set
            {
                elementSize = value;
                for (int i = 0; i < scrollableElements.Count; i++)
                {
                    scrollableElements[i].Resize(value);
                    scrollableElements[i].Move(new RelativeScreenPosition(spacing.X, spacing.Y + (elementSize.Y + spacing.Y) * i));
                    originalYPos[i] = scrollableElements[i].RelativePos.Y;
                }
            }
        }

        RelativeScreenPosition elementSize;

        public ScrollableBox(UIElement aParent, float visibleElements, UITexture aGfx, Color aBarColor, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, aGfx, aPos, aSize)
        {
            scrollableElements = new List<T>();

            RelativeScreenPosition barSpacing = RelativeScreenPosition.GetSquareFromX(WidthOfSpacing, Size);
            RelativeScreenPosition sizeOfScrollBar = new RelativeScreenPosition(WidthOfBar, 1f - barSpacing.Y - barSpacing.Y);
            scrollBar = new ScrollBar(this, aBarColor, new RelativeScreenPosition(1f - sizeOfScrollBar.X - barSpacing.X, barSpacing.Y), sizeOfScrollBar);
            originalYPos = new List<float>();

            spacing = RelativeScreenPosition.GetSquareFromX(WidthOfSpacing, Size);
            capturesScroll = true;

            elementSize = new RelativeScreenPosition(1f - spacing.X - spacing.X - sizeOfScrollBar.X - barSpacing.X, (1f - spacing.Y) / visibleElements);

            if (!TooMuchForWindow)
            {
                scrollBar.Visible = false;
            }
        }

        public override void SetScrollValue(float aValue)
        {
            if (scrollableElements.Count == 0)
            {
                scrollValue = 0f;
                scrollBar.SetValue(0f);
                return;
            }
            scrollBar.SetValue(aValue);
            scrollValue = MaxScroll * aValue;
            CapScroll();
            UpdateScrollableComponentPosition();
        }

        protected override void ScrolledOnMe(ScrollEvent aScrollEvent)
        {
            base.ScrolledOnMe(aScrollEvent);
            if (!TooMuchForWindow) return;

            scrollValue += aScrollEvent.DirectionAndSteps * ScrollSpeed;

            CapScroll();
            UpdateScrollableComponentPosition();

            if (MaxScroll > 0)
            {
                scrollBar.SetValue(scrollValue / MaxScroll);
            }
        }

        public void RemoveAllScrollableElements()
        {
            for (int i = 0; i < scrollableElements.Count; i++) KillChild(scrollableElements[i]);
            scrollableElements.Clear();
            originalYPos.Clear();
            scrollValue = 0f;
            scrollBar.SetValue(0f);
            scrollBar.SetScrollPlimpSize(1f);
        }


        public void RemoveScrollableElement(T aUIElement)
        {
            int scrollableID = scrollableElements.IndexOf(aUIElement);
            scrollableElements.RemoveAt(scrollableID);
            originalYPos.RemoveAt(scrollableID);
            int index = GetChildID(aUIElement);
            KillChild(index);

            SizeCheck();

        }

        public void RemoveScrollableElement(int aIndex) => RemoveScrollableElement(scrollableElements[aIndex]);

        public void AddScrollableElement(T aUIElement)
        {
            aUIElement.Resize(elementSize);
            aUIElement.Move(new RelativeScreenPosition(spacing.X, elementSize.Y * ScrollableElementsCount + spacing.Y * (ScrollableElementsCount + 1)));
            scrollableElements.Add(aUIElement);
            originalYPos.Add(aUIElement.RelativePos.Y);

            SizeCheck();
        }

        void SizeCheck()
        {
            if (!TooMuchForWindow)
            {
                scrollBar.SetScrollPlimpSize(1f);
                scrollBar.SetValue(0f);
                scrollBar.Visible = false;
            }
            else
            {
                scrollBar.Visible = true;
                scrollBar.SetScrollPlimpSize((elementSize.Y + spacing.Y) / (originalYPos.Last() + scrollableElements.Last().RelativeSize.Y + Spacing.Y));
            }

        }

        public void AddScrollableElements(List<T> aList)
        {
            for (int i = 0; i < aList.Count; i++) AddScrollableElement(aList[i]);
        }

        public void AddScrollableElements(T[] aArray)
        {
            for (int i = 0; i < aArray.Length; i++) AddScrollableElement(aArray[i]);
        }

        void CapScroll()
        {
            if (scrollValue <= 0)
            {
                scrollValue = 0;
                return;
            }

            
            if (scrollValue > MaxScroll)
            {
                scrollValue = MaxScroll;
            }
        }

        void UpdateScrollableComponentPosition()
        {
            for (int i = 0; i < scrollableElements.Count; i++)
            {

                scrollableElements[i].Move(new RelativeScreenPosition(scrollableElements[i].RelativePos.X, originalYPos[i] - scrollValue));
            }
        }

        internal void Sort(IComparer<T> comparer)
        {
            scrollableElements.Sort(comparer);
            UpdateScrollableComponentPosition();
        }
    }
}
