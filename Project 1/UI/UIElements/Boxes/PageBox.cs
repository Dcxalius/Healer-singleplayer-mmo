using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;

namespace Project_1.UI.UIElements.Boxes
{
    internal class PageBox : Box
    {
        public int CurrentPage => currentPage;
        public int MaxPages => maxPages;
        public int ItemsPerPage => itemsPerPage;
        public Point PageDimensions => pageDimensions;
        public int TotalItems => totalItems;
        public int StartIndex => currentPage * itemsPerPage;

        readonly GFXButton leftArrow;
        readonly GFXButton rightArrow;
        readonly Point pageDimensions;
        readonly int itemsPerPage;
        Action<UIElement, int> bindPageElement;
        Action<UIElement> clearPageElement;
        UIElement[] pageElements;

        int currentPage;
        int maxPages;
        int totalItems;

        public PageBox(UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize, Point aPageDimensions)
            : base(aGfx, aPos, aSize)
        {
            int pageX = Math.Max(1, aPageDimensions.X);
            int pageY = Math.Max(1, aPageDimensions.Y);
            pageDimensions = new Point(pageX, pageY);
            itemsPerPage = pageX * pageY;
            pageElements = Array.Empty<UIElement>();

            RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.05f, Size);
            RelativeScreenPosition arrowSize = new RelativeScreenPosition(0.1f, 0.05f);
            rightArrow = new GFXButton(new List<Action> { PressRightArrow }, new GfxPath(GfxType.UI, "RightArrow"), RelativeScreenPosition.One - spacing - arrowSize, arrowSize, Color.White);
            leftArrow = new GFXButton(new List<Action> { PressLeftArrow }, new GfxPath(GfxType.UI, "LeftArrow"), RelativeScreenPosition.One.OnlyY + spacing.OnlyX - spacing.OnlyY - arrowSize.OnlyY, arrowSize, Color.White);

            AddChild(rightArrow);
            AddChild(leftArrow);

            Reset(0);
        }

        public void SetPageElements(UIElement[] aElements, Action<UIElement, int> aBindPageElement, Action<UIElement> aClearPageElement = null)
        {
            for (int i = 0; i < pageElements.Length; i++)
            {
                KillChild(pageElements[i]);
            }

            pageElements = aElements ?? Array.Empty<UIElement>();
            bindPageElement = aBindPageElement;
            clearPageElement = aClearPageElement;

            for (int i = 0; i < pageElements.Length; i++)
            {
                AddChild(pageElements[i]);
            }

            // Keep arrows on top and clickable.
            KillChild(leftArrow);
            KillChild(rightArrow);
            AddChild(rightArrow);
            AddChild(leftArrow);

            PopulateCurrentPage();
        }

        public void Reset(int aTotalItems)
        {
            totalItems = Math.Max(0, aTotalItems);
            maxPages = Math.Max(1, (int)Math.Ceiling(totalItems / (float)itemsPerPage));
            currentPage = 0;
            UpdateArrowVisiblity();
            PopulateCurrentPage();
        }

        public void SetPage(int aPage)
        {
            if (maxPages <= 0) return;

            int targetPage = Math.Clamp(aPage, 0, maxPages - 1);
            if (targetPage == currentPage) return;

            currentPage = targetPage;
            UpdateArrowVisiblity();
            PopulateCurrentPage();
        }

        public void RefreshCurrentPage()
        {
            PopulateCurrentPage();
        }

        public int IndexOnCurrentPage(int aPageIndex) => StartIndex + aPageIndex;

        void PressRightArrow()
        {
            if (currentPage + 1 >= maxPages) return;
            SetPage(currentPage + 1);
        }

        void PressLeftArrow()
        {
            if (currentPage <= 0) return;
            SetPage(currentPage - 1);
        }

        void UpdateArrowVisiblity()
        {
            bool showArrows = maxPages > 1;
            leftArrow.Visible = showArrows && currentPage > 0;
            rightArrow.Visible = showArrows && currentPage + 1 < maxPages;
        }

        void PopulateCurrentPage()
        {
            if (pageElements.Length == 0) return;

            int startIndex = StartIndex;
            for (int i = 0; i < pageElements.Length; i++)
            {
                UIElement pageElement = pageElements[i];
                int elementIndex = startIndex + i;
                if (elementIndex < totalItems)
                {
                    pageElement.Visible = true;
                    bindPageElement?.Invoke(pageElement, elementIndex);
                    continue;
                }

                if (clearPageElement != null)
                {
                    clearPageElement(pageElement);
                }
                pageElement.Visible = false;
            }
        }
    }
}
