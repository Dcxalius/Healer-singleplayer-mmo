using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;

namespace Project_1.UI.UIElements.Boxes
{
    internal interface IPageBoxLabeledElement
    {
        string PageBoxLabel { get; }
    }

    internal class PageBox : Box
    {
        public int CurrentPage => currentPage;
        public int MaxPages => maxPages;
        public int ItemsPerPage => itemsPerPage;
        public Point PageDimensions => pageDimensions;
        public int TotalItems => totalItems;
        public int StartIndex => currentPage * itemsPerPage;

        readonly SquareGFXButton leftArrow;
        readonly SquareGFXButton rightArrow;
        readonly Label pageTitle;
        readonly Label[] pageLabels;
        readonly Point pageDimensions;
        readonly int itemsPerPage;
        Action<UIElement, int> bindPageElement;
        Action<UIElement> clearPageElement;
        Func<int, string> pageTitleProvider;
        protected UIElement[] pageElements;

        int currentPage;
        int maxPages;
        int totalItems;

        public PageBox(UIElement aParent, Func<PageBox, UIElement[]> aPageElementFactory, UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize, Point aPageDimensions, Action<UIElement, int> aBindPageElement = null, Action<UIElement> aClearPageElement = null, Func<PageBox, Label[]> aPageLabelFactory = null)
            : base(aParent, aGfx, aPos, aSize)
        {
            int pageX = Math.Max(1, aPageDimensions.X);
            int pageY = Math.Max(1, aPageDimensions.Y);
            pageDimensions = new Point(pageX, pageY);
            itemsPerPage = pageX * pageY;
            pageElements = aPageElementFactory.Invoke(this);
            pageLabels = aPageLabelFactory?.Invoke(this);
            bindPageElement = aBindPageElement;
            clearPageElement = aClearPageElement;

            RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.05f, Size);
            RelativeScreenPosition arrowSize = new RelativeScreenPosition(0.1f, 0f);
            pageTitle = new Label(this, new RelativeScreenPosition(0f, 0f), new RelativeScreenPosition(1f, 0.1f), Label.TextAllignment.Centred, Color.Black);
            rightArrow = new SquareGFXButton(this, new List<Action> { PressRightArrow }, new GfxPath(GfxType.UI, "RightArrow"), RelativeScreenPosition.Zero, arrowSize, Color.White);
            leftArrow = new SquareGFXButton(this, new List<Action> { PressLeftArrow }, new GfxPath(GfxType.UI, "LeftArrow"), RelativeScreenPosition.Zero, arrowSize, Color.White);
            LayoutArrows(spacing);
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

        void LayoutArrows(RelativeScreenPosition spacing)
        {
            rightArrow.Move(RelativeScreenPosition.One - spacing - rightArrow.RelativeSize);
            leftArrow.Move(new RelativeScreenPosition(spacing.X, 1f - spacing.Y - leftArrow.RelativeSize.Y));
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
                    UpdatePageLabel(i, pageElement);
                    continue;
                }

                if (clearPageElement != null)
                {
                    clearPageElement(pageElement);
                }
                ClearPageLabel(i);
                pageElement.Visible = false;
            }
        }

        void UpdatePageTitle()
        {
            pageTitle.Text = pageTitleProvider?.Invoke(currentPage) ?? DefaultPageTitle(currentPage);
        }

        void UpdatePageLabel(int aPageIndex, UIElement aPageElement)
        {
            if (pageLabels == null || aPageIndex < 0 || aPageIndex >= pageLabels.Length) return;

            Label pageLabel = pageLabels[aPageIndex];
            if (pageLabel == null) return;

            pageLabel.Visible = aPageElement.Visible;
            pageLabel.Text = (aPageElement as IPageBoxLabeledElement)?.PageBoxLabel;
        }

        void ClearPageLabel(int aPageIndex)
        {
            if (pageLabels == null || aPageIndex < 0 || aPageIndex >= pageLabels.Length) return;

            Label pageLabel = pageLabels[aPageIndex];
            if (pageLabel == null) return;

            pageLabel.Text = null;
            pageLabel.Visible = false;
        }

        static string DefaultPageTitle(int aPageIndex) => $"Page {aPageIndex + 1}";
    }
}
