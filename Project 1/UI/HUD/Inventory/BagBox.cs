using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Items;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.HUD;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;

namespace Project_1.UI.HUD.Inventory
{
    internal class BagBox : Box
    {
        public int SlotCount { get => slots.Length; }
        Item[] slots;
        int bagNr;


        public BagBox(int aBagNr) : base(new UITexture("WhiteBackground", Color.Beige), RelativeScreenPosition.Zero, RelativeScreenPosition.Zero)
        {
            bagNr = aBagNr;
        }

        void GetBagContent(Items.Inventory aInventory, int aSlotCount, int aColumnCount)
        {
            if (aSlotCount == 0)
            {
                Empty();
                return;
            }

            slots = new Item[aSlotCount];
            //AbsoluteScreenPosition absItem = InventoryBox.ItemSize.ToAbsoluteScreenPos(Size);
            AbsoluteScreenPosition absItem = InventoryBox.AbsItemSize;
            //AbsoluteScreenPosition absSpacing = InventoryBox.BagBoxSpacing.ToAbsoluteScreenPos(Size);
            AbsoluteScreenPosition absSpacing = InventoryBox.AbsBagBoxSpacing;

            SetSize(aSlotCount, ref aColumnCount, absItem, absSpacing);

            SetItems(aInventory, aColumnCount, absItem, absSpacing);
            AddChildren(slots);
        }

        void GetBagContent(Project_1.Items.Item[] itemSnapshots, int aSlotCount, int aColumnCount)
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

        void SetItems(Items.Inventory aInventory, int aColumnCount, AbsoluteScreenPosition aItemSize, AbsoluteScreenPosition aSpacing)
        {
            Items.Item[] items = aInventory.GetItemsInBox(bagNr);

            RelativeScreenPosition itemSize = aItemSize.ToRelativeScreenPosition(Size);
            RelativeScreenPosition spacing = aSpacing.ToRelativeScreenPosition(Size);

            for (int i = 0; i < slots.Length; i++)
            {
                // float x = spacing.X + i % aColumnCount * (itemSize.X + spacing.X);
                float x = spacing.X + (i % aColumnCount) * ((1 - spacing.X) / (float)aColumnCount);

                float y = spacing.Y + (itemSize.Y + spacing.Y) * (float)Math.Floor((double)i / aColumnCount);
                RelativeScreenPosition pos = new RelativeScreenPosition(x, y);
                // DebugManager.Print(pos.ToAbsoluteScreenPos(Size).ToString());
                if (items[i] != null)
                {
                    slots[i] = new Item(bagNr, i, true, items[i], pos, itemSize);
                }
                else
                {
                    slots[i] = new Item(bagNr, i, true, (Items.Item)null, pos, itemSize);
                    //DebugManager.Print(slots[i].Size.ToString() );
                }
            }
        }

        void SetItems(Project_1.Items.Item[] itemSnapshots, int aColumnCount, AbsoluteScreenPosition aItemSize, AbsoluteScreenPosition aSpacing)
        {
            RelativeScreenPosition itemSize = aItemSize.ToRelativeScreenPosition(Size);
            RelativeScreenPosition spacing = aSpacing.ToRelativeScreenPosition(Size);

            for (int i = 0; i < slots.Length; i++)
            {
                float x = spacing.X + (i % aColumnCount) * ((1 - spacing.X) / (float)aColumnCount);
                float y = spacing.Y + (itemSize.Y + spacing.Y) * (float)Math.Floor((double)i / aColumnCount);
                RelativeScreenPosition pos = new RelativeScreenPosition(x, y);
                if (itemSnapshots != null && i < itemSnapshots.Length && itemSnapshots[i] != null)
                {
                    slots[i] = new Item(bagNr, i, true, itemSnapshots[i], pos, itemSize);
                }
                else
                {
                    slots[i] = new Item(bagNr, i, true, (Items.Item)null, pos, itemSize);
                }
            }
        }

        public void Empty()
        {
            KillAllChildren();
            Resize(RelativeScreenPosition.Zero);
        }

        public void RefreshSlot(int aSlot, Items.Inventory aInventory)
        {
            slots[aSlot].AssignItem(aInventory.GetItemInSlot(bagNr, aSlot));
        }

        public void RefreshBag(Items.Inventory aInventory, int aSlotCount, int aColumnCount)
        {
            KillAllChildren();
            GetBagContent(aInventory, aSlotCount, aColumnCount);
        }

        public void RefreshBag(Project_1.Items.Item[] itemSnapshots, int aSlotCount, int aColumnCount)
        {
            KillAllChildren();
            GetBagContent(itemSnapshots, aSlotCount, aColumnCount);
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);
        }
    }
}
