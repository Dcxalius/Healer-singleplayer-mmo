using Newtonsoft.Json;
using Project_1.GameObjects.Entities.Friendlies;
using Project_1.Items.SubTypes;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Project_1.Items
{
    internal partial class Inventory
    {
        static void AssertSimThread() => ThreadAffinity.AssertSimThread();

        public const int bagSlots = 5;
        public const int defaultSlots = 32;

        [JsonProperty("Items")]
        public (int, int)?[][] ItemsAsID
        {
            get
            {
                (int, int)?[][] ids = new (int, int)?[items.Length][];

                ids[0] = new (int, int)?[defaultSlots];
                for (int i = 0; i < ids[0].Length; i++)
                {
                    if (items[0][i] == null) continue;
                    ids[0][i] = (items[0][i].ID, items[0][i].Count);
                }

                for (int i = 1; i < ids.Length; i++)
                {
                    if (bags[i] == null) continue;

                    ids[i] = new (int, int)?[bags[i].SlotCount];
                    for (int j = 0; j < ids[i].Length; j++)
                    {
                        if (items[i][j] == null) continue;
                        ids[i][j] = (items[i][j].ID, items[i][j].Count);
                    }
                }
                return ids;
            }
        }

        [JsonProperty("Bags")]
        public int?[] BagsAsID
        {
            get
            {
                int?[] ids = new int?[bags.Length];

                for (int i = 0; i < ids.Length; i++)
                {
                    if (bags[i] == null) continue;
                    ids[i] = bags[i].ID;
                }
                return ids;
            }
        }

        [JsonIgnore]
        public Container[] Bags => bags;
        Container[] bags;

        [JsonIgnore]
        public Item[][] Items => items;
        Item[][] items;
        public Inventory()
        {
            bags = new Container[bagSlots]; //Bag 0 is fornow always null
            items = new Item[bagSlots][];
            items[0] = new Item[defaultSlots];
            for (int i = 1; i < bags.Length; i++)
            {
                if (bags[i] != null)
                {
                    items[i] = new Item[bags[i].SlotCount];
                }
            }
        }

        [JsonConstructor]
        public Inventory(int?[] bags, (int, int)?[][] items)
        {
            this.bags = new Container[bagSlots]; //Bag 0 is fornow always null


            for (int i = 1; i < bags.Length; i++)
            {
                if (bags[i] == null) continue;
                this.bags[i] = new Container(ItemFactory.GetItemData<ContainerData>(bags[i].Value));
            }

            this.items = new Item[bagSlots][];
            this.items[0] = new Item[defaultSlots];
            for (int i = 0; i < this.items[0].Length; i++)
            {

                if (!items[0][i].HasValue) continue;
                this.items[0][i] = ItemFactory.CreateItem(ItemFactory.GetItemData(items[0][i].Value.Item1), items[0][i].Value.Item2);

            }
            for (int i = 1; i < bags.Length; i++)
            {
                if (this.bags[i] != null)
                {
                    this.items[i] = new Item[this.bags[i].SlotCount];
                    for (int j = 0; j < items[i].Length; j++)
                    {
                        if (!items[i][j].HasValue) continue;
                        this.items[i][j] = ItemFactory.CreateItem(ItemFactory.GetItemData(items[i][j].Value.Item1), items[i][j].Value.Item2);
                    }
                }
            }
        }

        public bool ConsumeItem((int, int) aBagAndSlotIndex, Friendly aFriendly)
        {
            AssertSimThread();
            return ConsumeItem(aBagAndSlotIndex.Item1, aBagAndSlotIndex.Item2, aFriendly);
        }

        public bool ConsumeItem(int aBagIndex, int aSlotIndex, Friendly aFriendly)
        {
            AssertSimThread();
            Debug.Assert(items[aBagIndex][aSlotIndex].ItemType == ItemData.ItemType.Consumable, "Tried to consume nonconcumable.");

            if (!(items[aBagIndex][aSlotIndex] as Consumable).Use(aFriendly)) return false;

            TrimStack(aBagIndex, aSlotIndex, 1);
            NotifySlotChanged(aBagIndex, aSlotIndex, this);
            return true;
        }

        public void LootItem(int aLootIndex)
        {
            AssertSimThread();
            int totalLooted = 0;
            string lootedItemName = null;
            while (true)
            {
                Item available = LootState.Peek(aLootIndex);
                if (available == null) break;

                bool placed = false;

                for (int i = 0; i < items.Length && !placed; i++)
                {
                    if (items[i] == null) continue;
                    for (int j = 0; j < items[i].Length && !placed; j++)
                    {
                        if (items[i][j] == null)
                        {
                            Item taken = LootState.Take(aLootIndex, available.Count);
                            if (taken == null) break;
                            items[i][j] = taken;
                            totalLooted += taken.Count;
                            lootedItemName ??= taken.Name;
                            NotifySlotChanged(i, j, this);
                            placed = true;
                            break;
                        }

                        if (items[i][j].ID != available.ID) continue;

                        int capacity = items[i][j].MaxStack - items[i][j].Count;
                        if (capacity <= 0) continue;

                        int takeAmount = Math.Min(capacity, available.Count);
                        Item takenItem = LootState.Take(aLootIndex, takeAmount);
                        if (takenItem == null) break;
                        items[i][j].Count += takenItem.Count;
                        totalLooted += takenItem.Count;
                        lootedItemName ??= takenItem.Name;
                        NotifySlotChanged(i, j, this);
                        placed = true;
                        break;
                    }
                }

                if (!placed) break;
            }

            if (totalLooted > 0)
            {
                PublishLootReceivedMessage(lootedItemName, totalLooted);
            }
        }

        public void LootItem(int aLootIndex, (int, int) aBagAndSlot)
        {
            AssertSimThread();
            Item available = LootState.Peek(aLootIndex);
            if (available == null) return;

            if (items[aBagAndSlot.Item1][aBagAndSlot.Item2] == null)
            {
                Item taken = LootState.Take(aLootIndex, available.Count);
                if (taken == null) return;
                items[aBagAndSlot.Item1][aBagAndSlot.Item2] = taken;
                NotifySlotChanged(aBagAndSlot, this);
                PublishLootReceivedMessage(taken.Name, taken.Count);
                return;
            }

            if (available.ID != items[aBagAndSlot.Item1][aBagAndSlot.Item2].ID) return;

            int capacity = items[aBagAndSlot.Item1][aBagAndSlot.Item2].MaxStack - items[aBagAndSlot.Item1][aBagAndSlot.Item2].Count;
            if (capacity <= 0) return;

            int takeAmount = Math.Min(capacity, available.Count);
            Item takenPartial = LootState.Take(aLootIndex, takeAmount);
            if (takenPartial == null) return;
            items[aBagAndSlot.Item1][aBagAndSlot.Item2].Count += takenPartial.Count;
            NotifySlotChanged(aBagAndSlot, this);
            PublishLootReceivedMessage(takenPartial.Name, takenPartial.Count);

        }

        static void PublishLootReceivedMessage(string itemName, int count)
        {
            if (count <= 0) return;
            if (string.IsNullOrWhiteSpace(itemName)) return;
            MailboxManager.PublishUiEvent(new ChatMessagePosted(ChatMessageType.Loot, $"{count}x [{itemName}]"));
        }

        public void AssignItem(Item item, (int, int) aBagAndSlotIndex)
        {
            AssertSimThread();
            AssignItem(item, aBagAndSlotIndex.Item1, aBagAndSlotIndex.Item2);
        }
        
        public void AssignItem(Item aItem, int aBagIndex, int aSlotIndex)
        {
            AssertSimThread();
            items[aBagIndex][aSlotIndex] = aItem;
            NotifySlotChanged(aBagIndex, aSlotIndex, this);
        }

        void NotifySlotChanged(int bagIndex, int slotIndex) => MailboxManager.PublishUiEvent(new InventorySlotChanged(bagIndex, slotIndex, BuildUiSnapshot()));
        void NotifySlotChanged((int, int) bagAndSlot) => NotifySlotChanged(bagAndSlot.Item1, bagAndSlot.Item2);

        // Temporary overloads to keep call sites compact during migration.
        void NotifySlotChanged(int bagIndex, int slotIndex, Inventory inventory) => NotifySlotChanged(bagIndex, slotIndex);
        void NotifySlotChanged((int, int) bagAndSlot, Inventory inventory) => NotifySlotChanged(bagAndSlot);

    }
}
