using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities.Players;
using Project_1.Items;
using Project_1.Textures;
using Project_1.UI.HUD;
using System;
using System.Collections.Generic;

namespace Project_1.UI.UIElements.Inventory
{
    internal class BagBox : Box
    {
        public int SlotCount { get => slots.Length; }
        Item[] slots;
        int columnCount;
        int bagNr;
        

        public BagBox(int aBagNr, int aSlotCount, int aColumnCount, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("WhiteBackground", Color.Beige), aPos, aSize)
        {
            bagNr = aBagNr;
            GetBagContent(aSlotCount, aColumnCount);
        }

        void GetBagContent(int aSlotCount, int aColumnCount)
        {
            if (aSlotCount == 0)
            {
                MakeZero();
                return;
            }
            slots = new Item[aSlotCount];

            columnCount = aColumnCount;
            Items.Item[] items = ObjectManager.Player.Inventory.GetItemsInBox(bagNr);

            float rowCount = (float)Math.Ceiling(aSlotCount / (double)aColumnCount);
            Resize(new RelativeScreenPosition(aColumnCount * InventoryBox.itemSize.X + aColumnCount * InventoryBox.spacing.X + InventoryBox.spacing.X, rowCount * InventoryBox.itemSize.Y + rowCount * InventoryBox.spacing.Y + InventoryBox.spacing.Y));

            for (int i = 0; i < slots.Length; i++)
            {
                float x = ((i % columnCount) * ((InventoryBox.itemSize.X + InventoryBox.spacing.X)) + InventoryBox.spacing.X);
                float y = InventoryBox.spacing.Y + (InventoryBox.itemSize.Y + InventoryBox.spacing.Y) * (float)Math.Floor((double)i / columnCount);
                RelativeScreenPosition pos = new RelativeScreenPosition(x, y);
                RelativeScreenPosition size = InventoryBox.itemSize;

                if (items[i] != null)
                {
                    slots[i] = new Item(bagNr, i, true, items[i].GfxPath, pos, size);
                }
                else
                {
                    slots[i] = new Item(bagNr, i, true, new GfxPath(GfxType.Item, null), pos, size);
                }
            }

            children.AddRange(slots);
        }

        void MakeZero()
        {
            children.Clear();
            Resize(RelativeScreenPosition.Zero);
        }

        public void RefreshSlot(int aSlot)
        {
            slots[aSlot].AssignItem(ObjectManager.Player.Inventory.GetItemInSlot(bagNr, aSlot));
        }

        public void RefreshBag(int aSlotCount, int aColumnCount)
        {
            children.Clear();
            GetBagContent(aSlotCount, aColumnCount);
        }
    }
}
