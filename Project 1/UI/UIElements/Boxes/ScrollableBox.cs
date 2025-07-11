﻿using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project_1.UI.UIElements.Boxes
{
    internal class ScrollableBox : Box
    {
        ScrollBar scrollBar;
        public int ScrollableElementsCount => scrollableElements.Count;
        List<UIElement> scrollableElements;
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
        float MaxScroll => originalYPos.Last() + scrollableElements.Last().RelativeSize.Y + Spacing.Y - 1f;
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
        public const float WidthOfBar = 0.03f;
        public const float WidthOfSpacing = 0.005f;

        public ScrollableBox(float visibleElements, UITexture aGfx, Color aBarColor, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aGfx, aPos, aSize)
        {
            scrollableElements = new List<UIElement>();

            RelativeScreenPosition barSpacing = RelativeScreenPosition.GetSquareFromX(WidthOfSpacing, Size);
            RelativeScreenPosition sizeOfScrollBar = new RelativeScreenPosition(WidthOfBar, 1f - barSpacing.Y - barSpacing.Y);
            scrollBar = new ScrollBar(aBarColor, new RelativeScreenPosition(1f - sizeOfScrollBar.X - barSpacing.X, barSpacing.Y), sizeOfScrollBar);
            originalYPos = new List<float>();
            AddChild(scrollBar);

            spacing = RelativeScreenPosition.GetSquareFromX(WidthOfSpacing, Size);
            capturesScroll = true;

            elementSize = new RelativeScreenPosition(1f - spacing.X - spacing.X - sizeOfScrollBar.X - barSpacing.X, (1f - spacing.Y) / visibleElements);
        }

        public void SetScrollValue(float aValue)
        {
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

            scrollBar.SetValue(scrollValue / MaxScroll);
        }

        public void RemoveAllScrollableElements()
        {
            for (int i = 0; i < scrollableElements.Count; i++) KillChild(scrollableElements[i]);
            scrollableElements.Clear();
            originalYPos.Clear();
        }


        public void RemoveScrollableElement(UIElement aUIElement)
        {
            int scrollableID = scrollableElements.IndexOf(aUIElement);
            scrollableElements.RemoveAt(scrollableID);
            originalYPos.RemoveAt(scrollableID);
            int index = GetChildID(aUIElement);
            KillChild(index);
        }

        public void RemoveScrollableElement(int aIndex) => RemoveScrollableElement(scrollableElements[aIndex]);

        public void AddScrollableElement(UIElement aUIElement)
        {
            aUIElement.Resize(elementSize);
            aUIElement.Move(new RelativeScreenPosition(spacing.X, elementSize.Y * ScrollableElementsCount + spacing.Y * (ScrollableElementsCount + 1)));
            scrollableElements.Add(aUIElement);
            originalYPos.Add(aUIElement.RelativePos.Y);
            AddChild(aUIElement);

            if (!TooMuchForWindow) scrollBar.SetScrollPlimpSize(1f);
            else scrollBar.SetScrollPlimpSize((elementSize.Y + spacing.Y) / (originalYPos.Last() + scrollableElements.Last().RelativeSize.Y + Spacing.Y));
        }

        public void AddScrollableElements(List<UIElement> aList)
        {
            for (int i = 0; i < aList.Count; i++) AddScrollableElement(aList[i]);
        }

        public void AddScrollableElements(UIElement[] aArray)
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
    }
}
