using Project_1.Items.SubTypes;
using Project_1.Managers;
using System.Diagnostics;
using System.Linq;

namespace Project_1.Items
{
    internal partial class Inventory
    {
        public bool EquipBag(Container aBag)
        {
            AssertSimThread();
            DebugManager.Print("Depricated Method used");
            for (int i = 1; i < bags.Length; i++)
            {
                if (bags[i] != null) continue;

                bags[i] = aBag;
                items[i] = new Item[aBag.SlotCount];
                return true;
            }

            return false;
        }

        public bool EquipBag((int, int) aBagAndSlot)
        {
            AssertSimThread();
            Item slotItem = items[aBagAndSlot.Item1][aBagAndSlot.Item2];
            if (slotItem == null) return false;
            Debug.Assert(slotItem.ItemType == ItemData.ItemType.Container);
            if (slotItem.ItemType != ItemData.ItemType.Container) return false;

            Container container = slotItem as Container;
            Debug.Assert(container != null, $"Inventory slot ({aBagAndSlot.Item1},{aBagAndSlot.Item2}) has ItemType.Container but runtime type {slotItem.GetType().Name}.");
            if (container == null) return false;

            for (int i = 1; i < bags.Length; i++)
            {
                if (bags[i] != null) continue;

                bags[i] = container;
                items[i] = new Item[bags[i].SlotCount];

                items[aBagAndSlot.Item1][aBagAndSlot.Item2] = null;
                NotifySlotChanged(-1, i, this);
                NotifySlotChanged(aBagAndSlot, this);
                return true;
            }

            return false;
        }

        public void AddBag(Container aBag, int aEmptySlotToAddTo)
        {
            AssertSimThread();
            Debug.Assert(aEmptySlotToAddTo != 0, "Tried to Add a bag to default bagslot.");
            Debug.Assert(bags[aEmptySlotToAddTo] == null, "Tried to add to occupied slot.");
            bags[aEmptySlotToAddTo] = aBag;
            items[aEmptySlotToAddTo] = new Item[aBag.SlotCount];
            NotifySlotChanged(-1, aEmptySlotToAddTo, this);
        }

        public bool UnequipBag(int aBagSlot)
        {
            AssertSimThread();
            Debug.Assert(bags[aBagSlot] != null, "Tried to remove nonexistant bag.");

            if (!items[aBagSlot].All(item => item == null))
            {
                return false;
            }

            if (AddItem(bags[aBagSlot]))
            {
                bags[aBagSlot] = null;
                items[aBagSlot] = null;
                NotifySlotChanged(-1, aBagSlot, this);
            }

            return true;
        }

        public void UnequipBag(int aBag, (int, int) aInventorySlot)
        {
            AssertSimThread();
            if (items[aBag].Count(item => item == null) != bags[aBag].SlotCount) return;

            items[aInventorySlot.Item1][aInventorySlot.Item2] = bags[aBag];
            bags[aBag] = null;
            items[aBag] = null;
            NotifySlotChanged(aInventorySlot, this);
            NotifySlotChanged(-1, aBag, this);
        }

        public void RearrangeBags(int aBagSlot, int aSlotToSwapWith)
        {
            AssertSimThread();
            Item[] tempItems = items[aBagSlot];
            Container tempBag = bags[aBagSlot];
            items[aBagSlot] = items[aSlotToSwapWith];
            bags[aBagSlot] = bags[aSlotToSwapWith];
            items[aSlotToSwapWith] = tempItems;
            bags[aSlotToSwapWith] = tempBag;
            NotifySlotChanged(-1, aBagSlot, this);
            NotifySlotChanged(-1, aSlotToSwapWith, this);
        }

        public void SwapPlacesOfBags(int aBagSlot, int aSlotToSwapWith)
        {
            AssertSimThread();
            Debug.Assert(aBagSlot != aSlotToSwapWith, "tried to swap bag with iteself");
            Debug.Assert(bags[aBagSlot] != null, "originator bag was empty");
            if (bags[aSlotToSwapWith] == null)
            {
                bags[aSlotToSwapWith] = bags[aBagSlot];
                items[aSlotToSwapWith] = items[aBagSlot];

                bags[aBagSlot] = null;
                items[aBagSlot] = null;

                NotifySlotChanged(-1, aBagSlot, this);
                NotifySlotChanged(-1, aSlotToSwapWith, this);
                return;
            }

            Container tempBag = bags[aBagSlot];
            Item[] tempItems = items[aBagSlot];

            bags[aBagSlot] = bags[aSlotToSwapWith];
            items[aBagSlot] = items[aSlotToSwapWith];

            bags[aSlotToSwapWith] = tempBag;
            items[aSlotToSwapWith] = tempItems;

            NotifySlotChanged(-1, aBagSlot, this);
            NotifySlotChanged(-1, aSlotToSwapWith, this);
        }

        public void SwapBags((int, int) aSlot, int aSlotToSwapWith)
        {
            AssertSimThread();
            Item slotItem = items[aSlot.Item1][aSlot.Item2];
            if (slotItem == null) return;
            Debug.Assert(slotItem.ItemType == ItemData.ItemType.Container, "Tried to treat non bag as a bag.");
            if (slotItem.ItemType != ItemData.ItemType.Container) return;

            Container slotContainer = slotItem as Container;
            Debug.Assert(slotContainer != null, $"Inventory slot ({aSlot.Item1},{aSlot.Item2}) has ItemType.Container but runtime type {slotItem.GetType().Name}.");
            if (slotContainer == null) return;

            if (bags[aSlotToSwapWith] == null)
            {
                AddBag(slotContainer, aSlotToSwapWith);
                items[aSlot.Item1][aSlot.Item2] = null;
                NotifySlotChanged(aSlot, this);
                NotifySlotChanged(-1, aSlotToSwapWith, this);
                return;
            }

            if (slotContainer.SlotCount < CountOfItemsInBag(aSlotToSwapWith))
            {
                DebugManager.Print("Tried to swap with a bag too small.");
                return;
            }

            Container tempBag = bags[aSlotToSwapWith];
            Item[] tempItems = items[aSlotToSwapWith].Skip(tempBag.SlotCount).ToArray();
            bags[aSlotToSwapWith] = slotContainer;
            items[aSlotToSwapWith] = new Item[bags[aSlotToSwapWith].SlotCount];
            for (int i = 0; i < tempItems.Length; i++)
            {
                AddItemToInventoryFromInventory(tempItems[i], aSlotToSwapWith);
            }

            items[aSlot.Item1][aSlot.Item2] = tempBag;
            NotifySlotChanged(aSlot, this);
            NotifySlotChanged(-1, aSlotToSwapWith, this);
        }

        void AddItemToInventoryFromInventory(Item aItem, int aInventory)
        {
            AssertSimThread();
            for (int i = 0; i < items[aInventory].Length; i++)
            {
                if (items[aInventory][i] != null) continue;

                AssignItem(aItem, aInventory, i);
                return;
            }
        }
    }
}
