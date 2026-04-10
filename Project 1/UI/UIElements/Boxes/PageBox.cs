using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.UIElements;
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
        readonly Label pageTitle;
        readonly Point pageDimensions;
        readonly int itemsPerPage;
        Action<UIElement, int> bindPageElement;
        Action<UIElement> clearPageElement;
        Func<int, string> pageTitleProvider;
        protected UIElement[] pageElements;

        int currentPage;
        int maxPages;
        int totalItems;

        public PageBox(UIElement aParent, Func<PageBox, UIElement[]> aPageElementFactory, UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize, Point aPageDimensions, Action<UIElement, int> aBindPageElement = null, Action<UIElement> aClearPageElement = null)
            : base(aParent, aGfx, aPos, aSize)
        {
            int pageX = Math.Max(1, aPageDimensions.X);
            int pageY = Math.Max(1, aPageDimensions.Y);
            pageDimensions = new Point(pageX, pageY);
            itemsPerPage = pageX * pageY;
            pageElements = aPageElementFactory.Invoke(this);
            bindPageElement = aBindPageElement;
            clearPageElement = aClearPageElement;

            RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.05f, Size);
            RelativeScreenPosition arrowSize = new RelativeScreenPosition(0.1f, 0.05f);
            pageTitle = new Label(this, new RelativeScreenPosition(0f, 0f), new RelativeScreenPosition(1f, 0.1f), Label.TextAllignment.Centred, Color.Black);
            rightArrow = new GFXButton(this, new List<Action> { PressRightArrow }, new GfxPath(GfxType.UI, "RightArrow"), RelativeScreenPosition.One - spacing - arrowSize, arrowSize, Color.White);
            leftArrow = new GFXButton(this, new List<Action> { PressLeftArrow }, new GfxPath(GfxType.UI, "LeftArrow"), RelativeScreenPosition.One.OnlyY + spacing.OnlyX - spacing.OnlyY - arrowSize.OnlyY, arrowSize, Color.White);
            pageTitleProvider = DefaultPageTitle;
            Reset(0);
        }

        public void SetPageTitleProvider(Func<int, string> aProvider)
        {
            pageTitleProvider = aProvider ?? DefaultPageTitle;
            UpdatePageTitle();
        }

        public void SetBinders(Action<UIElement, int> aBindPageElement, Action<UIElement> aClearPageElement = null)
        {
            bindPageElement = aBindPageElement;
            clearPageElement = aClearPageElement;

            PopulateCurrentPage();
        }

        public void Reset(int aTotalItems)
        {
            totalItems = Math.Max(0, aTotalItems);
            maxPages = Math.Max(1, (int)Math.Ceiling(totalItems / (float)itemsPerPage));
            currentPage = 0;
            UpdateArrowVisiblity();
            PopulateCurrentPage();
            UpdatePageTitle();
        }

        public void SetPage(int aPage)
        {
            if (maxPages <= 0) return;

            int targetPage = Math.Clamp(aPage, 0, maxPages - 1);
            if (targetPage == currentPage) return;

            currentPage = targetPage;
            UpdateArrowVisiblity();
            PopulateCurrentPage();
            UpdatePageTitle();
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
                    //TODO: This causes page elemts to be hoverable on creation
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

        void UpdatePageTitle()
        {
            pageTitle.Text = pageTitleProvider?.Invoke(currentPage) ?? DefaultPageTitle(currentPage);
        }

        static string DefaultPageTitle(int aPageIndex) => $"Page {aPageIndex + 1}";
    }
}
