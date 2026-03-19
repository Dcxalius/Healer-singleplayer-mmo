using EquipmentItem = Project_1.Items.SubTypes.Equipment;
using EquipmentSlots = Project_1.GameObjects.Unit.Equipment;
using Microsoft.Xna.Framework;
using Project_1.Items;
using Project_1.Items.SubTypes;
using Project_1.Textures;
using System;

namespace Project_1.Messaging.Events
{
    internal readonly struct ItemUiSnapshot
    {
        public static ItemUiSnapshot Empty => default;

        public ItemUiSnapshot(int id, int count, int hash = 0, bool hasHash = false)
            : this(id, count, hash, hasHash, ItemData.ItemType.NotSet, EquipmentItem.Type.Count, 0, 0, default, default, string.Empty, string.Empty, default, false)
        {
            if (id <= 0) return;
            ItemData data = ItemFactory.GetItemData(id);
            if (data == null) return;
            this = FromItemData(data, count, hash, hasHash);
        }

        public ItemUiSnapshot(
            int id,
            int count,
            int hash,
            bool hasHash,
            ItemData.ItemType itemType,
            EquipmentItem.Type equipmentType,
            int maxStack,
            int cost,
            GfxPath gfxPath,
            Color qualityColor,
            string name,
            string description,
            ItemDescriptorSnapshot descriptor,
            bool hasDescriptor)
        {
            Id = id;
            Count = count;
            Hash = hash;
            HasHash = hasHash;
            ItemType = itemType;
            EquipmentType = equipmentType;
            MaxStack = maxStack;
            Cost = cost;
            GfxPath = gfxPath;
            QualityColor = qualityColor;
            Name = name ?? string.Empty;
            Description = description ?? string.Empty;
            Descriptor = descriptor;
            HasDescriptor = hasDescriptor;
            HasValue = true;
        }

        public int Id { get; }
        public int Count { get; }
        public int Hash { get; }
        public bool HasHash { get; }
        public ItemData.ItemType ItemType { get; }
        public EquipmentItem.Type EquipmentType { get; }
        public int MaxStack { get; }
        public int Cost { get; }
        public GfxPath GfxPath { get; }
        public Color QualityColor { get; }
        public string Name { get; }
        public string Description { get; }
        public ItemDescriptorSnapshot Descriptor { get; }
        public bool HasDescriptor { get; }
        public bool HasValue { get; }
        public bool IsEquipmentLike => ItemType == ItemData.ItemType.Equipment || ItemType == ItemData.ItemType.Weapon;
        public bool IsMainHandRestrictedType => EquipmentType >= EquipmentItem.Type.MainHander && EquipmentType < EquipmentItem.Type.Count;

        public static ItemUiSnapshot FromItem(Item item)
        {
            if (item == null) return Empty;
            ItemData.ItemType itemType = item.ItemType;
            EquipmentItem.Type equipmentType = item is EquipmentItem equipmentItem ? equipmentItem.type : EquipmentItem.Type.Count;
            int maxStack = item.MaxStack;
            int cost = item.Cost;
            GfxPath gfxPath = item.GfxPath;
            Color qualityColor = item.ItemQualityColor;
            string name = item.Name;
            string description = item.Description;
            ItemDescriptorSnapshot descriptor = ItemDescriptorSnapshot.FromItem(item);

            if (item is EquipmentItem equipment)
            {
                return new ItemUiSnapshot(
                    item.ID,
                    item.Count,
                    equipment.Hash,
                    true,
                    itemType,
                    equipmentType,
                    maxStack,
                    cost,
                    gfxPath,
                    qualityColor,
                    name,
                    description,
                    descriptor,
                    true);
            }

            return new ItemUiSnapshot(
                item.ID,
                item.Count,
                0,
                false,
                itemType,
                equipmentType,
                maxStack,
                cost,
                gfxPath,
                qualityColor,
                name,
                description,
                descriptor,
                true);
        }

        public static ItemUiSnapshot FromItemId(int itemId, int count = 1)
        {
            if (itemId <= 0) return Empty;
            ItemData data = ItemFactory.GetItemData(itemId);
            return FromItemData(data, count);
        }

        public static ItemUiSnapshot FromItemData(ItemData data, int count = 1, int hash = 0, bool hasHash = false)
        {
            if (data == null) return Empty;
            ItemData.ItemType itemType = data.Type;
            EquipmentItem.Type equipmentType = data is EquipmentData equipmentData ? equipmentData.Slot : EquipmentItem.Type.Count;
            ItemDescriptorSnapshot descriptor = ItemDescriptorSnapshot.FromItemData(data);
            return new ItemUiSnapshot(
                data.ID,
                Math.Max(1, count),
                hash,
                hasHash,
                itemType,
                equipmentType,
                data.MaxStack,
                data.Cost,
                data.GfxPath,
                ColorFromQuality(data.Quality),
                data.Name,
                data.Description,
                descriptor,
                true);
        }

        public Item ToItem()
        {
            if (!HasValue) return null;

            if (HasHash)
            {
                ItemData data = ItemFactory.GetItemData(Id);
                if (data is WeaponData weaponData) return new Weapon(weaponData, Hash);
                if (data is EquipmentData equipmentData) return new EquipmentItem(equipmentData, Hash);
            }

            return ItemFactory.CreateItem(ItemFactory.GetItemData(Id), Count);
        }

        public bool FitsInSlot(EquipmentSlots.Slot slot)
        {
            if (!IsEquipmentLike) return false;

            switch (slot)
            {
                case EquipmentSlots.Slot.Head:
                case EquipmentSlots.Slot.Neck:
                case EquipmentSlots.Slot.Shoulders:
                case EquipmentSlots.Slot.Back:
                case EquipmentSlots.Slot.Chest:
                case EquipmentSlots.Slot.Wrist:
                case EquipmentSlots.Slot.Hands:
                case EquipmentSlots.Slot.Belt:
                case EquipmentSlots.Slot.Legs:
                case EquipmentSlots.Slot.Feet:
                    return (EquipmentSlots.Slot)EquipmentType == slot;
                case EquipmentSlots.Slot.Trinket1:
                case EquipmentSlots.Slot.Trinket2:
                    return EquipmentType == EquipmentItem.Type.Trinket;
                case EquipmentSlots.Slot.Finger1:
                case EquipmentSlots.Slot.Finger2:
                    return EquipmentType == EquipmentItem.Type.Finger;
                case EquipmentSlots.Slot.MainHand:
                    return EquipmentType == EquipmentItem.Type.MainHander
                        || EquipmentType == EquipmentItem.Type.OneHander
                        || EquipmentType == EquipmentItem.Type.TwoHander;
                case EquipmentSlots.Slot.OffHand:
                    return EquipmentType == EquipmentItem.Type.OffHander
                        || EquipmentType == EquipmentItem.Type.OneHander;
                case EquipmentSlots.Slot.Ranged:
                    return EquipmentType == EquipmentItem.Type.Ranged;
                default:
                    return false;
            }
        }

        static Color ColorFromQuality(Item.Quality quality)
        {
            return quality switch
            {
                Item.Quality.Trash => Color.LightGray,
                Item.Quality.Common => Color.White,
                Item.Quality.Uncommon => Color.Green,
                Item.Quality.Rare => Color.Blue,
                Item.Quality.Epic => Color.Purple,
                Item.Quality.Legendary => Color.Orange,
                _ => Color.White
            };
        }
    }
}
