using Newtonsoft.Json;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Stats;
using System;
using static Project_1.Items.SubTypes.Equipment;

namespace Project_1.Items.SubTypes
{
    internal class EquipmentData : ItemData
    {
        const double MinimumRollableSuffixValue = 1.0;

        const double StatTermPower = 1.5;
        const double ItemValuePower = 2.0 / 3.0;
        const double ItemValueScale = 100.0;

        const double UncommonItemSlotOffset = 9.8;
        const double UncommonItemSlotScale = 1.21;
        const double RareItemSlotOffset = 4.2;
        const double RareItemSlotScale = 1.42;
        const double EpicItemSlotOffset = 11.2;
        const double EpicItemSlotScale = 1.64;

        static readonly (string name, StatBonuses[] stats)[] suffixTemplates =
        {
            ("of the Tiger", new[] { StatBonuses.Strength, StatBonuses.Agility }),
            ("of the Bear", new[] { StatBonuses.Strength, StatBonuses.Stamina }),
            ("of the Gorilla", new[] { StatBonuses.Strength, StatBonuses.Intellect }),
            ("of the Boar", new[] { StatBonuses.Spirit, StatBonuses.Strength }),
            ("of the Monkey", new[] { StatBonuses.Agility, StatBonuses.Stamina }),
            ("of the Falcon", new[] { StatBonuses.Agility, StatBonuses.Intellect }),
            ("of the Wolf", new[] { StatBonuses.Agility, StatBonuses.Spirit }),
            ("of the Owl", new[] { StatBonuses.Intellect, StatBonuses.Spirit }),
            ("of the Whale", new[] { StatBonuses.Stamina, StatBonuses.Strength }),
            ("of the Eagle", new[] { StatBonuses.Intellect, StatBonuses.Stamina }),
            ("of Agility", new[] { StatBonuses.Agility }),
            ("of Strength", new[] { StatBonuses.Strength }),
            ("of Intellect", new[] { StatBonuses.Intellect }),
            ("of Stamina", new[] { StatBonuses.Stamina }),
            ("of Spirit", new[] { StatBonuses.Spirit }),
            ("of Frozen Wrath", new[] { StatBonuses.SpellDamageOneSchool }),
            ("of Fiery Wrath", new[] { StatBonuses.SpellDamageOneSchool }),
            ("of Arcane Wrath", new[] { StatBonuses.SpellDamageOneSchool }),
            ("of Nature's Wrath", new[] { StatBonuses.SpellDamageOneSchool }),
            ("of Shadow Wrath", new[] { StatBonuses.SpellDamageOneSchool })
        };

        public enum StatBonuses
        {
            BonusArmor = 22,
            AttackPowerVsDemonsBeastsUndead = 76,
            RangedAttackPower = 92,
            SpellHealing = 100,
            AttackPower = 115,
            BlockingValue = 150,
            SpellDamageOneSchool = 159,
            SpellDamageAllSpells = 192,
            MagicResistOneSchool = 230,
            PrimaryStat = 230,
            Agility = 230,
            Strength = 230,
            Stamina = 230,
            Intellect = 230,
            Spirit = 230,
            Defense = 345,
            RegenPer5Sec = 550,
            WeaponSkillOther = 550,
            WeaponSkillDaggers = 720,
            DamageShield = 720,
            ChanceToBlock = 1300,
            ChanceToHit = 2200,
            ChanceToHitWithAllSpells = 2500,
            ChanceToDodge = 2500,
            ChanceToCritWithAllSpells = 2600,
            ChanceToCrit = 3200,
            ChanceToParry = 3600
        }

        [JsonIgnore]
        public PairReport StatReport
        {
            get
            {
                PairReport report = new PairReport();

                if (baseStats.Armor != 0) report.AddLine("Armor", baseStats.Armor);
                baseStats.AppendToExistingReport(ref report);

                return report;
            }
        }

        public SecondayStatBonus<int>[] SecondayStatsInt => secondaryStatsInt;
        SecondayStatBonus<int>[] secondaryStatsInt;

        public SecondayStatBonus<float>[] SecondayStatsFloat => secondaryStatsFloat;
        SecondayStatBonus<float>[] secondaryStatsFloat;

        public EquipmentStats BaseStats => baseStats;
        EquipmentStats baseStats;

        public GearType Material => material;
        GearType material;

        public Equipment.Type Slot => slot;
        Equipment.Type slot;

        public EquipmentData(int id, string gfxName, string name, string description, Equipment.Type slot, ItemType itemType, int armor, int[] baseStats, Item.Quality quality, int cost, GearType material, SecondayStatBonus<int>[] secondaryStatsInt, SecondayStatBonus<float>[] secondaryStatsFloat, int itemLevel = 1)
            : base(id, gfxName, name, description, 1, itemType, quality, cost, itemLevel)
        {
            this.slot = slot;
            this.baseStats = new EquipmentStats(baseStats, armor);
            this.material = material;
            this.secondaryStatsInt = secondaryStatsInt;
            this.secondaryStatsFloat = secondaryStatsFloat;
        }

