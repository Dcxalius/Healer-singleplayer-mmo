using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Input;
using Project_1.Items;
using Project_1.Textures;
using Project_1.UI.HUD;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Inventory
{
    internal class Loot : Box
    {
        Item item;
        Label itemName;

        static RelativeScreenPosition Spacing => RelativeScreenPosition.GetSquareFromX(0.005f);

        public Loot(int aSlotIndex, Items.Item aItem, GfxPath aPath) : base(new UITexture("GrayBackground", Color.AliceBlue), RelativeScreenPosition.Zero, RelativeScreenPosition.Zero)
        {
            if (aItem == null) return;
            item = new Item(-2, aSlotIndex, true, aItem.ItemQualityColor, aPath, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero);
            itemName = new Label(aItem.Name, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.CentreLeft, aItem.ItemQualityColor);
            AddChild(item);
            AddChild(itemName);
            if (aItem.MaxStack == 1) return;
            item.ItemCount = aItem.Count.ToString();
        }

        public override void Resize(RelativeScreenPosition aSize)
        {
            base.Resize(aSize);

            if (item == null) return;

            item.Move(Spacing);
            RelativeScreenPosition itemSize = RelativeScreenPosition.GetSquareFromY(aSize.Y - Spacing.Y * 2);
            item.Resize(itemSize);
            itemName.Move(itemSize.OnlyX + Spacing + Spacing.OnlyX);
            itemName.Resize(aSize - itemSize.OnlyX - Spacing * 2);

            
        }

        public void Hide()
        {
            gfx = null;
            item = null;
            Resize(RelativeScreenPosition.Zero);
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            if (heldEvents.ClickThatCreated != InputManager.ClickType.Right) return;


            ObjectManager.Player.Inventory.LootItem(item.slotIndex);

            base.ClickedOnAndReleasedOnMe();
        }
    }
}
