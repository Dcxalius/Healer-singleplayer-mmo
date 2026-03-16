using System;

namespace Project_1.Items.SubTypes
{
    internal partial class EquipmentData
    {
        const double StatTermPower = 1.5;
        const double ItemValuePower = 2.0 / 3.0;
        const double ItemValueScale = 100.0;

        const double UncommonItemSlotOffset = 9.8;
        const double UncommonItemSlotScale = 1.21;
        const double RareItemSlotOffset = 4.2;
        const double RareItemSlotScale = 1.42;
        const double EpicItemSlotOffset = 11.2;
        const double EpicItemSlotScale = 1.64;

        public static double ComputeItemValue(EquipmentData aEquipmentData)
        {
            double poweredStatSum = ComputePoweredStatSum(aEquipmentData);
            return ItemValueFromPoweredStatSum(poweredStatSum);
        }

        static double ComputeItemValue(ReadOnlySpan<double> aStatValues, ReadOnlySpan<double> aStatModifiers)
        {
            ValidateStatInputs(aStatValues, aStatModifiers);

            double poweredStatSum = 0;
            for (int i = 0; i < aStatValues.Length; i++)
            {
                AddStatContribution(ref poweredStatSum, aStatValues[i], aStatModifiers[i]);
            }

            return ItemValueFromPoweredStatSum(poweredStatSum);
        }

        public static double ComputeItemSlotValue(ReadOnlySpan<double> aStatValues, ReadOnlySpan<double> aStatModifiers, double aSlotModifier)
        {
            if (aSlotModifier <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(aSlotModifier), "Slot modifier must be greater than 0.");
            }

            return ComputeItemValue(aStatValues, aStatModifiers) * aSlotModifier;
        }

        public static double ComputeItemLevel(ReadOnlySpan<double> aStatValues, ReadOnlySpan<double> aStatModifiers, double aSlotModifier, Item.Quality aQuality)
        {
            double itemSlotValue = ComputeItemSlotValue(aStatValues, aStatModifiers, aSlotModifier);
            return aQuality switch
            {
                Item.Quality.Uncommon => (itemSlotValue + UncommonItemSlotOffset) / UncommonItemSlotScale,
                Item.Quality.Rare => (itemSlotValue + RareItemSlotOffset) / RareItemSlotScale,
                Item.Quality.Epic => (itemSlotValue - EpicItemSlotOffset) / EpicItemSlotScale,
                _ => throw new ArgumentOutOfRangeException(nameof(aQuality), "Unsupported item quality for ilvl calculation.")
            };
        }

        static double RequiredPoweredSumArgument(EquipmentData aEquipmentData)
        {
            if (aEquipmentData == null)
            {
                throw new ArgumentNullException(nameof(aEquipmentData));
            }

            double slotModifier = GetSlotModifier(aEquipmentData.Slot);
            double itemSlotValue = aEquipmentData.Quality switch
            {
                Item.Quality.Uncommon => (aEquipmentData.ItemLevel * UncommonItemSlotScale) - UncommonItemSlotOffset,
                Item.Quality.Rare => (aEquipmentData.ItemLevel * RareItemSlotScale) - RareItemSlotOffset,
                Item.Quality.Epic => (aEquipmentData.ItemLevel * EpicItemSlotScale) + EpicItemSlotOffset,
                _ => throw new ArgumentOutOfRangeException(nameof(aEquipmentData.Quality), "Unsupported item quality for inverse ilvl calculation.")
            };

            double itemValue = itemSlotValue / slotModifier;
            double scaledItemValue = itemValue * ItemValueScale;
            if (scaledItemValue < 0)
            {
                throw new ArgumentException("Inverse ilvl calculation produced a negative item value.");
            }

            return Math.Pow(scaledItemValue, StatTermPower);
        }

