using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.HUD.Inventory;
using Project_1.UI.HUD.Windows.Gossip;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.Windows
{
    internal class ShopWindow : Window
    {
        GFXButton leftArrow;
        GFXButton rightArrow;
        int currentPage;
        int maxPages;


        ItemForSale[] itemsForSale;
        int[] itemIDsInShop;
        public ShopWindow() : base(new UITexture("WhiteBackground", Color.Lime))
        {
            RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.05f, Size);
            itemsForSale = new ItemForSale[10];
            for (int i = 0; i < itemsForSale.Length; i++)
            {
                itemsForSale[i] = new ItemForSale(new RelativeScreenPosition((ItemForSale.size.X + spacing.X) * (i % 2) + spacing.X * (i % 2 + 1), (ItemForSale.size.Y + spacing.Y) * MathF.Floor(i / 2) + spacing.Y * (MathF.Floor(1 / 2) + 1)));
            }
            AddChildren(itemsForSale);

            RelativeScreenPosition arrowSize = new RelativeScreenPosition(0.1f, 0.05f);
            rightArrow = new GFXButton(new List<Action> { PressRightArrow}, new GfxPath(GfxType.UI, "RightArrow"), RelativeScreenPosition.One - spacing - arrowSize, arrowSize, Color.White);
            leftArrow = new GFXButton(new List<Action> { PressLeftArrow}, new GfxPath(GfxType.UI, "LeftArrow"), RelativeScreenPosition.One.OnlyY + spacing.OnlyX - spacing.OnlyY - arrowSize.OnlyY, arrowSize, Color.White);

            AddChild(rightArrow);
            AddChild(leftArrow);
            //TODO: Add buyback system
        }

        public override void Update()
        {
            base.Update();

            // Shop range checks are handled on the simulation thread.
        }

        void PressRightArrow()
        {
            if (currentPage + 1 == maxPages) return;
            if (itemIDsInShop.Length > 10 * maxPages) return;

            currentPage += 1;

            SetNewPage();
        }

        void PressLeftArrow()
        {
            if (currentPage == 0) return;
            currentPage -= 1;

            SetNewPage();
        }

        void SetNewPage()
        {
            for (int i = 0; i < itemsForSale.Length; i++)
            {
                if (itemIDsInShop.Length <= currentPage * 10 + i)
                {
                    for (int j = i; j < itemsForSale.Length; j++)
                    {
                        itemsForSale[j].Clear();
                    }
                    break;
                }

                itemsForSale[i].Set(itemIDsInShop[currentPage * 10 + i]);
            }
        }

        public void OpenShop(int[] itemIds)
        {
            itemIDsInShop = itemIds ?? Array.Empty<int>();
            for (int i = 0; i < itemsForSale.Length; i++)
            {
                if (itemIDsInShop.Length <= i) break;
                itemsForSale[i].Set(itemIDsInShop[i]);
            }

            maxPages = (int)MathF.Floor(itemIDsInShop.Length / 10) + 1;
            currentPage = 0;
        }

        public void ClearShop()
        {
            itemIDsInShop = null;
            for (int i = 0; i < itemsForSale.Length; i++)
            {
                itemsForSale[i].Clear();
            }
        }
    }
}
