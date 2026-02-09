using Project_1.Items;
using Project_1.Items.SubTypes;
using System;

namespace Project_1.Messaging.Events
{
    internal readonly struct ItemDescriptorSnapshot
    {
        public ItemDescriptorSnapshot(string name, string description, string statReport, int sellPrice, bool hasStatReport, bool hasSellPrice)
        {
            Name = name;
            Description = description;
            StatReport = statReport;
            SellPrice = sellPrice;
            HasStatReport = hasStatReport;
            HasSellPrice = hasSellPrice;
        }

        public string Name { get; }
        public string Description { get; }
        public string StatReport { get; }
        public int SellPrice { get; }
        public bool HasStatReport { get; }
        public bool HasSellPrice { get; }

        public static ItemDescriptorSnapshot FromItem(Item item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            string statReport = null;
            if (item is Equipment equipment)
            {
                statReport = equipment.StatReport.Value;
            }

            bool hasStatReport = !string.IsNullOrEmpty(statReport);
            bool hasSellPrice = item.Cost > 0;
            int sellPrice = hasSellPrice ? item.SellPrice : 0;
            return new ItemDescriptorSnapshot(item.Name, item.Description, statReport, sellPrice, hasStatReport, hasSellPrice);
        }
    }

    internal readonly struct DescriptorBoxSet
    {
        public DescriptorBoxSet(ItemDescriptorSnapshot snapshot, Camera.AbsoluteScreenPosition? pos = null)
        {
            Snapshot = snapshot;
            Position = pos;
        }
        public ItemDescriptorSnapshot Snapshot { get; }
        public Camera.AbsoluteScreenPosition? Position { get; }
    }

    internal readonly struct DescriptorBoxClear
    {
    }
}