        static double[] RequiredStatValuesEqualContribution(EquipmentData aEquipmentData, ReadOnlySpan<StatBonuses> aStatModifiers)
        {
            if (aStatModifiers.Length == 0)
            {
                throw new ArgumentException("At least one stat modifier is required.", nameof(aStatModifiers));
            }

            for (int i = 0; i < aStatModifiers.Length; i++)
            {
                if (GetStatModifierValue(aStatModifiers[i]) <= 0)
                {
                    throw new ArgumentException("Each stat modifier must be greater than 0.", nameof(aStatModifiers));
                }
            }

            double poweredStatSum = RequiredPoweredSumArgument(aEquipmentData);
            double perTermPoweredValue = poweredStatSum / aStatModifiers.Length;
            double weightedStatValue = Math.Pow(perTermPoweredValue, ItemValuePower);
            double[] statValues = new double[aStatModifiers.Length];

            for (int i = 0; i < aStatModifiers.Length; i++)
            {
                statValues[i] = weightedStatValue / GetStatModifierValue(aStatModifiers[i]);
            }

            return statValues;
        }

        static void ValidateStatInputs(ReadOnlySpan<double> aStatValues, ReadOnlySpan<double> aStatModifiers)
        {
            if (aStatValues.Length != aStatModifiers.Length)
            {
                throw new ArgumentException("Stat values and stat modifiers must have the same length.");
            }

            if (aStatValues.Length == 0)
            {
                throw new ArgumentException("At least one stat value is required.");
            }
        }

        static void AddSecondaryStatContributions(ref double aPoweredStatSum, SecondayStatBonus<int>[] aSecondaryStats)
        {
            if (aSecondaryStats == null)
            {
                return;
            }

            for (int i = 0; i < aSecondaryStats.Length; i++)
            {
                if (!TryGetStatModifier(aSecondaryStats[i].SecondaryStat, out StatBonuses statModifier))
                {
                    continue;
                }

                AddStatContribution(ref aPoweredStatSum, aSecondaryStats[i].Value, statModifier);
            }
        }

        static void AddSecondaryStatContributions(ref double aPoweredStatSum, SecondayStatBonus<float>[] aSecondaryStats)
        {
            if (aSecondaryStats == null)
            {
                return;
            }

            for (int i = 0; i < aSecondaryStats.Length; i++)
            {
                if (!TryGetStatModifier(aSecondaryStats[i].SecondaryStat, out StatBonuses statModifier))
                {
                    continue;
                }

                AddStatContribution(ref aPoweredStatSum, aSecondaryStats[i].Value, statModifier);
            }
        }

        static bool TryGetStatModifier(string aStatName, out StatBonuses aModifier)
        {
            if (string.IsNullOrWhiteSpace(aStatName))
            {
                aModifier = default;
                return false;
            }

            if (Enum.TryParse(aStatName, true, out aModifier))
            {
                return true;
            }

            string normalizedName = aStatName.Replace(" ", string.Empty).Replace("-", string.Empty);
            return Enum.TryParse(normalizedName, true, out aModifier);
        }

        static void AddStatContribution(ref double aPoweredStatSum, double aStatValue, StatBonuses aStatModifier)
        {
            AddStatContribution(ref aPoweredStatSum, aStatValue, GetStatModifierValue(aStatModifier));
        }

        static void AddStatContribution(ref double aPoweredStatSum, double aStatValue, double aStatModifier)
        {
            if (aStatValue == 0)
            {
                return;
            }

            double weightedStatValue = aStatValue * aStatModifier;
            if (weightedStatValue < 0)
            {
                throw new ArgumentException("Stat value multiplied by stat modifier must be non-negative.");
            }

            aPoweredStatSum += Math.Pow(weightedStatValue, StatTermPower);
        }

        static double GetSlotModifier(Equipment.Type aSlot)
        {
            return aSlot switch
            {
                Equipment.Type.Head or Equipment.Type.Chest or Equipment.Type.Legs => 1.0,
                Equipment.Type.Shoulders or Equipment.Type.Hands or Equipment.Type.Belt or Equipment.Type.Feet => 1.35,
                Equipment.Type.Trinket => 1.47,
                Equipment.Type.Wrist or Equipment.Type.Neck or Equipment.Type.Back or Equipment.Type.Finger => 1.85,
                Equipment.Type.TwoHander => 1.0,
                Equipment.Type.OneHander or Equipment.Type.MainHander => 2.44,
                Equipment.Type.OffHander => 1.92,
                Equipment.Type.Ranged => 3.33,
                _ => throw new ArgumentOutOfRangeException(nameof(aSlot), aSlot, "Unhandled equipment slot."),
            };
        }

