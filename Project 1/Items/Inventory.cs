using Newtonsoft.Json.Bson;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.Items.SubTypes;
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
        Entity owner; //TODO: Remove this dependency and move all of this into unitdata

        public int bagSlots = 4;
        public int defaultSlots = 32;

        Item[][] items;
        public Container[] bags;
        public Inventory(Entity aOwner)
        {
            owner = aOwner;


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

            if (DebugManager.Mode(DebugMode.InvCheats))
            {
                EquipBag( ItemFactory.CreateItem(ItemFactory.GetItemData("Small Bag") ) as Container);
                items[0][22] = ItemFactory.CreateItem(ItemFactory.GetItemData("Medium Bag"));
            }
        }

        public int OpenSlots() { return items.Count(item => item == null); }

        public bool ConsumeItem((int, int) aBagAndSlotIndex)
        {
            return ConsumeItem(aBagAndSlotIndex.Item1, aBagAndSlotIndex.Item2);
        }

        public bool ConsumeItem(int aBagIndex, int aSlotIndex)
        {
            Debug.Assert(items[aBagIndex][aSlotIndex].ItemType == ItemData.ItemType.Consumable, "Tried to consume nonconcumable.");

            if (!(items[aBagIndex][aSlotIndex] as Consumable).Use(owner)) return false;

            TrimStack(aBagIndex, aSlotIndex, 1);
            HUDManager.RefreshInventorySlot(aBagIndex, aSlotIndex);
            return true;
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
                    HUDManager.RefreshInventorySlot(-1, i);
                    HUDManager.RefreshInventorySlot(aBagAndSlot);


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
            HUDManager.RefreshInventorySlot(-1, aEmptySlotToAddTo);
        }

        public bool UnequipBag(int aBagSlot)
        {
            Debug.Assert(bags[aBagSlot] != null, "Tried to remove nonexistant bag.");

            if (items[aBagSlot].All(item => item == null))
            {
                if (AddItem(bags[aBagSlot]))
                {
                    bags[aBagSlot] = null;
                    items[aBagSlot] = null;
                    HUDManager.RefreshInventorySlot(-1, aBagSlot);
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
            HUDManager.RefreshInventorySlot(aInventorySlot);
            HUDManager.RefreshInventorySlot(-1, aBag);

        }

        public void RearrangeBags(int aBagSlot, int aSlotToSwapWith)
        {
            Item[] tempItems = items[aBagSlot];
            Container tempBag = bags[aBagSlot];
            items[aBagSlot] = items[aSlotToSwapWith];
            bags[aBagSlot] = bags[aSlotToSwapWith];
            items[aSlotToSwapWith] = tempItems;
            bags[aSlotToSwapWith] = tempBag;
            HUDManager.RefreshInventorySlot(-1, aBagSlot);
            HUDManager.RefreshInventorySlot(-1, aSlotToSwapWith);
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

                HUDManager.RefreshInventorySlot(-1, aBagSlot);
                HUDManager.RefreshInventorySlot(-1, aSlotToSwapWith);
                return;
            }

            Container tempBag = bags[aBagSlot];
            Item[] tempItems = items[aBagSlot];

            bags[aBagSlot] = bags[aSlotToSwapWith];
            items[aBagSlot] = items[aSlotToSwapWith];

            bags[aSlotToSwapWith] = tempBag;
            items[aSlotToSwapWith] = tempItems;

            HUDManager.RefreshInventorySlot(-1, aBagSlot);
            HUDManager.RefreshInventorySlot(-1, aSlotToSwapWith);
        }

        public void SwapBags((int, int) aSlot, int aSlotToSwapWith)
        {
            Debug.Assert(items[aSlot.Item1][aSlot.Item2].ItemType == ItemData.ItemType.Container, "Tried to treat non bag as a bag.");


            if (bags[aSlotToSwapWith] == null)
            {
                //Bag to empty
                AddBag(items[aSlot.Item1][aSlot.Item2] as Container, aSlotToSwapWith);
                items[aSlot.Item1][aSlot.Item2] = null;
                HUDManager.RefreshInventorySlot(aSlot);
                HUDManager.RefreshInventorySlot(-1, aSlotToSwapWith);
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
            HUDManager.RefreshInventorySlot(aSlot);
            HUDManager.RefreshInventorySlot(-1, aSlotToSwapWith);
        }


        int CountOfItemsInBag(int aBagSlot)
        {
            return items[aBagSlot].Where(item => item != null).Count();
        }

        public void LootItem(int aLootIndex)
        {
            Item item = HUDManager.GetLootItem(aLootIndex);
            for (int i = 0; i < items.Length; i++)
            {
                for (int j = 0; j < items[i].Length; j++)
                {
                    if (items[i][j] == null)
                    {
                        items[i][j] = item;
                        HUDManager.ReduceLootItem(aLootIndex, item.Count);
                        HUDManager.RefreshInventorySlot(i, j);
                        return;
                    }

                    if (items[i][j].ID != item.ID) continue;

                    if (items[i][j].Count + item.Count <= item.MaxStack)
                    {
                        items[i][j].Count += item.Count;
                        HUDManager.ReduceLootItem(aLootIndex, item.Count);
                        HUDManager.RefreshInventorySlot(i, j);
                        return;
                    }

                    int c = item.MaxStack - items[i][j].Count;
                    items[i][j].Count = item.MaxStack;
                    HUDManager.ReduceLootItem(aLootIndex, c);
                    HUDManager.RefreshInventorySlot(i, j);
                }
            }
        }

        public void LootItem(int aLootIndex, (int, int) aBagAndSlot)
        {
            Item item = HUDManager.GetLootItem(aLootIndex);
            if (items[aBagAndSlot.Item1][aBagAndSlot.Item2] == null)
            {
                items[aBagAndSlot.Item1][aBagAndSlot.Item2] = item;
                HUDManager.ReduceLootItem(aLootIndex, item.Count);
                HUDManager.RefreshInventorySlot(aBagAndSlot);
                return;
            }
            if (item.ID != items[aBagAndSlot.Item1][aBagAndSlot.Item2].ID) return;
            

            if (items[aBagAndSlot.Item1][aBagAndSlot.Item2].Count + item.Count <= item.MaxStack)
            {
                items[aBagAndSlot.Item1][aBagAndSlot.Item2].Count += item.Count;
                HUDManager.ReduceLootItem(aLootIndex, item.Count);
                HUDManager.RefreshInventorySlot(aBagAndSlot);
                return;
            }

            int c = item.MaxStack - items[aBagAndSlot.Item1][aBagAndSlot.Item2].Count;
            items[aBagAndSlot.Item1][aBagAndSlot.Item2].Count = item.MaxStack;
            HUDManager.ReduceLootItem(aLootIndex, c);
            HUDManager.RefreshInventorySlot(aBagAndSlot);

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
                        HUDManager.RefreshInventorySlot(i, j);
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
                            HUDManager.RefreshInventorySlot(i, j);
                        }
                        else
                        {
                            items[i][j].Count = count;
                            HUDManager.RefreshInventorySlot(i, j);
                            return true;
                        }
                    }
                }
            }
            aItem.Count = count;
            return false;
        }

        public void AddItem(Item aItem, int aBagIndex, int aSlotIndex)
        {
            Debug.Assert(aItem != null);
            Debug.Assert(GetItemInSlot(aBagIndex, aSlotIndex) == null);

            AssignItem(aItem, aBagIndex, aSlotIndex);
        }

        public void AddItem(Item aItem, (int, int) aBagAndSlotIndex)
        {
            AddItem(aItem, aBagAndSlotIndex.Item1, aBagAndSlotIndex.Item2);
        }

        public void SwapItems((int, int) aSlot, (int, int) aSlotToSwapWith)
        {
            if (items[aSlotToSwapWith.Item1][aSlotToSwapWith.Item2] == null)
            {
                items[aSlotToSwapWith.Item1][aSlotToSwapWith.Item2] = items[aSlot.Item1][aSlot.Item2];
                items[aSlot.Item1][aSlot.Item2] = null;
                HUDManager.RefreshInventorySlot(aSlot.Item1, aSlot.Item2);
                HUDManager.RefreshInventorySlot(aSlotToSwapWith.Item1, aSlotToSwapWith.Item2);
                return;
            }

            if (items[aSlot.Item1][aSlot.Item2].ID == items[aSlotToSwapWith.Item1][aSlotToSwapWith.Item2].ID) //If same item, stack them
            {
                int total = items[aSlot.Item1][aSlot.Item2].Count + items[aSlotToSwapWith.Item1][aSlotToSwapWith.Item2].Count;
                if (total > items[aSlot.Item1][aSlot.Item2].MaxStack)
                {
                    items[aSlot.Item1][aSlot.Item2].Count = items[aSlot.Item1][aSlot.Item2].MaxStack;
                    items[aSlotToSwapWith.Item1][aSlotToSwapWith.Item2].Count = total - items[aSlot.Item1][aSlot.Item2].MaxStack;
                }
                else
                {
                    items[aSlotToSwapWith.Item1][aSlotToSwapWith.Item2] = null;
                    items[aSlot.Item1][aSlot.Item2].Count = total; //Swap these
                }
            }
            else //Actually swap them
            {
                Item tempItem = items[aSlot.Item1][aSlot.Item2];
                items[aSlot.Item1][aSlot.Item2] = items[aSlotToSwapWith.Item1][aSlotToSwapWith.Item2];
                items[aSlotToSwapWith.Item1][aSlotToSwapWith.Item2] = tempItem;
            }

            HUDManager.RefreshInventorySlot(aSlot.Item1, aSlot.Item2);
            HUDManager.RefreshInventorySlot(aSlotToSwapWith.Item1, aSlotToSwapWith.Item2);
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

        public void TrimStack(int aBagIndex, int aSlotIndex, int aCount)
        {

            Debug.Assert(aCount <= items[aBagIndex][aSlotIndex].Count, "Tried to remove to much from item");
            items[aBagIndex][aSlotIndex].Count -= aCount;
            if (items[aBagIndex][aSlotIndex].Count == 0)
            {
                items[aBagIndex][aSlotIndex] = null;
            }
            HUDManager.RefreshInventorySlot(aBagIndex, aSlotIndex);
        }

        public void TrimStack((int, int) aBagAndSlotIndex, int aCount)
        {
            TrimStack(aBagAndSlotIndex.Item1, aBagAndSlotIndex.Item2, aCount);
        }

        public bool DestroyItem(Item aItem)
        {
            for (int i = 0; i < items.Length; i++)
            {
                for (int j = 0; j < items[i].Length; j++)
                {
                    if (items[i][j] == aItem)
                    {
                        AssignItem(null, i, j);
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
                    AssignItem(aItem, aInventory, i);
                    return;
                }
            }
        }

        public Container GetBag(int aSlotIndex)
        {
            Debug.Assert(aSlotIndex > 0, "Incorrect slot given");
            return bags[aSlotIndex];
        }

        public Container[] GetBags()
        {
            return bags;
        }

        internal void Equip((int, int) aIndex)
        {
            Item item = items[aIndex.Item1][aIndex.Item2];

            if (item == null) return;
            if (!(item.ItemType == ItemData.ItemType.Equipment || item.ItemType == ItemData.ItemType.Weapon)) return;

            Equipment equipment = item as Equipment;
            GameObjects.Unit.Equipment wearing = owner.Equipment;

            if (equipment.type == Equipment.Type.TwoHander) 
            {
                if (OpenSlots() < 1 && wearing.EquipedInSlot(GameObjects.Unit.Equipment.Slot.MainHand) != null && wearing.EquipedInSlot(GameObjects.Unit.Equipment.Slot.OffHand) != null) return;

                (Item, Item) equipedInSlots = owner.EquipTwoHander(equipment);
                AssignItem(equipedInSlots.Item1, aIndex);
                if (equipedInSlots.Item2 == null) return;
                AddItem(equipedInSlots.Item2);
                return;
            }

            Item equipedInSlot = owner.Equip(equipment);
            AssignItem(equipedInSlot, aIndex);
        }

        internal void SwapEquipment((int, int) aIndex, int aEquipmentSlot)
        {
            Item item = items[aIndex.Item1][aIndex.Item2];

            if (item == null) return;
            if (!(item.ItemType == ItemData.ItemType.Equipment || item.ItemType == ItemData.ItemType.Weapon)) return;

            Equipment equipment = item as Equipment;
            GameObjects.Unit.Equipment wearing = owner.Equipment;

            if(equipment.type == Equipment.Type.TwoHander)
            {
                if (!(GameObjects.Unit.Equipment.Slot.MainHand == (GameObjects.Unit.Equipment.Slot)aEquipmentSlot || GameObjects.Unit.Equipment.Slot.OffHand == (GameObjects.Unit.Equipment.Slot)aEquipmentSlot)) return;
                Equip(aIndex);
                return;
            }

            if (equipment.type < Equipment.Type.Trinket || equipment.type >= Equipment.Type.MainHander)
            {
                if (GameObjects.Unit.Equipment.SlotToSlot(equipment.type) != (GameObjects.Unit.Equipment.Slot)aEquipmentSlot) return;
                Equip(aIndex);
                return;
            }

            //Only things here should be Trinkets, rings and one handers
            Item equipedInSlot = owner.EquipInParticularSlot(equipment, (GameObjects.Unit.Equipment.Slot)aEquipmentSlot);
            AssignItem(equipedInSlot, aIndex);
        }

        public void AssignItem(Item item, (int, int) aBagAndSlotIndex)
        {
            AssignItem(item, aBagAndSlotIndex.Item1, aBagAndSlotIndex.Item2);
        }
        
        public void AssignItem(Item aItem, int aBagIndex, int aSlotIndex)
        {
            items[aBagIndex][aSlotIndex] = aItem;
            HUDManager.RefreshInventorySlot(aBagIndex, aSlotIndex);
        }
    }
}
