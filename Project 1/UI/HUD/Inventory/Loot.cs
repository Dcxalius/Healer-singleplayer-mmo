using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Items;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.HUD;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.Inventory
{
    internal class Loot : Box
    {
        Item item;
        Label itemName;
        readonly int slotIndex;

        RelativeScreenPosition Spacing
        {
            get
            {
                AbsoluteScreenPosition size = Size;
                if (size.X <= 0 || size.Y <= 0) return RelativeScreenPosition.Zero;
                return RelativeScreenPosition.GetSquareFromX(0.005f, size);
            }
        }

        public Loot(int aSlotIndex, Items.Item aItem, GfxPath aPath) : base(new UITexture("GrayBackground", Color.AliceBlue), RelativeScreenPosition.Zero, RelativeScreenPosition.Zero)
        {
            slotIndex = aSlotIndex;
            if (aItem == null) return;
            item = new Item(-2, aSlotIndex, true, aItem, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero);
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

            AbsoluteScreenPosition absSize = aSize.ToAbsoluteScreenPos(ParentSize);
            if (absSize.X <= 0 || absSize.Y <= 0) return;

            RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.005f, absSize);

            item.Move(spacing);
            RelativeScreenPosition itemSize = RelativeScreenPosition.GetSquareFromY(1f - spacing.Y * 2, absSize);
            item.Resize(itemSize);
            itemName.Move(itemSize.OnlyX + spacing + spacing.OnlyX);
            itemName.Resize(RelativeScreenPosition.One - itemSize.OnlyX - spacing * 2);


        }

        public void Hide()
        {
            gfx = null;
            item = null;
            Resize(RelativeScreenPosition.Zero);
        }

        public void UpdateItem(Items.Item aItem)
        {
            if (aItem == null)
            {
                Hide();
                return;
            }

            if (item == null)
            {
                // recreate if previously hidden
                item = new Item(-2, slotIndex, true, aItem, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero);
                AddChild(item);
            }

            item.AssignItem(aItem);
            if (itemName == null)
            {
                itemName = new Label(aItem.Name, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.CentreLeft, aItem.ItemQualityColor);
                AddChild(itemName);
            }
            else
            {
                itemName.Text = aItem.Name;
                itemName.Color = aItem.ItemQualityColor;
            }
            Resize(RelativeSize);
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            if (heldEvents.ClickThatCreated != InputManager.ClickType.Right) return;


            Mailboxes.Main.Publish(new LootItemRequested(item.slotIndex, null));

            base.ClickedOnAndReleasedOnMe();
        }
    }
}