        static double GetStatModifierValue(StatBonuses aStatBonus)
        {
            return aStatBonus switch
            {
                StatBonuses.BonusArmor => 22,
                StatBonuses.AttackPowerVsDemonsBeastsUndead => 76,
                StatBonuses.RangedAttackPower => 92,
                StatBonuses.SpellHealing => 100,
                StatBonuses.AttackPower => 115,
                StatBonuses.BlockingValue => 150,
                StatBonuses.FrostSpellDamage => 159,
                StatBonuses.FireSpellDamage => 159,
                StatBonuses.ArcaneSpellDamage => 159,
                StatBonuses.NatureSpellDamage => 159,
                StatBonuses.ShadowSpellDamage => 159,
                StatBonuses.HolySpellDamage => 159,
                StatBonuses.SpellDamageAllSpells => 192,
                StatBonuses.FireResist => 230,
                StatBonuses.FrostResist => 230,
                StatBonuses.ArcaneResist => 230,
                StatBonuses.NatureResist => 230,
                StatBonuses.ShadowResist => 230,
                StatBonuses.HolyResist => 230,
                StatBonuses.Agility => 230,
                StatBonuses.Strength => 230,
                StatBonuses.Stamina => 230,
                StatBonuses.Intellect => 230,
                StatBonuses.Spirit => 230,
                StatBonuses.Defense => 345,
                StatBonuses.HpRegenPer5Sec => 550,
                StatBonuses.ManaRegenPer5Sec => 550,
                StatBonuses.WeaponSkillOther => 550,
                StatBonuses.WeaponSkillDaggers => 720,
                StatBonuses.ShieldThorns => 720,
                StatBonuses.ChanceToBlock => 1300,
                StatBonuses.ChanceToHit => 2200,
                StatBonuses.ChanceToHitWithAllSpells => 2500,
                StatBonuses.ChanceToDodge => 2500,
                StatBonuses.ChanceToCritWithAllSpells => 2600,
                StatBonuses.ChanceToCrit => 3200,
                StatBonuses.ChanceToParry => 3600,
                _ => throw new ArgumentOutOfRangeException(nameof(aStatBonus), aStatBonus, "Unhandled stat bonus type."),
            };
        }

        static int PositiveModulo(int aValue, int aModulus)
        {
            int result = aValue % aModulus;
            if (result < 0)
            {
                result += aModulus;
            }

            return result;
        }

        static double ComputePoweredStatSum(EquipmentData aEquipmentData)
        {
            if (aEquipmentData == null)
            {
                throw new ArgumentNullException(nameof(aEquipmentData));
            }

            double poweredStatSum = 0;
            AddStatContribution(ref poweredStatSum, aEquipmentData.BaseStats.Armor, StatBonuses.BonusArmor);
            AddStatContribution(ref poweredStatSum, aEquipmentData.BaseStats.Strength, StatBonuses.Strength);
            AddStatContribution(ref poweredStatSum, aEquipmentData.BaseStats.Agility, StatBonuses.Agility);
            AddStatContribution(ref poweredStatSum, aEquipmentData.BaseStats.Intellect, StatBonuses.Intellect);
            AddStatContribution(ref poweredStatSum, aEquipmentData.BaseStats.Spirit, StatBonuses.Spirit);
            AddStatContribution(ref poweredStatSum, aEquipmentData.BaseStats.Stamina, StatBonuses.Stamina);
            AddSecondaryStatContributions(ref poweredStatSum, aEquipmentData.SecondayStatsInt);
            AddSecondaryStatContributions(ref poweredStatSum, aEquipmentData.SecondayStatsFloat);
            return poweredStatSum;
        }

        static double ComputePoweredStatSum(ReadOnlySpan<int> aStatValues, ReadOnlySpan<StatBonuses> aStatModifiers)
        {
            double poweredStatSum = 0;
            for (int i = 0; i < aStatValues.Length; i++)
            {
                AddStatContribution(ref poweredStatSum, aStatValues[i], aStatModifiers[i]);
            }

            return poweredStatSum;
        }

        static double ItemValueFromPoweredStatSum(double aPoweredStatSum)
        {
            return Math.Pow(aPoweredStatSum, ItemValuePower) / ItemValueScale;
        }
    }
}
