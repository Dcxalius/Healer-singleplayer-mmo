using Project_1.Managers;
using System.Collections.Generic;
using System.Diagnostics;

namespace Project_1.Items
{
    internal partial class Inventory
    {
        public bool AddItem(Item aItem)
        {
            AssertSimThread();
            if (aItem == null)
            {
                return false;
            }

            int remainingCount = aItem.Count;
            for (int i = 0; i < items.Length; i++)
            {
                if (aItem.MaxStack == 1) break;
                if (items[i] == null) continue;
                for (int j = 0; j < items[i].Length; j++)
                {
                    Item slotItem = items[i][j];
                    if (slotItem == null) continue;
                    if (slotItem.ID != aItem.ID) continue;

                    int overflowCount = slotItem.AddToStack(remainingCount);
                    NotifySlotChanged(i, j, this);
                    if (overflowCount == 0)
                    {
                        return true;
                    }

                    remainingCount = overflowCount;
                }
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null) continue;
                for (int j = 0; j < items[i].Length; j++)
                {
                    if (items[i][j] != null) continue;

                    items[i][j] = aItem;
                    if (aItem.MaxStack < remainingCount)
                    {
                        items[i][j].Count = aItem.MaxStack;
                        remainingCount -= aItem.MaxStack;
                        NotifySlotChanged(i, j, this);
                    }
                    else
                    {
                        items[i][j].Count = remainingCount;
                        NotifySlotChanged(i, j, this);
                        return true;
                    }
                }
            }

            aItem.Count = remainingCount;
            return false;
        }

        public void AddItem(Item aItem, int aBagIndex, int aSlotIndex)
        {
            AssertSimThread();
            Debug.Assert(aItem != null);
            Debug.Assert(GetItemInSlot(aBagIndex, aSlotIndex) == null);
            AssignItem(aItem, aBagIndex, aSlotIndex);
        }

        public void AddItem(Item aItem, (int, int) aBagAndSlotIndex)
        {
            AssertSimThread();
            AddItem(aItem, aBagAndSlotIndex.Item1, aBagAndSlotIndex.Item2);
        }

        public void SwapItems((int, int) aSlot, (int, int) aSlotToSwapWith)
        {
            AssertSimThread();
            Item sourceItem = items[aSlot.Item1][aSlot.Item2];
            if (sourceItem == null)
            {
                return;
            }

            if (items[aSlotToSwapWith.Item1][aSlotToSwapWith.Item2] == null)
            {
                items[aSlotToSwapWith.Item1][aSlotToSwapWith.Item2] = sourceItem;
                items[aSlot.Item1][aSlot.Item2] = null;
                NotifySlotChanged(aSlot.Item1, aSlot.Item2, this);
                NotifySlotChanged(aSlotToSwapWith.Item1, aSlotToSwapWith.Item2, this);
                return;
            }

            Item targetItem = items[aSlotToSwapWith.Item1][aSlotToSwapWith.Item2];
            if (sourceItem.ID == targetItem.ID)
            {
                int total = sourceItem.Count + targetItem.Count;
                if (total > sourceItem.MaxStack)
                {
                    sourceItem.Count = sourceItem.MaxStack;
                    targetItem.Count = total - sourceItem.MaxStack;
                }
                else
                {
                    items[aSlotToSwapWith.Item1][aSlotToSwapWith.Item2] = null;
                    sourceItem.Count = total;
                }
            }
            else
            {
                items[aSlot.Item1][aSlot.Item2] = targetItem;
                items[aSlotToSwapWith.Item1][aSlotToSwapWith.Item2] = sourceItem;
            }

            NotifySlotChanged(aSlot.Item1, aSlot.Item2, this);
            NotifySlotChanged(aSlotToSwapWith.Item1, aSlotToSwapWith.Item2, this);
        }

        public bool RemoveItem(Item aItem, int aCountToRemove)
        {
            AssertSimThread();
            if (aItem == null || aCountToRemove <= 0)
            {
                return false;
            }

            int remainingCount = aCountToRemove;
            List<(int, int)> slotsToClear = new List<(int, int)>();
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null) continue;
                for (int j = 0; j < items[i].Length; j++)
                {
                    Item slotItem = items[i][j];
                    if (slotItem == null) continue;
                    if (slotItem.ID != aItem.ID) continue;

                    if (remainingCount > slotItem.Count)
                    {
                        remainingCount -= slotItem.Count;
                        slotsToClear.Add((i, j));
                    }

                    if (remainingCount <= slotItem.Count)
                    {
                        TrimStack((i, j), remainingCount);

                        for (int k = 0; k < slotsToClear.Count; k++)
                        {
                            (int, int) slotToClear = slotsToClear[k];
                            TrimStack(slotToClear, items[slotToClear.Item1][slotToClear.Item2].Count);
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public void TrimStack(int aBagIndex, int aSlotIndex, int aCount)
        {
            AssertSimThread();
            Item slotItem = items[aBagIndex][aSlotIndex];
            Debug.Assert(slotItem != null, "Tried to trim an empty slot.");
            if (slotItem == null)
            {
                return;
            }

            Debug.Assert(aCount <= slotItem.Count, "Tried to remove to much from item");
            if (aCount > slotItem.Count)
            {
                return;
            }

            slotItem.Count -= aCount;
            if (slotItem.Count == 0)
            {
                items[aBagIndex][aSlotIndex] = null;
            }

            NotifySlotChanged(aBagIndex, aSlotIndex, this);
        }

        public void TrimStack((int, int) aBagAndSlotIndex, int aCount)
        {
            AssertSimThread();
            TrimStack(aBagAndSlotIndex.Item1, aBagAndSlotIndex.Item2, aCount);
        }

        public bool DestroyItem(Item aItem)
        {
            AssertSimThread();
            if (aItem == null)
            {
                return false;
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null) continue;
                for (int j = 0; j < items[i].Length; j++)
                {
                    if (items[i][j] != aItem) continue;

                    AssignItem(null, i, j);
                    return true;
                }
            }

            DebugManager.Print("Tried to destoy item but couldnt find it.");
            return false;
        }

        public int DestroyItemAtSlot(int aBagIndex, int aSlotIndex)
        {
            AssertSimThread();
            if (items[aBagIndex][aSlotIndex] == null)
            {
                DebugManager.Print("Tried to destoy item but couldnt find it.");
                return 0;
            }

            int count = items[aBagIndex][aSlotIndex].Count;
            items[aBagIndex][aSlotIndex] = null;
            return count;
        }
    }
}
