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
        public StatLineSnapshot(string name, double value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }
        public double Value { get; }
    }

    internal readonly struct StatReportSnapshot
    {
        public static StatReportSnapshot Empty => new StatReportSnapshot(Array.Empty<StatLineSnapshot>());

        public StatReportSnapshot(StatLineSnapshot[] lines)
        {
            Lines = lines ?? Array.Empty<StatLineSnapshot>();
        }

        public StatLineSnapshot[] Lines { get; }
        public int Count => Lines?.Length ?? 0;

        public static StatReportSnapshot FromPairReport(PairReport report)
        {
            if (report == null || report.Count == 0) return Empty;
            StatLineSnapshot[] lines = new StatLineSnapshot[report.Count];
            for (int i = 0; i < report.Count; i++)
            {
                var line = report.Lines[i];
                lines[i] = new StatLineSnapshot(line.Name, line.Value);
            }
            return new StatReportSnapshot(lines);
        }
    }
}
