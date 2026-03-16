using System;

namespace Project_1.Items.SubTypes
{
    internal partial class EquipmentData
    {
        static int CalculateArmorFromFormula(Equipment.Type aSlot, Equipment.GearType aMaterial, Item.Quality aQuality, int aItemLevel)
        {
            double slotModifier = GetArmorSlotModifier(aSlot);
            if (slotModifier <= 0)
            {
                return 0;
            }

            double baseArmor = GetArmorClassBase(aSlot, aMaterial, aItemLevel);
            double qualityMultiplier = GetArmorQualityMultiplier(aQuality);
            return Math.Max(0, (int)Math.Round(baseArmor * slotModifier * qualityMultiplier, MidpointRounding.AwayFromZero));
        }

        static double GetArmorClassBase(Equipment.Type aSlot, Equipment.GearType aMaterial, int aItemLevel)
        {
            if (aSlot == Equipment.Type.Back)
            {
                return 1.19 * aItemLevel + 5.1;
            }

            return aMaterial switch
            {
                Equipment.GearType.Cloth => 1.19 * aItemLevel + 5.1,
                Equipment.GearType.Leather => 2.22 * aItemLevel + 10,
                Equipment.GearType.Mail => 4.9 * aItemLevel + 29,
                Equipment.GearType.Plate => 9.0 * aItemLevel + 23,
                _ => 0
            };
        }

        static double GetArmorSlotModifier(Equipment.Type aSlot)
        {
            return aSlot switch
            {
                Equipment.Type.Chest => 1.00,
                Equipment.Type.Legs => 0.875,
                Equipment.Type.Head => 0.8125,
                Equipment.Type.Shoulders => 0.75,
                Equipment.Type.Feet => 0.6875,
                Equipment.Type.Hands => 0.625,
                Equipment.Type.Belt => 0.5625,
                Equipment.Type.Wrist => 0.4375,
                Equipment.Type.Back => 0.48,
                _ => 0
            };
        }

        static double GetArmorQualityMultiplier(Item.Quality aQuality)
        {
            return aQuality switch
            {
                Item.Quality.Rare => 1.1,
                Item.Quality.Epic => 1.2,
                Item.Quality.Legendary => 1.2,
                _ => 1.0
            };
        }
    }
}
