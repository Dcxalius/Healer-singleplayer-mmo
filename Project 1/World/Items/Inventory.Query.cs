using Project_1.Items.SubTypes;
using Project_1.Messaging.Events;
using System.Diagnostics;
using System.Linq;

namespace Project_1.Items
{
    internal partial class Inventory
    {
        public int OpenSlots()
        {
            int openSlots = 0;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null) continue;
                for (int j = 0; j < items[i].Length; j++)
                {
                    if (items[i][j] == null) openSlots++;
                }
            }

            return openSlots;
        }

        public Item[] GetItemsInBox(int aIndex)
        {
            return items[aIndex];
        }

        public Item GetItemInSlot((int, int) aBagAndSlotIndex)
        {
            return items[aBagAndSlotIndex.Item1][aBagAndSlotIndex.Item2];
        }

        public Item GetItemInSlot(int aBagIndex, int aSlotIndex)
        {
            return items[aBagIndex][aSlotIndex];
        }

        int CountOfItemsInBag(int aBagSlot)
        {
            return items[aBagSlot].Count(item => item != null);
        }

        public Bag GetBag(int aSlotIndex)
        {
            Debug.Assert(aSlotIndex > 0, "Incorrect slot given");
            return bags[aSlotIndex];
        }

        public Bag[] GetBags()
        {
            return bags;
        }

        public InventoryUiSnapshot BuildUiSnapshot()
        {
            AssertSimThread();
            ItemUiSnapshot[] bagSnapshots = new ItemUiSnapshot[bagSlots];
            for (int i = 1; i < bagSlots; i++)
            {
                if (bags[i] == null) continue;
                bagSnapshots[i] = ItemUiSnapshot.FromItem(bags[i]);
            }

            ItemUiSnapshot[][] itemSnapshots = new ItemUiSnapshot[bagSlots][];
            for (int i = 0; i < bagSlots; i++)
            {
                if (items[i] == null) continue;
                itemSnapshots[i] = CloneItemArray(items[i]);
            }

            return new InventoryUiSnapshot(bagSnapshots, itemSnapshots);
        }

        static ItemUiSnapshot[] CloneItemArray(Item[] source)
        {
            if (source == null) return null;

            ItemUiSnapshot[] copy = new ItemUiSnapshot[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] == null) continue;
                copy[i] = ItemUiSnapshot.FromItem(source[i]);
            }

            return copy;
        }
    }
}
