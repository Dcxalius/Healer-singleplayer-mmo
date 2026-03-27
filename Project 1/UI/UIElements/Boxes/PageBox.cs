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
        UIElement[] pageElements;

        int currentPage;
        int maxPages;
        int totalItems;

        public PageBox(UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize, Point aPageDimensions, UIElement aParent = null)
            : base(aGfx, aPos, aSize, aParent)
        {
            int pageX = Math.Max(1, aPageDimensions.X);
            int pageY = Math.Max(1, aPageDimensions.Y);
            pageDimensions = new Point(pageX, pageY);
            itemsPerPage = pageX * pageY;
            pageElements = Array.Empty<UIElement>();

            RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.05f, Size);
            RelativeScreenPosition arrowSize = new RelativeScreenPosition(0.1f, 0.05f);
            pageTitle = new Label(null, new RelativeScreenPosition(0f, 0f), new RelativeScreenPosition(1f, 0.1f), Label.TextAllignment.Centred, Color.Black, aParent: this);
            rightArrow = new GFXButton(new List<Action> { PressRightArrow }, new GfxPath(GfxType.UI, "RightArrow"), RelativeScreenPosition.One - spacing - arrowSize, arrowSize, Color.White, aParent: this);
            leftArrow = new GFXButton(new List<Action> { PressLeftArrow }, new GfxPath(GfxType.UI, "LeftArrow"), RelativeScreenPosition.One.OnlyY + spacing.OnlyX - spacing.OnlyY - arrowSize.OnlyY, arrowSize, Color.White, aParent: this);
            pageTitleProvider = DefaultPageTitle;

            AddChild(pageTitle);
            AddChild(rightArrow);
            AddChild(leftArrow);

            Reset(0);
        }

        public void SetPageTitleProvider(Func<int, string> aProvider)
        {
            pageTitleProvider = aProvider ?? DefaultPageTitle;
            UpdatePageTitle();
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

            // Keep title visible above page content.
            KillChild(pageTitle);
            AddChild(pageTitle);

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
