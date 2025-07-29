using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Npcs;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.HUD.Inventory;
using Project_1.UI.HUD.Windows.Gossip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.Windows
{
    internal class ShopWindow : Window
    {
        Npc shopKeeper;
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

            //TODO: Add page system
            //TODO: Add buyback system
        }

        public override void Update()
        {
            base.Update();

            if (shopKeeper != null)
            {
                if (!shopKeeper.InConversationRange(ObjectManager.Player.FeetPosition))
                {
                    ClearShop();
                    CloseWindow();
                }
            }
        }

        public void OpenShop(ShopGossipOption aSO, Npc aShopKeeper)
        {
            itemIDsInShop = aSO.ItemIDsInShop;
            shopKeeper = aShopKeeper;
            for (int i = 0; i < itemsForSale.Length; i++)
            {
                itemsForSale[i].Set(aSO.ItemIDsInShop[i]);
            }
            
            
        }

        public void ClearShop()
        {
            itemIDsInShop = null;
            for (int i = 0; i < itemsForSale.Length; i++)
            {
                itemsForSale[i].Clear();
            }
            shopKeeper = null;
        }
    }
}
