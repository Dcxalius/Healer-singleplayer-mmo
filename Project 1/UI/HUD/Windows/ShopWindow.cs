using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using System;

namespace Project_1.UI.HUD.Windows
{
    internal class ShopWindow : Window
    {
        readonly PageBox pageBox;
        readonly ItemForSale[] itemsForSale;
        ItemUiSnapshot[] itemsInShop = Array.Empty<ItemUiSnapshot>();
        string shopkeeperName = string.Empty;

        public ShopWindow() : base(new UITexture("WhiteBackground", Color.Lime))
        {
            RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.05f, Size);
            itemsForSale = new ItemForSale[10];
            for (int i = 0; i < itemsForSale.Length; i++)
            {
                itemsForSale[i] = new ItemForSale(new RelativeScreenPosition((ItemForSale.size.X + spacing.X) * (i % 2) + spacing.X * (i % 2 + 1), (ItemForSale.size.Y + spacing.Y) * MathF.Floor(i / 2) + spacing.Y * (MathF.Floor(1 / 2) + 1)));
            }

            pageBox = new PageBox(new UITexture("WhiteBackground", Color.Transparent), RelativeScreenPosition.Zero, RelativeScreenPosition.One, new Point(2, 5));
            pageBox.SetPageElements(itemsForSale, BindItemSlot, ClearItemSlot);
            AddChild(pageBox);
            //TODO: Add buyback system
        }

        public override void Update()
        {
            base.Update();
            // Shop range checks are handled on the simulation thread.
        }

        void BindItemSlot(UIElement aElement, int aItemIndex)
        {
            ItemForSale itemForSale = aElement as ItemForSale;
            if (itemForSale == null) return;
            if (aItemIndex < 0 || aItemIndex >= itemsInShop.Length)
            {
                itemForSale.Clear();
                return;
            }

            itemForSale.Set(itemsInShop[aItemIndex]);
        }

        void ClearItemSlot(UIElement aElement)
        {
            ItemForSale itemForSale = aElement as ItemForSale;
            itemForSale?.Clear();
        }

        public void OpenShop(int[] itemIds, string aShopkeeperName)
        {
            if (itemIds == null || itemIds.Length == 0)
            {
                itemsInShop = Array.Empty<ItemUiSnapshot>();
            }
            else
            {
                itemsInShop = new ItemUiSnapshot[itemIds.Length];
                for (int i = 0; i < itemIds.Length; i++)
                {
                    itemsInShop[i] = ItemUiSnapshot.FromItemId(itemIds[i]);
                }
            }

            shopkeeperName = aShopkeeperName ?? string.Empty;
            pageBox.SetPageTitleProvider(GetPageTitle);
            pageBox.Reset(itemsInShop.Length);
        }

        public void ClearShop()
        {
            itemsInShop = Array.Empty<ItemUiSnapshot>();
            shopkeeperName = string.Empty;
            pageBox.SetPageTitleProvider(null);
            pageBox.Reset(0);
        }

        string GetPageTitle(int aPageIndex)
        {
            if (string.IsNullOrWhiteSpace(shopkeeperName))
            {
                return $"Page {aPageIndex + 1}";
            }

            return shopkeeperName;
        }
    }
}
