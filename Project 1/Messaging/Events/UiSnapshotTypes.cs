using PairReport = Project_1.GameObjects.Unit.PairReport;
using EquipmentItem = Project_1.Items.SubTypes.Equipment;
using Project_1.Items;
using Project_1.Items.SubTypes;
using System;

namespace Project_1.Messaging.Events
{
    internal readonly struct ItemUiSnapshot
    {
        public static ItemUiSnapshot Empty => default;

        public ItemUiSnapshot(int id, int count, int hash = 0, bool hasHash = false)
        {
            Id = id;
            Count = count;
            Hash = hash;
            HasHash = hasHash;
            HasValue = true;
        }

        public int Id { get; }
        public int Count { get; }
        public int Hash { get; }
        public bool HasHash { get; }
        public bool HasValue { get; }

        public static ItemUiSnapshot FromItem(Item item)
        {
            if (item == null) return Empty;
            if (item is EquipmentItem equipment)
            {
                return new ItemUiSnapshot(item.ID, item.Count, equipment.Hash, true);
            }
            return new ItemUiSnapshot(item.ID, item.Count);
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
