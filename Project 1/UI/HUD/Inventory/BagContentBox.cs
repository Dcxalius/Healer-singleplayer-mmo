using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;
using System;

namespace Project_1.UI.HUD.Inventory
{
    internal class BagContentBox : Box
    {
        public int SlotCount => slots.Length;
        Item[] slots = Array.Empty<Item>();
        readonly int bagNr;

        public BagContentBox(UI.UIElements.UIElement aParent, int aBagNr) : base(aParent, new UITexture("WhiteBackground", Color.Beige), RelativeScreenPosition.Zero, RelativeScreenPosition.Zero)
        {
            bagNr = aBagNr;
        }

        void GetBagContent(ItemUiSnapshot[] itemSnapshots, int aSlotCount, int aColumnCount)
        {
            if (aSlotCount == 0)
            {
                Empty();
                return;
            }

            slots = new Item[aSlotCount];
            AbsoluteScreenPosition absItem = InventoryBox.AbsItemSize;
            AbsoluteScreenPosition absSpacing = InventoryBox.AbsBagBoxSpacing;
            SetSize(aSlotCount, ref aColumnCount, absItem, absSpacing);
            SetItems(itemSnapshots, aColumnCount, absItem, absSpacing);
            AddChildren(slots);
        }

        void SetSize(int aSlotCount, ref int aColumnCount, AbsoluteScreenPosition aItemSize, AbsoluteScreenPosition aSpacing)
        {
            float rowCount = MathF.Ceiling(aSlotCount / (float)aColumnCount);
            AbsoluteScreenPosition absoluteSize = aItemSize * rowCount + aSpacing * (rowCount + 1);
            RelativeScreenPosition newSize = (aItemSize * aColumnCount + aSpacing * (aColumnCount + 1)).ToRelativeScreenPosition(ParentSize);
            if (aSlotCount < aColumnCount)
            {
                AbsoluteScreenPosition newWidth = aItemSize * aSlotCount + aSpacing * (aSlotCount + 1);
                newSize.X = newWidth.ToRelativeScreenPosition(ParentSize).X;
                aColumnCount = aSlotCount;
            }

            newSize.Y = absoluteSize.ToRelativeScreenPosition(ParentSize).Y;
            Resize(newSize);
        }

        void SetItems(ItemUiSnapshot[] itemSnapshots, int aColumnCount, AbsoluteScreenPosition aItemSize, AbsoluteScreenPosition aSpacing)
        {
            RelativeScreenPosition itemSize = aItemSize.ToRelativeScreenPosition(Size);
            RelativeScreenPosition spacing = aSpacing.ToRelativeScreenPosition(Size);
            for (int i = 0; i < slots.Length; i++)
            {
                float x = spacing.X + (i % aColumnCount) * ((1 - spacing.X) / aColumnCount);
                float y = spacing.Y + (itemSize.Y + spacing.Y) * (float)Math.Floor((double)i / aColumnCount);
                RelativeScreenPosition pos = new RelativeScreenPosition(x, y);
                if (itemSnapshots != null && i < itemSnapshots.Length && itemSnapshots[i].HasValue)
                {
                    slots[i] = new Item(this, bagNr, i, true, itemSnapshots[i], pos, itemSize);
                }
                else
                {
                    slots[i] = new Item(this, bagNr, i, true, ItemUiSnapshot.Empty, pos, itemSize);
                }
            }
        }

        public void Empty()
        {
            KillAllChildren();
            Resize(RelativeScreenPosition.Zero);
        }

        public void RefreshBag(ItemUiSnapshot[] itemSnapshots, int aSlotCount, int aColumnCount)
        {
            KillAllChildren();
            GetBagContent(itemSnapshots, aSlotCount, aColumnCount);
        }

        public void RefreshSlot(int aSlot, ItemUiSnapshot snapshot)
        {
            if (aSlot < 0 || aSlot >= slots.Length) return;
            slots[aSlot].AssignItem(snapshot);
        }

        public override void Draw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            base.Draw(aBatch);
        }
    }
}
