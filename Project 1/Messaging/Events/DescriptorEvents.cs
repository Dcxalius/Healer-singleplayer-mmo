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

        public static ItemDescriptorSnapshot FromItemData(ItemData itemData)
        {
            if (itemData == null) throw new ArgumentNullException(nameof(itemData));
            string statReport = null;
            if (itemData is WeaponData weaponData)
            {
                statReport = BuildWeaponDataStatReport(weaponData);
            }
            else if (itemData is EquipmentData equipmentData)
            {
                statReport = equipmentData.StatReport.Value;
            }

            bool hasStatReport = !string.IsNullOrEmpty(statReport);
            bool hasSellPrice = itemData.Cost > 0;
            int sellPrice = hasSellPrice ? (int)MathF.Floor(itemData.Cost / 4f) : 0;
            return new ItemDescriptorSnapshot(itemData.Name, itemData.Description, statReport, sellPrice, hasStatReport, hasSellPrice);
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

        static string BuildWeaponDataStatReport(WeaponData weaponData)
        {
            List<string> lines = new List<string>
            {
                BuildWeaponCategoryLine(weaponData.Slot, weaponData.WeaponType)
            };

            if (weaponData.MaxAttackDamage > 0)
            {
                int minDamage = (int)Math.Round(weaponData.MinAttackDamage, MidpointRounding.AwayFromZero);
                int maxDamage = (int)Math.Round(weaponData.MaxAttackDamage, MidpointRounding.AwayFromZero);
                lines.Add($"{minDamage} - {maxDamage} Damage");
            }

            if (weaponData.AttackSpeed > 0)
            {
                lines.Add($"Speed {weaponData.AttackSpeed.ToString("0.##", CultureInfo.InvariantCulture)}");
            }

            string equipmentLines = weaponData.StatReport.Value;
            if (!string.IsNullOrEmpty(equipmentLines))
            {
                lines.Add(equipmentLines);
            }

            return string.Join("\n", lines);
        }

        static string BuildWeaponCategoryLine(Equipment.Type slotType, Weapon.WeaponType weaponType)
        {
            return $"{GetHandRequirementDisplayName(slotType)} {GetWeaponTypeDisplayName(weaponType)}";
        }

        static string GetHandRequirementDisplayName(Equipment.Type slotType)
        {
            return slotType switch
            {
                Equipment.Type.TwoHander => "Two-handed",
                Equipment.Type.MainHander => "Main-hand",
                Equipment.Type.OffHander => "Off-hand",
                Equipment.Type.Ranged => "Ranged",
                _ => "One-handed"
            };
        }

        static string GetWeaponTypeDisplayName(Weapon.WeaponType weaponType)
        {
            return weaponType switch
            {
                Weapon.WeaponType.Dagger => "Dagger",
                Weapon.WeaponType.Sword or Weapon.WeaponType.TwoHandedSword => "Sword",
                Weapon.WeaponType.Axe or Weapon.WeaponType.TwoHandedAxe => "Axe",
                Weapon.WeaponType.Mace or Weapon.WeaponType.TwoHandedMace => "Mace",
                Weapon.WeaponType.Fist => "Fist Weapon",
                Weapon.WeaponType.Staff => "Staff",
                Weapon.WeaponType.Bow => "Bow",
                Weapon.WeaponType.Gun => "Gun",
                Weapon.WeaponType.Thrown => "Thrown",
                Weapon.WeaponType.Wand => "Wand",
                Weapon.WeaponType.Shield => "Shield",
                Weapon.WeaponType.Holdable => "Held Item",
                _ => "Weapon"
            };
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
