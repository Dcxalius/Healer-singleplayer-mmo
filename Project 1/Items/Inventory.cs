using Newtonsoft.Json.Bson;
using Project_1.Managers;
using Project_1.UI.HUD;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items
{
    internal class Inventory
    {
        public int bagSlots = 4;
        public int defaultSlots = 32;

        Item[][] items;
        public Container[] bags;
        public Inventory()
        {
            bags = new Container[bagSlots + 1]; //Bag 0 is fornow always null
            items = new Item[bagSlots + 1][];
            items[0] = new Item[defaultSlots];
            for (int i = 1; i < bagSlots; i++)
            {
                if (bags[i] != null)
                {
                    items[i] = new Item[bags[i].SlotCount];
                }
            }

            if (DebugManager.mode == DebugMode.On)
            {
                EquipBag( ItemFactory.CreateItem(ItemFactory.GetItemData("Small Bag") ) as Container);
                items[0][22] = ItemFactory.CreateItem(ItemFactory.GetItemData("Medium Bag"));
            }
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

        public bool EquipBag(Container aBag)
        {
            DebugManager.Print(GetType(), "Depricated Method used");
            for (int i = 1; i < bags.Length; i++)
            {
                if (bags[i] == null)
                {
                    bags[i] = aBag;
                    items[i] = new Item[aBag.SlotCount];
                    return true;
                }
            }

            return false;
        }

        public bool EquipBag((int, int) aBagAndSlot)
        {
            Debug.Assert(items[aBagAndSlot.Item1][aBagAndSlot.Item2].ItemType == ItemData.ItemType.Container);
            for (int i = 1; i < bags.Length; i++)
            {
                if (bags[i] == null)
                {
                    bags[i] = items[aBagAndSlot.Item1][aBagAndSlot.Item2] as Container;
                    items[i] = new Item[bags[i].SlotCount];

                    items[aBagAndSlot.Item1][aBagAndSlot.Item2] = null;
                    HUDManager.RefreshSlot(aBagAndSlot);
                    HUDManager.RefreshSlot(-1, i);


                    return true;
                }
            }

            return false;
        }

        public void AddBag(Container aBag, int aEmptySlotToAddTo)
        {
            Debug.Assert(aEmptySlotToAddTo != 0, "Tried to Add a bag to default bagslot.");
            Debug.Assert(bags[aEmptySlotToAddTo] == null, "Tried to add to occupied slot.");
            bags[aEmptySlotToAddTo] = aBag;
            items[aEmptySlotToAddTo] = new Item[aBag.SlotCount];
            HUDManager.RefreshSlot(-1, aEmptySlotToAddTo);
        }

        public bool RemoveBag(int aBagSlot)
        {
            Debug.Assert(bags[aBagSlot] != null, "Tried to remove nonexistant bag.");

            if (items[aBagSlot].All(item => item == null))
            {
                if (AddItem(bags[aBagSlot]))
                {
                    bags[aBagSlot] = null;
                    items[aBagSlot] = null;
                    HUDManager.RefreshSlot(-1, aBagSlot);
                }
                return true;
            }


            return false;
        }

        public void UnequipBag(int aBag, (int, int) aInventorySlot)
        {
            if (items[aBag].Where(item => item == null).Count() != bags[aBag].SlotCount) return;
            items[aInventorySlot.Item1][aInventorySlot.Item2] = bags[aBag];
            bags[aBag] = null;
            items[aBag] = null;
            HUDManager.RefreshSlot(aInventorySlot);
            HUDManager.RefreshSlot(-1, aBag);

        }

        public void RearrangeBags(int aBagSlot, int aSlotToSwapWith)
        {
            Item[] tempItems = items[aBagSlot];
            Container tempBag = bags[aBagSlot];
            items[aBagSlot] = items[aSlotToSwapWith];
            bags[aBagSlot] = bags[aSlotToSwapWith];
            items[aSlotToSwapWith] = tempItems;
            bags[aSlotToSwapWith] = tempBag;
            HUDManager.RefreshSlot(-1, aBagSlot);
            HUDManager.RefreshSlot(-1, aSlotToSwapWith);
        }

        public void SwapPlacesOfBags(int aBagSlot, int aSlotToSwapWith)
        {
            Debug.Assert(aBagSlot != aSlotToSwapWith, "tried to swap bag with iteself");
            Debug.Assert(bags[aBagSlot] != null, "originator bag was empty");
            if (bags[aSlotToSwapWith] == null)
            {
                //Bag to empty
                bags[aSlotToSwapWith] = bags[aBagSlot];
                items[aSlotToSwapWith] = items[aBagSlot];

                bags[aBagSlot] = null;
                items[aBagSlot] = null;

                HUDManager.RefreshSlot(-1, aBagSlot);
                HUDManager.RefreshSlot(-1, aSlotToSwapWith);
                return;
            }

            Container tempBag = bags[aBagSlot];
            Item[] tempItems = items[aBagSlot];

            bags[aBagSlot] = bags[aSlotToSwapWith];
            items[aBagSlot] = items[aSlotToSwapWith];

            bags[aSlotToSwapWith] = tempBag;
            items[aSlotToSwapWith] = tempItems;

            HUDManager.RefreshSlot(-1, aBagSlot);
            HUDManager.RefreshSlot(-1, aSlotToSwapWith);
        }

        public void SwapBags((int, int) aSlot, int aSlotToSwapWith)
        {
            Debug.Assert(items[aSlot.Item1][aSlot.Item2].ItemType == ItemData.ItemType.Container, "Tried to treat non bag as a bag.");


            if (bags[aSlotToSwapWith] == null)
            {
                //Bag to empty
                AddBag(items[aSlot.Item1][aSlot.Item2] as Container, aSlotToSwapWith);
                items[aSlot.Item1][aSlot.Item2] = null;
                HUDManager.RefreshSlot(aSlot);
                HUDManager.RefreshSlot(-1, aSlotToSwapWith);
                return;
            }

            if ((items[aSlot.Item1][aSlot.Item2] as Container).SlotCount < CountOfItemsInBag(aSlotToSwapWith))
            {
                DebugManager.Print(GetType(), "Tried to swap with a bag too small.");
                return;
            }

            //Bag in bag to bag in rack
            Container tempBag = bags[aSlotToSwapWith];
            Item[] tempItems = items[aSlotToSwapWith].Skip(tempBag.SlotCount).ToArray();
            bags[aSlotToSwapWith] = items[aSlot.Item1][aSlot.Item2] as Container;
            items[aSlotToSwapWith] = new Item[bags[aSlotToSwapWith].SlotCount];
            for (int i = 0; i < tempItems.Length; i++)
            {
                AddItemToInventoryFromInventory(tempItems[i], aSlotToSwapWith);

            }
            items[aSlot.Item1][aSlot.Item2] = tempBag;
            HUDManager.RefreshSlot(aSlot);
            HUDManager.RefreshSlot(-1, aSlotToSwapWith);
        }


        int CountOfItemsInBag(int aBagSlot)
        {
            return items[aBagSlot].Where(item => item != null).Count();
        }

        public void LootItem(int aLootIndex, (int, int) aBagAndSlot)
        {
            Item item = HUDManager.LootItem(aLootIndex);
            if (items[aBagAndSlot.Item1][aBagAndSlot.Item2] == null)
            {
                items[aBagAndSlot.Item1][aBagAndSlot.Item2] = item;
                HUDManager.ReduceLootItem(aLootIndex, item.Count);
                HUDManager.RefreshSlot(aBagAndSlot);
                return;
            }
            if (item.ID != items[aBagAndSlot.Item1][aBagAndSlot.Item2].ID) return;
            

            if (items[aBagAndSlot.Item1][aBagAndSlot.Item2].Count + item.Count <= item.MaxStack)
            {
                items[aBagAndSlot.Item1][aBagAndSlot.Item2].Count += item.Count;
                HUDManager.ReduceLootItem(aLootIndex, item.Count);
                HUDManager.RefreshSlot(aBagAndSlot);
                return;
            }

            int c = item.MaxStack - items[aBagAndSlot.Item1][aBagAndSlot.Item2].Count;
            items[aBagAndSlot.Item1][aBagAndSlot.Item2].Count = item.MaxStack;
            HUDManager.ReduceLootItem(aLootIndex, c);
            HUDManager.RefreshSlot(aBagAndSlot);

        }

        public bool AddItem(Item aItem)
        {
            int count = aItem.Count;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null) continue;
                for (int j = 0; j < items[i].Length; j++)
                {
                    if (items[i][j] == null) continue;
                    if (items[i][j].ID == aItem.ID)
                    {
                        int tempCount = items[i][j].AddToStack(count);
                        HUDManager.RefreshSlot(i, j);
                        if (tempCount == 0)
                        {
                            return true;
                        }

                        count = tempCount;
                    }

                    
                }
                
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null) continue;
                for (int j = 0; j < items[i].Length; j++)
                {
                    if (items[i][j] == null)
                    {
                        items[i][j] = aItem;
                        if (aItem.MaxStack < count)
                        {
                            items[i][j].Count = aItem.MaxStack;
                            count -= aItem.MaxStack;
                            HUDManager.RefreshSlot(i, j);
                        }
                        else
                        {
                            items[i][j].Count = count;
                            HUDManager.RefreshSlot(i, j);
                            return true;
                        }
                    }
                }
            }
            aItem.Count = count;
            return false;
        }

        public void SwapItems((int, int) aSlot, (int, int) aSlotToSwapWith)
        {
            if (items[aSlot.Item1][aSlot.Item2] == items[aSlotToSwapWith.Item1][aSlotToSwapWith.Item2])
            {
                //TODO: Merge
            }
            else
            {
                Item tempItem = items[aSlot.Item1][aSlot.Item2];
                items[aSlot.Item1][aSlot.Item2] = items[aSlotToSwapWith.Item1][aSlotToSwapWith.Item2];
                items[aSlotToSwapWith.Item1][aSlotToSwapWith.Item2] = tempItem;
            }

            HUDManager.RefreshSlot(aSlot.Item1, aSlot.Item2);
            HUDManager.RefreshSlot(aSlotToSwapWith.Item1, aSlotToSwapWith.Item2);
        }

        public bool RemoveItem(Item aItem, int aCountToRemove)
        {
            int count = aCountToRemove;
            List<(int, int)> itemsToRemoveFrom = new List<(int, int)>();
            for (int i = 0; i < items.GetLength(0); i++)
            {
                for (int j = 0; j < items.GetLength(1); j++)
                {
                    if (items[i][j].ID == aItem.ID)
                    {
                        if (count > items[i][j].Count)
                        {
                            count -= items[i][j].Count;
                            itemsToRemoveFrom.Add((i, j));
                        }

                        if (count <= items[i][j].Count)
                        {
                            TrimStack((i, j), count);

                            for (int k = 0; k < itemsToRemoveFrom.Count; k++)
                            {
                                (int, int) couple = itemsToRemoveFrom[k];
                                TrimStack(couple, items[couple.Item1][couple.Item2].Count);                            
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void TrimStack((int, int) aSlot, int aCount)
        {
            Debug.Assert(aCount <= items[aSlot.Item1][aSlot.Item2].Count, "Tried to remove to much from item");
            items[aSlot.Item1][aSlot.Item2].Count -= aCount;
            if (items[aSlot.Item1][aSlot.Item2].Count == 0)
            {
                items[aSlot.Item1][aSlot.Item2] = null;
                HUDManager.RefreshSlot(aSlot);
            }
        }

        public bool DestroyItem(Item aItem)
        {
            for (int i = 0; i < items.Length; i++)
            {
                for (int j = 0; j < items[i].Length; j++)
                {
                    if (items[i][j] == aItem)
                    {
                        items[i][j] = null;
                        HUDManager.RefreshSlot(i, j);
                        return true;
                    }
                }
            }

            DebugManager.Print(GetType(), "Tried to destoy item but couldnt find it.");
            return false;
        }

        void AddItemToInventoryFromInventory(Item aItem, int aInventory)
        {
            for (int i = 0; i < items[aInventory].Length; i++)
            {
                if (items[aInventory][i] == null)
                {
                    items[aInventory][i] = aItem;
                    HUDManager.RefreshSlot(aInventory, i);
                    return;
                }
            }
        }

        public Container[] GetBags()
        {
            return bags;
        }
    }
}
