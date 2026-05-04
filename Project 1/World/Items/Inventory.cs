using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        static readonly JsonSerializer itemSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });

        public const int bagSlots = 5;
        public const int defaultSlots = 32;

        [JsonProperty("Items", TypeNameHandling = TypeNameHandling.Auto)]
        public Item[][] SerializableItems
        {
            get => items;
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
        public Bag[] Bags => bags;
        Bag[] bags;

        [JsonIgnore]
        public Item[][] Items => items;
        Item[][] items;
        public Inventory()
        {
            bags = new Bag[bagSlots]; //Bag 0 is fornow always null
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
        public Inventory(int?[] bags, JToken items)
        {
            this.bags = new Bag[bagSlots]; //Bag 0 is fornow always null


            for (int i = 1; i < bags.Length; i++)
            {
                if (bags[i] == null) continue;
                this.bags[i] = new Bag(ItemFactory.GetItemData<BagData>(bags[i].Value));
            }

            this.items = new Item[bagSlots][];
            this.items[0] = new Item[defaultSlots];
            DeserializeBagItems(items?[0], this.items[0]);
            for (int i = 1; i < bags.Length; i++)
            {
                if (this.bags[i] != null)
                {
                    this.items[i] = new Item[this.bags[i].SlotCount];
                    DeserializeBagItems(items?[i], this.items[i]);
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

        internal bool ConsumeOneFromSlot((int, int) aBagAndSlotIndex)
        {
            AssertSimThread();
            Item item = GetItemInSlot(aBagAndSlotIndex);
            if (item == null) return false;

            TrimStack(aBagAndSlotIndex.Item1, aBagAndSlotIndex.Item2, 1);
            NotifySlotChanged(aBagAndSlotIndex, this);
            return true;
        }

        internal void RefreshSlot((int, int) aBagAndSlotIndex)
        {
            AssertSimThread();
            NotifySlotChanged(aBagAndSlotIndex, this);
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

        static void DeserializeBagItems(JToken bagToken, Item[] destination)
        {
            if (bagToken == null || destination == null) return;
            if (bagToken.Type != JTokenType.Array) return;

            int slotCount = Math.Min(destination.Length, bagToken.Count());
            for (int i = 0; i < slotCount; i++)
            {
                destination[i] = DeserializeItemToken(bagToken[i]);
            }
        }

        static Item DeserializeItemToken(JToken itemToken)
        {
            if (itemToken == null || itemToken.Type == JTokenType.Null) return null;

            if (itemToken.Type == JTokenType.Array)
            {
                int?[] legacyItem = itemToken.ToObject<int?[]>();
                if (legacyItem == null || legacyItem.Length < 2 || !legacyItem[0].HasValue || !legacyItem[1].HasValue)
                {
                    return null;
                }

                return ItemFactory.CreateItem(ItemFactory.GetItemData(legacyItem[0].Value), legacyItem[1].Value);
            }

            return itemToken.ToObject<Item>(itemSerializer);
        }

    }
}
