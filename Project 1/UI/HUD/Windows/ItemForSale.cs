using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Items;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.HUD.Inventory;
using Project_1.UI.HUD.Managers;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.Windows
{
    internal class ItemForSale : Box
    {
        Image displayItem;
        Items.Item itemForSale;
        Label itemName;
        Label goldCost;
        Image goldImage;

        public readonly static RelativeScreenPosition size;
        readonly static RelativeScreenPosition spacing;

        static ItemForSale()
        {
            size = new RelativeScreenPosition(0.4f, 0.10f);
            spacing = RelativeScreenPosition.GetSquareFromX(0.06f, size.ToAbsoluteScreenPos(Window.WindowSize.ToAbsoluteScreenPos()));
        }

        public ItemForSale(RelativeScreenPosition aPos) : base(new UITexture("WhiteBackground", Color.Lavender), aPos, size)
        {
            displayItem = new Image(UITexture.Null, spacing, RelativeScreenPosition.GetSquareFromY(1f - spacing.Y * 2, size.ToAbsoluteScreenPos(Window.WindowSize.ToAbsoluteScreenPos())));

            itemName = new Label(null, displayItem.RelativeSize.OnlyX + spacing.OnlyX * 2, new RelativeScreenPosition(1, 0.5f) - displayItem.RelativeSize.OnlyX - spacing.OnlyX * 2, Label.TextAllignment.CentreLeft, Color.Black);
            
            goldCost = new Label(null, displayItem.RelativeSize.OnlyX + displayItem.RelativeSize.OnlyY / 2 + spacing.OnlyX, new RelativeScreenPosition(0.9f, 0.5f) - displayItem.RelativeSize.OnlyX - spacing.OnlyX * 2, Label.TextAllignment.CentreRight, Color.Black);
            goldImage = new Image(new UITexture("Gold", Color.White), new RelativeScreenPosition(1f - spacing.X, 0.75f), RelativeScreenPosition.GetSquareFromX(0.1f, size.ToAbsoluteScreenPos(Window.WindowSize.ToAbsoluteScreenPos())));
            
            AddChild(displayItem);
            AddChild(itemName); 
            AddChild(goldCost);
            AddChild(goldImage);

            //TODO: Allow the setting of an item to have multiple counts
        }

        protected override void OnHover()
        {
            base.OnHover();

            if (!Visible) return;
            if (itemForSale == null) return;
            HUDManager.SetDescriptorBox(itemForSale, RelativePositionOnScreen);
        }

        protected override void OnDeHover()
        {
            base.OnDeHover();

            HUDManager.SetDescriptorBox(null);
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();

            if (!Visible) return;

            if (itemForSale == null) return;

            if (itemForSale.Cost > ObjectManager.Player.Gold) return; //TODO: Print error msg

            ObjectManager.Player.ChangeGold(-itemForSale.Cost);
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(itemForSale.ID, itemForSale.Count));
        }

        public void Set(int aItemID)
        {
            itemForSale = ItemFactory.CreateItem(aItemID);
            displayItem.SetImage(itemForSale.GfxPath);
            goldCost.Text = itemForSale.Cost.ToString();
            itemName.Text = itemForSale.Name;
            goldImage.Visible = true;
        }

        public void Clear()
        {
            displayItem.ClearImage();
            goldImage.Visible = false;
            itemName.Text = null;
            goldCost.Text = null;
            itemForSale = null;
        }
    }
}