        [JsonConstructor]
        public EquipmentData(int id, string gfxName, string name, string description, Equipment.Type slot, int armor, int[] baseStats, Item.Quality quality, int cost, GearType material, SecondayStatBonus<int>[] secondaryStatsInt, SecondayStatBonus<float>[] secondayStatFloat, int itemLevel = 1)
            : this(id, gfxName, name, description, slot, ItemType.Equipment, armor, baseStats, quality, cost, material, secondaryStatsInt, secondayStatFloat, itemLevel)
        {
        }

        public (string name, (StatBonuses stat, int value)[] stats) Suffix(int aHash)
        {
            double availablePoweredBudget = GetAvailableSuffixPoweredBudget();
            var template = GetSuffixTemplate(aHash, availablePoweredBudget, out int selectableTemplateCount);
            int[] statValues = CalculateSuffixStatValues(aHash, template.stats, availablePoweredBudget, selectableTemplateCount);
            var statLines = new (StatBonuses stat, int value)[template.stats.Length];
            for (int i = 0; i < template.stats.Length; i++)
            {
                statLines[i] = (template.stats[i], statValues[i]);
            }

            return (template.name, statLines);
        }

        public int SuffixID(int aHash)
        {
            return SelectSuffixTemplateIndex(aHash, GetAvailableSuffixPoweredBudget()).templateIndex;
        }

        public int SuffixStatValue(int aStatIndex, int aHash)
        {
            double availablePoweredBudget = GetAvailableSuffixPoweredBudget();
            var template = GetSuffixTemplate(aHash, availablePoweredBudget, out int selectableTemplateCount);
            if ((uint)aStatIndex >= (uint)template.stats.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(aStatIndex), aStatIndex, "Invalid suffix stat index.");
            }

            return CalculateSuffixStatValues(aHash, template.stats, availablePoweredBudget, selectableTemplateCount)[aStatIndex];
        }

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
            // scaledItemValue < 0 only when itemSlotValue < 0, because slotModifier is always > 0 for all slots:
            // Head/Chest/Legs=1.00, Shoulders/Hands/Belt/Feet=1.35, Trinket=1.47,
            // Wrist/Neck/Back/Finger=1.85, TwoHander=1.00, OneHander/MainHander=2.44, OffHander=1.92, Ranged=3.33.
            // Therefore the minimum ilvl to keep scaledItemValue non-negative is the same for every slot:
            // Uncommon: ilvl >= 9.8 / 1.21 = 8.099...  (integer minimum 9)
            // Rare:     ilvl >= 4.2 / 1.42 = 2.958...  (integer minimum 3)
            // Epic:     ilvl >= -11.2 / 1.64 = -6.829... (already non-negative for normal ilvl values)
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
                if (aStatModifiers[i] <= 0)
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
                statValues[i] = weightedStatValue / (double)aStatModifiers[i];
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
            AddStatContribution(ref aPoweredStatSum, aStatValue, (double)aStatModifier);
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

        static int PositiveModulo(int aValue, int aModulus)
        {
            int result = aValue % aModulus;
            if (result < 0)
            {
                result += aModulus;
            }

            return result;
        }

        (string name, StatBonuses[] stats) GetSuffixTemplate(int aHash, double aAvailablePoweredBudget, out int aSelectableTemplateCount)
        {
            var selected = SelectSuffixTemplateIndex(aHash, aAvailablePoweredBudget);
            aSelectableTemplateCount = selected.selectableTemplateCount;
            return suffixTemplates[selected.templateIndex];
        }

