using Newtonsoft.Json.Bson;
using Project_1.Managers;
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
        public Bag[] bags;
        public Inventory()
        {
            bags = new Bag[bagSlots];
            items = new Item[bagSlots + 1][];
            items[0] = new Item[defaultSlots];
            for (int i = 0; i < items.GetLength(0) - 1; i++)
            {
                if (bags[i] != null)
                {
                    items[i + 1] = new Item[bags[i].SlotCount];
                }
            }

            if (DebugManager.mode == DebugMode.On)
            {
                bags[3] = new Bag(ItemFactory.GetItemData(0));
            }
        }

        

        public bool EquipBag(Bag aBag)
        {
            for (int i = 0; i < bags.Length; i++)
            {
                if (bags[i] == null)
                {
                    bags[i] = aBag;
                    items[i + 1] = new Item[aBag.SlotCount];
                    return true;
                }
            }

            return false;
        }

        public void AddBag(Bag aBag, int aEmptySlotToAddTo)
        {
            Debug.Assert(bags[aEmptySlotToAddTo] == null, "Tried to add to occupied slot.");
            bags[aEmptySlotToAddTo] = aBag;
        }

        public Bag RemoveBag(int aBagSlot)
        {
            Debug.Assert(bags[aBagSlot] != null, "Tried to remove nonexistant bag.");

            if (items[aBagSlot + 1].All(item => item == null))
            {
                return bags[aBagSlot];
            }

            return null;
        }

        public void RearrangeBags(int aBagSlot, int aSlotToSwapWith)
        {
            Item[] tempItems = items[aBagSlot + 1];
            Bag tempBag = bags[aBagSlot];
            items[aBagSlot + 1] = items[aSlotToSwapWith + 1];
            bags[aBagSlot] = bags[aSlotToSwapWith];
            items[aSlotToSwapWith + 1] = tempItems;
            bags[aSlotToSwapWith] = tempBag;
        }

        public Bag SwapBags(Bag aBag, int aSlotToSwapWith)
        {
            if (bags[aSlotToSwapWith] == null)
            {
                AddBag(aBag, aSlotToSwapWith);
                return null;
            }

            if (aBag.SlotCount < CountOfItemsInBag(aSlotToSwapWith))
            {
                DebugManager.Print(GetType(), "Tried to swap with a bag too small.");
                return aBag;
            }

            Bag tempBag = bags[aSlotToSwapWith];
            Item[] tempItems = (Item[])items[aSlotToSwapWith + 1].Skip(tempBag.SlotCount);
            bags[aSlotToSwapWith] = aBag;
            for (int i = 0; i < tempItems.Length; i++)
            {
                AddItemToInventoryFromInventory(tempItems[i], aSlotToSwapWith + 1);

            }

            return tempBag;
        }

        int CountOfItemsInBag(int aBagSlot)
        {
            return items[aBagSlot + 1].Where(item => item != null).Count();
        }

        public bool AddItem(Item aItem)
        {
            int count = aItem.Count;
            for (int i = 0; i < items.GetLength(0); i++)
            {
                for (int j = 0; j < items.GetLength(1); j++)
                {
                    if (items[i][j].ID == aItem.ID)
                    {
                        int tempCount = items[i][j].AddToStack(count);
                        if (tempCount == 0)
                        {
                            return true;
                        }

                        count = tempCount;
                    }

                    
                }
                
            }

            for (int i = 0; i < items.GetLength(0); i++)
            {
                for (int j = 0; j < items.GetLength(1); j++)
                {
                    if (items[i][j] == null)
                    {
                        items[i][j] = aItem;
                        if (aItem.MaxStack > count)
                        {
                            items[i][j].Count = aItem.MaxStack;
                            count -= aItem.MaxStack;
                        }
                        else
                        {
                            items[i][j].Count = count;
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
            Item tempItem = items[aSlot.Item1][aSlot.Item2];
            items[aSlot.Item1][aSlot.Item2] = items[aSlotToSwapWith.Item1][aSlotToSwapWith.Item2];
            items[aSlotToSwapWith.Item1][aSlotToSwapWith.Item2] = tempItem;
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

        public void TrimStack((int, int) xdd, int aCount)
        {
            Debug.Assert(aCount <= items[xdd.Item1][xdd.Item2].Count, "Tried to remove to much from item");
            items[xdd.Item1][xdd.Item2].Count -= aCount;
            if (items[xdd.Item1][xdd.Item2].Count == 0)
            {
                items[xdd.Item1][xdd.Item2] = null;
            }
        }

        public bool DestroyItem(Item aItem)
        {
            for (int i = 0; i < items.GetLength(0); i++)
            {
                for (int j = 0; j < items.GetLength(1); j++)
                {
                    if (items[i][j] == aItem)
                    {
                        items[i][j] = null;
                        return true;
                    }
                }
            }

            DebugManager.Print(GetType(), "Tried to destoy item but couldnt find it.");
            return false;
        }

        void AddItemToInventoryFromInventory(Item aItem, int aInventory) //If adding to bag make sure to add + 1 to offset default bag
        {
            for (int i = 0; i < items.GetLength(1); i++)
            {
                if (items[aInventory][i] == null)
                {
                    items[aInventory][i] = aItem;
                }
            }
        }

        public Bag[] GetBags()
        {
            return bags;
        }
    }
}
