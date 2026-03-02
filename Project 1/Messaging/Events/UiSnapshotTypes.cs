using PairReport = Project_1.GameObjects.Unit.PairReport;
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

    internal readonly struct StatLineSnapshot
    {
        public StatLineSnapshot(string name, double value, StatLineCategory category = StatLineCategory.Unspecified)
        {
            Name = name;
            Value = value;
            Category = category;
        }

        public string Name { get; }
        public double Value { get; }
        public StatLineCategory Category { get; }
    }

    internal enum StatLineCategory
    {
        Unspecified = 0,
        Primary = 1,
        Attack = 2,
        Spell = 3,
        Defense = 4
    }

    internal static class StatLineCategoryResolver
    {
        public static StatLineCategory ResolveCharacter(string statName)
        {
            if (string.IsNullOrWhiteSpace(statName))
            {
                return StatLineCategory.Unspecified;
            }

            return statName switch
            {
                "Strength" or "Agility" or "Intellect" or "Spirit" or "Stamina" => StatLineCategory.Primary,
                "Armor" or "Dodge Chance" or "Parry Chance" or "Block Chance" or "Block Value" => StatLineCategory.Defense,
                _ when statName.StartsWith("Spell ", StringComparison.Ordinal) => StatLineCategory.Spell,
                _ => StatLineCategory.Attack
            };
        }
    }

    internal readonly struct SpellSchoolBonusSnapshot
    {
        public SpellSchoolBonusSnapshot(string schoolName, double value)
        {
            SchoolName = schoolName;
            Value = value;
        }

        public string SchoolName { get; }
        public double Value { get; }
    }

    internal readonly struct SpellReportDetailsSnapshot
    {
        public static SpellReportDetailsSnapshot Empty => new SpellReportDetailsSnapshot(
            Array.Empty<SpellSchoolBonusSnapshot>(),
            Array.Empty<SpellSchoolBonusSnapshot>(),
            Array.Empty<SpellSchoolBonusSnapshot>());

        public SpellReportDetailsSnapshot(
            SpellSchoolBonusSnapshot[] damageBonuses,
            SpellSchoolBonusSnapshot[] critChanceBonuses,
            SpellSchoolBonusSnapshot[] hitChanceBonuses)
        {
            DamageBonuses = damageBonuses ?? Array.Empty<SpellSchoolBonusSnapshot>();
            CritChanceBonuses = critChanceBonuses ?? Array.Empty<SpellSchoolBonusSnapshot>();
            HitChanceBonuses = hitChanceBonuses ?? Array.Empty<SpellSchoolBonusSnapshot>();
        }

        public SpellSchoolBonusSnapshot[] DamageBonuses { get; }
        public SpellSchoolBonusSnapshot[] CritChanceBonuses { get; }
        public SpellSchoolBonusSnapshot[] HitChanceBonuses { get; }
    }

    internal readonly struct StatReportSnapshot
    {
        public static StatReportSnapshot Empty => new StatReportSnapshot(Array.Empty<StatLineSnapshot>(), SpellReportDetailsSnapshot.Empty);

        public StatReportSnapshot(StatLineSnapshot[] lines)
            : this(lines, SpellReportDetailsSnapshot.Empty)
        {
        }

        public StatReportSnapshot(StatLineSnapshot[] lines, SpellReportDetailsSnapshot spellDetails)
        {
            Lines = lines ?? Array.Empty<StatLineSnapshot>();
            SpellDetails = spellDetails;
        }

        public StatLineSnapshot[] Lines { get; }
        public SpellReportDetailsSnapshot SpellDetails { get; }
        public int Count => Lines?.Length ?? 0;

        public static StatReportSnapshot FromPairReport(
            PairReport report,
            SpellReportDetailsSnapshot spellDetails = default,
            Func<string, StatLineCategory> categoryResolver = null)
        {
            if (report == null || report.Count == 0) return Empty;
            StatLineSnapshot[] lines = new StatLineSnapshot[report.Count];
            for (int i = 0; i < report.Count; i++)
            {
                var line = report.Lines[i];
                StatLineCategory category = categoryResolver?.Invoke(line.Name) ?? StatLineCategory.Unspecified;
                lines[i] = new StatLineSnapshot(line.Name, line.Value, category);
            }
            return new StatReportSnapshot(lines, spellDetails);
        }
    }
}