        int[] CalculateSuffixStatValues(int aHash, ReadOnlySpan<StatBonuses> aSuffixStats, double aAvailablePoweredBudget, int aSelectableTemplateCount)
        {
            int statCount = aSuffixStats.Length;
            int[] statValues = new int[statCount];
            if (statCount == 0 || !IsTemplateRollable(aSuffixStats, aAvailablePoweredBudget))
            {
                return statValues;
            }

            double perStatPoweredBudget = aAvailablePoweredBudget / statCount;
            double weightedStatValue = Math.Pow(perStatPoweredBudget, ItemValuePower);

            int[] lowRoll = new int[statCount];
            int[] highRoll = new int[statCount];
            int[] variableIndices = new int[statCount];
            int variableCount = 0;
            for (int i = 0; i < statCount; i++)
            {
                double rawStatValue = weightedStatValue / (double)aSuffixStats[i];
                lowRoll[i] = (int)Math.Floor(rawStatValue);
                highRoll[i] = rawStatValue > lowRoll[i] ? lowRoll[i] + 1 : lowRoll[i];
                statValues[i] = lowRoll[i];

                if (highRoll[i] > lowRoll[i])
                {
                    variableIndices[variableCount] = i;
                    variableCount++;
                }
            }

            if (variableCount == 0)
            {
                return statValues;
            }

            if (variableCount > 30)
            {
                throw new InvalidOperationException("Suffix generator supports at most 30 variable stats.");
            }

            uint selectableCount = (uint)Math.Max(1, aSelectableTemplateCount);
            int variantSeed = (int)(unchecked((uint)aHash) / selectableCount);
            int variantCount = 1 << variableCount;
            int variantIndex = PositiveModulo(variantSeed, variantCount);

            for (int bitIndex = 0; bitIndex < variableCount; bitIndex++)
            {
                int statIndex = variableIndices[bitIndex];
                if ((variantIndex & (1 << bitIndex)) != 0)
                {
                    statValues[statIndex] = highRoll[statIndex];
                }
            }

            double selectedPoweredBudget = ComputePoweredStatSum(statValues, aSuffixStats);
            if (selectedPoweredBudget <= aAvailablePoweredBudget)
            {
                return statValues;
            }

            int startIndex = PositiveModulo(variantSeed, variableCount);
            for (int offset = 0; offset < variableCount && selectedPoweredBudget > aAvailablePoweredBudget; offset++)
            {
                int statIndex = variableIndices[(startIndex + offset) % variableCount];
                if (statValues[statIndex] == highRoll[statIndex] && highRoll[statIndex] > lowRoll[statIndex])
                {
                    statValues[statIndex] = lowRoll[statIndex];
                    selectedPoweredBudget = ComputePoweredStatSum(statValues, aSuffixStats);
                }
            }

            return statValues;
        }

        double GetAvailableSuffixPoweredBudget()
        {
            return RequiredPoweredSumArgument(this) - ComputePoweredStatSum(this);
        }

        (int templateIndex, int selectableTemplateCount) SelectSuffixTemplateIndex(int aHash, double aAvailablePoweredBudget)
        {
            int[] rollableTemplates = GetRollableSuffixTemplateIndices(aAvailablePoweredBudget);
            if (rollableTemplates.Length == 0)
            {
                return (FindLowestBudgetTemplateIndex(), 1);
            }

            int selectedIndex = (int)(unchecked((uint)aHash) % (uint)rollableTemplates.Length);
            return (rollableTemplates[selectedIndex], rollableTemplates.Length);
        }

        int[] GetRollableSuffixTemplateIndices(double aAvailablePoweredBudget)
        {
            int[] templateIndices = new int[suffixTemplates.Length];
            int count = 0;
            for (int i = 0; i < suffixTemplates.Length; i++)
            {
                if (!IsTemplateRollable(suffixTemplates[i].stats, aAvailablePoweredBudget))
                {
                    continue;
                }

                templateIndices[count] = i;
                count++;
            }

            if (count == 0)
            {
                return Array.Empty<int>();
            }

            int[] result = new int[count];
            Array.Copy(templateIndices, result, count);
            return result;
        }

        int FindLowestBudgetTemplateIndex()
        {
            int lowestIndex = 0;
            double lowestRequiredBudget = double.MaxValue;
            for (int i = 0; i < suffixTemplates.Length; i++)
            {
                double requiredBudget = GetMinimumRequiredPoweredBudget(suffixTemplates[i].stats);
                if (requiredBudget < lowestRequiredBudget)
                {
                    lowestRequiredBudget = requiredBudget;
                    lowestIndex = i;
                }
            }

            return lowestIndex;
        }

        static bool IsTemplateRollable(ReadOnlySpan<StatBonuses> aSuffixStats, double aAvailablePoweredBudget)
        {
            if (aSuffixStats.Length == 0 || aAvailablePoweredBudget <= 0)
            {
                return false;
            }

            double perStatPoweredBudget = aAvailablePoweredBudget / aSuffixStats.Length;
            if (perStatPoweredBudget <= 0)
            {
                return false;
            }

            double weightedStatValue = Math.Pow(perStatPoweredBudget, ItemValuePower);
            for (int i = 0; i < aSuffixStats.Length; i++)
            {
                double rawStatValue = weightedStatValue / (double)aSuffixStats[i];
                if (rawStatValue < MinimumRollableSuffixValue)
                {
                    return false;
                }
            }

            return true;
        }

        static double GetMinimumRequiredPoweredBudget(ReadOnlySpan<StatBonuses> aSuffixStats)
        {
            double requiredBudget = 0;
            for (int i = 0; i < aSuffixStats.Length; i++)
            {
                double weightedMinimum = MinimumRollableSuffixValue * (double)aSuffixStats[i];
                requiredBudget += Math.Pow(weightedMinimum, StatTermPower);
            }

            return requiredBudget;
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
