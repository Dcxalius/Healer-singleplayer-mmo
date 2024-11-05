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
        public Bag[] bags;
        public Inventory()
        {
            bags = new Bag[bagSlots];
            items = new Item[bagSlots + 1][];
            items[0] = new Item[defaultSlots];
            for (int i = 0; i < bagSlots; i++)
            {
                if (bags[i] != null)
                {
                    items[i + 1] = new Item[bags[i].SlotCount];
                }
            }

            if (DebugManager.mode == DebugMode.On)
            {
                EquipBag( new Bag(ItemFactory.GetItemData(0)));
                items[0][22] = new Bag(ItemFactory.GetItemData(2));
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

        public bool EquipBag(Bag aBag)
        {
            DebugManager.Print(GetType(), "Depricated Method used");
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
            items[aEmptySlotToAddTo + 1] = new Item[aBag.SlotCount];
            HUDManager.RefreshSlot(-1, aEmptySlotToAddTo);
        }

        public Bag RemoveBag(int aBagSlot)
        {
            Debug.Assert(bags[aBagSlot] != null, "Tried to remove nonexistant bag.");

            if (items[aBagSlot + 1].All(item => item == null))
            {
                Bag returnBag = bags[aBagSlot];
                bags[aBagSlot] = null;
                HUDManager.RefreshSlot(-1, aBagSlot);
                return returnBag;
            }


            return null;
        }

        public void UnequipBag(int aBag, (int, int) aInventorySlot)
        {
            if (items[aBag + 1].Where(item => item == null).Count() != bags[aBag].SlotCount) return;
            items[aInventorySlot.Item1][aInventorySlot.Item2] = bags[aBag];
            bags[aBag] = null;
            items[aBag + 1] = null;
            HUDManager.RefreshSlot(aInventorySlot);
            HUDManager.RefreshSlot(-1, aBag);

        }

        public void RearrangeBags(int aBagSlot, int aSlotToSwapWith)
        {
            Item[] tempItems = items[aBagSlot + 1];
            Bag tempBag = bags[aBagSlot];
            items[aBagSlot + 1] = items[aSlotToSwapWith + 1];
            bags[aBagSlot] = bags[aSlotToSwapWith];
            items[aSlotToSwapWith + 1] = tempItems;
            bags[aSlotToSwapWith] = tempBag;
            HUDManager.RefreshSlot(-1, aBagSlot);
            HUDManager.RefreshSlot(-1, aSlotToSwapWith);
        }

        public void SwapPlacesOfBags(int aBagSlot, int aSlotToSwapWith)
        {
            Debug.Assert(aBagSlot != aSlotToSwapWith && bags[aBagSlot] != null, "xdd");
            if (bags[aSlotToSwapWith] == null)
            {
                //Bag to empty
                bags[aSlotToSwapWith] = bags[aBagSlot];
                items[aSlotToSwapWith + 1] = items[aBagSlot + 1];

                HUDManager.RefreshSlot(-1, aBagSlot);
                HUDManager.RefreshSlot(-1, aSlotToSwapWith);
                return;
            }

            Bag tempBag = bags[aBagSlot];
            Item[] tempItems = items[aBagSlot + 1];

            bags[aBagSlot] = bags[aSlotToSwapWith];
            items[aBagSlot + 1] = items[aSlotToSwapWith + 1];

            bags[aSlotToSwapWith] = tempBag;
            items[aSlotToSwapWith] = tempItems;

            HUDManager.RefreshSlot(-1, aBagSlot);
            HUDManager.RefreshSlot(-1, aSlotToSwapWith);
        }

        public void SwapBags((int, int) aSlot,  int aSlotToSwapWith)
        {
            Debug.Assert(items[aSlot.Item1][aSlot.Item2].GetType() == typeof(Bag), "Tried to treat non bag as a bag.");
           

            if (bags[aSlotToSwapWith] == null)
            {
                //Bag to empty
                AddBag(items[aSlot.Item1][aSlot.Item2] as Bag, aSlotToSwapWith);
                items[aSlot.Item1][aSlot.Item2] = null;
                HUDManager.RefreshSlot(aSlot);
                HUDManager.RefreshSlot(-1, aSlotToSwapWith);
                return;
            }

            if ((items[aSlot.Item1][aSlot.Item2] as Bag).SlotCount < CountOfItemsInBag(aSlotToSwapWith))
            {
                DebugManager.Print(GetType(), "Tried to swap with a bag too small.");
                return;
            }

            //Bag in bag to bag in rack
            Bag tempBag = bags[aSlotToSwapWith];
            Item[] tempItems = (Item[])items[aSlotToSwapWith + 1].Skip(tempBag.SlotCount);
            bags[aSlotToSwapWith] = bags[aSlot.Item2];
            for (int i = 0; i < tempItems.Length; i++)
            {
                AddItemToInventoryFromInventory(tempItems[i], aSlotToSwapWith + 1);

            }
        }


        int CountOfItemsInBag(int aBagSlot)
        {
            return items[aBagSlot + 1].Where(item => item != null).Count();
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
            Item tempItem = items[aSlot.Item1][aSlot.Item2];
            items[aSlot.Item1][aSlot.Item2] = items[aSlotToSwapWith.Item1][aSlotToSwapWith.Item2];
            items[aSlotToSwapWith.Item1][aSlotToSwapWith.Item2] = tempItem;
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

        void AddItemToInventoryFromInventory(Item aItem, int aInventory) //If adding to bag make sure to add + 1 to offset default bag
        {
            for (int i = 0; i < items[aInventory].Length; i++)
            {
                if (items[aInventory][i] == null)
                {
                    items[aInventory][i] = aItem;
                    HUDManager.RefreshSlot(aInventory, i);
                }
            }
        }

        public Bag[] GetBags()
        {
            return bags;
        }
    }
}
