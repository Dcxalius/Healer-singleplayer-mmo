using Project_1.Items;
using Project_1.Items.SubTypes;
using Project_1.Camera;
using System;
using System.Collections.Generic;
using System.Globalization;

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
            if (item is Weapon weapon)
            {
                statReport = BuildWeaponStatReport(weapon);
            }
            else if (item is Equipment equipment)
            {
                statReport = equipment.StatReport.Value;
            }

            bool hasStatReport = !string.IsNullOrEmpty(statReport);
            bool hasSellPrice = item.Cost > 0;
            int sellPrice = hasSellPrice ? item.SellPrice : 0;
            return new ItemDescriptorSnapshot(item.Name, item.Description, statReport, sellPrice, hasStatReport, hasSellPrice);
        }

        static string BuildWeaponStatReport(Weapon weapon)
        {
            List<string> lines = new List<string>
            {
                weapon.TooltipWeaponCategoryLine
            };

            if (weapon.WeaponData.MaxAttackDamage > 0)
            {
                int minDamage = (int)Math.Round(weapon.WeaponData.MinAttackDamage, MidpointRounding.AwayFromZero);
                int maxDamage = (int)Math.Round(weapon.WeaponData.MaxAttackDamage, MidpointRounding.AwayFromZero);
                lines.Add($"{minDamage} - {maxDamage} Damage");
            }

            if (weapon.WeaponData.AttackSpeed > 0)
            {
                lines.Add($"Speed {weapon.WeaponData.AttackSpeed.ToString("0.##", CultureInfo.InvariantCulture)}");
            }

            string equipmentLines = weapon.StatReport.Value;
            if (!string.IsNullOrEmpty(equipmentLines))
            {
                lines.Add(equipmentLines);
            }

            return string.Join("\n", lines);
        }
    }

    internal readonly struct DescriptorBoxSet
    {
        public DescriptorBoxSet(ItemDescriptorSnapshot snapshot, AbsoluteScreenPosition? pos = null)
        {
            Snapshot = snapshot;
            Position = pos;
        }
        public ItemDescriptorSnapshot Snapshot { get; }
        public AbsoluteScreenPosition? Position { get; }
    }

    internal readonly struct DescriptorBoxClear
    {
    }
}
