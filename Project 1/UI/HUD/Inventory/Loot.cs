using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;

namespace Project_1.UI.HUD.Inventory
{
    internal class Loot : Box
    {
        Item item;
        Label itemName;
        readonly int slotIndex;

        public Loot(int aSlotIndex, ItemUiSnapshot snapshot) : base(new UITexture("GrayBackground", Color.AliceBlue), RelativeScreenPosition.Zero, RelativeScreenPosition.Zero)
        {
            slotIndex = aSlotIndex;
            if (!snapshot.HasValue) return;

            item = new Item(-2, aSlotIndex, true, snapshot, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero);
            itemName = new Label(snapshot.Name, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.CentreLeft, snapshot.QualityColor);
            AddChild(item);
            AddChild(itemName);
            if (snapshot.MaxStack <= 1) return;
            item.ItemCount = snapshot.Count.ToString();
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

        public void UpdateItem(ItemUiSnapshot snapshot)
        {
            if (!snapshot.HasValue)
            {
                Hide();
                return;
            }

            if (item == null)
            {
                item = new Item(-2, slotIndex, true, snapshot, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero);
                AddChild(item);
            }

            item.AssignItem(snapshot);
            if (itemName == null)
            {
                itemName = new Label(snapshot.Name, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.CentreLeft, snapshot.QualityColor);
                AddChild(itemName);
            }
            else
            {
                itemName.Text = snapshot.Name;
                itemName.Color = snapshot.QualityColor;
            }

            Resize(RelativeSize);
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            if (heldEvents.ClickThatCreated != InputManager.ClickType.Right) return;
            MailboxManager.PublishSimCommand(new LootItemRequested(slotIndex, null));
            base.ClickedOnAndReleasedOnMe();
        }
    }
}
