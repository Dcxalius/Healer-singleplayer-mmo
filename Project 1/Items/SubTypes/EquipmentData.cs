using Newtonsoft.Json;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Stats;
using System;
using System.Diagnostics;
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
            ("of Power", new[] { StatBonuses.AttackPower }),
            ("of Frozen Wrath", new[] { StatBonuses.FrostSpellDamage }),
            ("of Fiery Wrath", new[] { StatBonuses.FireSpellDamage}),
            ("of Arcane Wrath", new[] { StatBonuses.ArcaneSpellDamage}),
            ("of Nature's Wrath", new[] { StatBonuses.NatureSpellDamage}),
            ("of Shadow Wrath", new[] { StatBonuses.ShadowSpellDamage }),
            ("of Holy Wrath", new[] {StatBonuses.HolySpellDamage} ),
            ("of Frozen Protection", new[] { StatBonuses.FrostResist }),
            ("of Fiery Protection", new[] { StatBonuses.FireResist }),
            ("of Arcane Protection", new[] { StatBonuses.ArcaneResist }),
            ("of Nature's Protection", new[] { StatBonuses.NatureResist }),
            ("of Shadow Protection", new[] { StatBonuses.ShadowResist }),
            ("of Holy Protection", new[] { StatBonuses.HolyResist })
        };

        static EquipmentData()
        {
            ValidateSuffixTemplates();
        }

        public enum StatBonuses
        {
            BonusArmor,
            AttackPowerVsDemonsBeastsUndead,
            RangedAttackPower,
            SpellHealing,
            AttackPower,
            BlockingValue,
            FrostSpellDamage,
            FireSpellDamage,
            ArcaneSpellDamage,
            NatureSpellDamage,
            ShadowSpellDamage,
            HolySpellDamage,
            SpellDamageAllSpells,
            FireResist,
            FrostResist,
            ArcaneResist,
            NatureResist,
            ShadowResist,
            HolyResist,
            Agility,
            Strength,
            Stamina,
            Intellect,
            Spirit,
            Defense,
            HpRegenPer5Sec,
            ManaRegenPer5Sec,
            WeaponSkillOther,
            WeaponSkillDaggers,
            ShieldThorns,
            ChanceToBlock,
            ChanceToHit,
            ChanceToHitWithAllSpells,
            ChanceToDodge,
            ChanceToCritWithAllSpells,
            ChanceToCrit,
            ChanceToParry
        }

        public static bool IsValidSuffixDefinition((string name, (StatBonuses stat, int value)[] stats) aSuffix, out string aError)
        {
            if (string.IsNullOrWhiteSpace(aSuffix.name))
            {
                aError = "Suffix name is empty.";
                return false;
            }

            int templateIndex = FindSuffixTemplateIndexByName(aSuffix.name);
            if (templateIndex < 0)
            {
                aError = $"Unknown suffix template name '{aSuffix.name}'.";
                return false;
            }

            ReadOnlySpan<StatBonuses> templateStats = suffixTemplates[templateIndex].stats;
            if (aSuffix.stats == null || aSuffix.stats.Length != templateStats.Length)
            {
                aError = $"Suffix stat count mismatch for '{aSuffix.name}'.";
                return false;
            }

            for (int i = 0; i < templateStats.Length; i++)
            {
                if (aSuffix.stats[i].stat != templateStats[i])
                {
                    aError = $"Suffix stat mismatch for '{aSuffix.name}' at index {i}. Expected {templateStats[i]}, got {aSuffix.stats[i].stat}.";
                    return false;
                }

                if (aSuffix.stats[i].value <= 0)
                {
                    aError = $"Suffix stat '{aSuffix.stats[i].stat}' on '{aSuffix.name}' rolled non-positive value {aSuffix.stats[i].value}.";
                    return false;
                }
            }

            aError = string.Empty;
            return true;
        }

        public static string GetStatBonusDisplayName(StatBonuses aStatBonus)
        {
            return aStatBonus switch
            {
                StatBonuses.BonusArmor => "Bonus Armor",
                StatBonuses.AttackPowerVsDemonsBeastsUndead => "Attack Power vs Demons/Beasts/Undead",
                StatBonuses.RangedAttackPower => "Ranged Attack Power",
                StatBonuses.SpellHealing => "Spell Healing",
                StatBonuses.AttackPower => "Attack Power",
                StatBonuses.BlockingValue => "Blocking Value",
                StatBonuses.FrostSpellDamage => "Frost Spelldamage",
                StatBonuses.FireSpellDamage => "Fire Spelldamage",
                StatBonuses.ArcaneSpellDamage => "Arcane Spelldamage",
                StatBonuses.NatureSpellDamage => "Nature Spelldamage",
                StatBonuses.ShadowSpellDamage => "Shadow Spelldamage",
                StatBonuses.HolySpellDamage => "Holy Spelldamage",
                StatBonuses.SpellDamageAllSpells => "Spelldamage",
                StatBonuses.FireResist => "Fire Resist",
                StatBonuses.FrostResist => "Frost Resist",
                StatBonuses.ArcaneResist => "Arcane Resist",
                StatBonuses.NatureResist => "Nature Resist",
                StatBonuses.ShadowResist => "Shadow Resist",
                StatBonuses.HolyResist => "Holy Resist",
                StatBonuses.Agility => "Agility",
                StatBonuses.Strength => "Strength",
                StatBonuses.Stamina => "Stamina",
                StatBonuses.Intellect => "Intellect",
                StatBonuses.Spirit => "Spirit",
                StatBonuses.Defense => "Defense",
                StatBonuses.HpRegenPer5Sec => "HP/5",
                StatBonuses.ManaRegenPer5Sec => "Mana/5",
                StatBonuses.WeaponSkillOther => "Weapon Skill", //TODO: Break this out to all weapons, both here and in the enum
                StatBonuses.WeaponSkillDaggers => "Dagger Skill",
                StatBonuses.ShieldThorns => "Refelect",
                StatBonuses.ChanceToBlock => "Chance To Block",
                StatBonuses.ChanceToHit => "Chance To Hit",
                StatBonuses.ChanceToHitWithAllSpells => "Spell Hit Chance",
                StatBonuses.ChanceToDodge => "Chance To Dodge",
                StatBonuses.ChanceToCritWithAllSpells => "Spell Crit Chance",
                StatBonuses.ChanceToCrit => "Chance To Crit",
                StatBonuses.ChanceToParry => "Chance To Parry",
                _ => aStatBonus.ToString()
            };
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

        public EquipmentData(int id, string gfxName, string name, string description, Equipment.Type slot, ItemType itemType, Item.Quality quality, int cost, GearType material, SecondayStatBonus<int>[] secondaryStatsInt, SecondayStatBonus<float>[] secondaryStatsFloat, int itemLevel = 1)
            : base(id, gfxName, name, description, 1, itemType, quality, cost, itemLevel)
        {
            this.slot = slot;
            int calculatedArmor = itemType == ItemType.Equipment
                ? CalculateArmorFromFormula(slot, material, quality, itemLevel)
                : 0;
            SetBaseStats(Array.Empty<int>(), calculatedArmor);
            this.material = material;
            // TODO: Rare/epic itemization currently has no template-level load path for these arrays.
            // Keep template secondary stats empty while green suffix stats are sourced from Equipment instances.
            this.secondaryStatsInt = Array.Empty<SecondayStatBonus<int>>();
            this.secondaryStatsFloat = Array.Empty<SecondayStatBonus<float>>();
        }

        [JsonConstructor]
        public EquipmentData(int id, string gfxName, string name, string description, Equipment.Type slot, Item.Quality quality, int cost, GearType material, SecondayStatBonus<int>[] secondaryStatsInt, SecondayStatBonus<float>[] secondayStatFloat, int itemLevel = 1)
            : this(id, gfxName, name, description, slot, ItemType.Equipment, quality, cost, material, secondaryStatsInt, secondayStatFloat, itemLevel)
        {
        }

        protected void SetBaseStats(int[] aBaseStats, int aArmor)
        {
            baseStats = new EquipmentStats(NormalizePrimaryStats(aBaseStats), Math.Max(0, aArmor));
        }

        static int[] NormalizePrimaryStats(int[] aBaseStats)
        {
            int expectedLength = (int)PrimaryStats.PrimaryStat.Count;
            int[] normalized = new int[expectedLength];
            if (aBaseStats == null)
            {
                return normalized;
            }

            int copyLength = Math.Min(expectedLength, aBaseStats.Length);
            Array.Copy(aBaseStats, normalized, copyLength);
            return normalized;
        }

        public (string name, (StatBonuses stat, int value)[] stats) Suffix(int aHash)
        {
            double availablePoweredBudget = GetAvailableSuffixPoweredBudget();
            var selected = SelectSuffixTemplateIndex(aHash, availablePoweredBudget);
            if (selected.templateIndex < 0)
            {
                return (string.Empty, Array.Empty<(StatBonuses stat, int value)>());
            }

            var template = suffixTemplates[selected.templateIndex];
            int selectableTemplateCount = selected.selectableTemplateCount;
            int[] statValues = selected.isForcedMinimumFallback
                ? new[] { 1 }
                : CalculateSuffixStatValues(aHash, template.stats, availablePoweredBudget, selectableTemplateCount);
            var statLines = new (StatBonuses stat, int value)[template.stats.Length];
            for (int i = 0; i < template.stats.Length; i++)
            {
                statLines[i] = (template.stats[i], statValues[i]);
            }

            var generatedSuffix = (template.name, statLines);
            if (!IsValidSuffixDefinition(generatedSuffix, out string suffixError))
            {
                throw new InvalidOperationException($"Generated invalid suffix: {suffixError}");
            }

            return generatedSuffix;
        }

        public int SuffixID(int aHash)
        {
            return SelectSuffixTemplateIndex(aHash, GetAvailableSuffixPoweredBudget()).templateIndex;
        }

        public int SuffixStatValue(int aStatIndex, int aHash)
        {
            double availablePoweredBudget = GetAvailableSuffixPoweredBudget();
            var selected = SelectSuffixTemplateIndex(aHash, availablePoweredBudget);
            if (selected.templateIndex < 0)
            {
                return 0;
            }

            var template = suffixTemplates[selected.templateIndex];
            int selectableTemplateCount = selected.selectableTemplateCount;
            if ((uint)aStatIndex >= (uint)template.stats.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(aStatIndex), aStatIndex, "Invalid suffix stat index.");
            }

            if (selected.isForcedMinimumFallback)
            {
                return aStatIndex == 0 ? 1 : 0;
            }

            int value = CalculateSuffixStatValues(aHash, template.stats, availablePoweredBudget, selectableTemplateCount)[aStatIndex];
            if (value <= 0)
            {
                throw new InvalidOperationException($"Suffix stat '{template.stats[aStatIndex]}' rolled invalid value {value}.");
            }

            return value;
        }

        [Conditional("DEBUG")]
        public void AssertSuffixDeterminism(int aHash)
        {
            var first = Suffix(aHash);
            var second = Suffix(aHash);
            if (!SuffixesEqual(first, second))
            {
                throw new InvalidOperationException("Suffix generation is non-deterministic for the same hash.");
            }

            int suffixId = SuffixID(aHash);
            if (suffixId >= 0)
            {
                string expectedName = suffixTemplates[suffixId].name;
                if (!string.Equals(first.name, expectedName, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"Suffix ID/name mismatch for hash {aHash}. Expected '{expectedName}', got '{first.name}'.");
                }
            }

            for (int i = 0; i < first.stats.Length; i++)
            {
                int expectedValue = SuffixStatValue(i, aHash);
                if (first.stats[i].value != expectedValue)
                {
                    throw new InvalidOperationException($"Suffix stat value mismatch for hash {aHash} at index {i}. Expected {expectedValue}, got {first.stats[i].value}.");
                }
            }
        }

        static bool SuffixesEqual(
            (string name, (StatBonuses stat, int value)[] stats) aLeft,
            (string name, (StatBonuses stat, int value)[] stats) aRight)
        {
            if (!string.Equals(aLeft.name, aRight.name, StringComparison.Ordinal))
            {
                return false;
            }

            if ((aLeft.stats?.Length ?? 0) != (aRight.stats?.Length ?? 0))
            {
                return false;
            }

            for (int i = 0; i < aLeft.stats.Length; i++)
            {
                if (aLeft.stats[i].stat != aRight.stats[i].stat || aLeft.stats[i].value != aRight.stats[i].value)
                {
                    return false;
                }
            }

            return true;
        }

        static int FindSuffixTemplateIndexByName(string aSuffixName)
        {
            for (int i = 0; i < suffixTemplates.Length; i++)
            {
                if (string.Equals(suffixTemplates[i].name, aSuffixName, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }

        static void ValidateSuffixTemplates()
        {
            for (int i = 0; i < suffixTemplates.Length; i++)
            {
                string name = suffixTemplates[i].name;
                ReadOnlySpan<StatBonuses> stats = suffixTemplates[i].stats;

                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new InvalidOperationException($"Suffix template at index {i} has an empty name.");
                }

                if (stats.Length == 0)
                {
                    throw new InvalidOperationException($"Suffix template '{name}' has no stats.");
                }

                for (int statIndex = 0; statIndex < stats.Length; statIndex++)
                {
                    for (int checkIndex = statIndex + 1; checkIndex < stats.Length; checkIndex++)
                    {
                        if (stats[statIndex] == stats[checkIndex])
                        {
                            throw new InvalidOperationException($"Suffix template '{name}' repeats stat '{stats[statIndex]}'.");
                        }
                    }
                }

                if (stats.Length == 1)
                {
                    string canonicalSingleName = GetCanonicalSingleStatSuffixName(stats[0]);
                    if (canonicalSingleName != null && !string.Equals(name, canonicalSingleName, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException($"Suffix template '{name}' should be named '{canonicalSingleName}' for stat '{stats[0]}'.");
                    }
                }

                if (name.StartsWith("of the ", StringComparison.Ordinal))
                {
                    for (int statIndex = 0; statIndex < stats.Length; statIndex++)
                    {
                        if (!IsPrimaryStatBonus(stats[statIndex]))
                        {
                            throw new InvalidOperationException($"Suffix template '{name}' uses non-primary stat '{stats[statIndex]}' in an 'of the' family.");
                        }
                    }
                }
            }
        }

        static bool IsPrimaryStatBonus(StatBonuses aStatBonus)
        {
            return aStatBonus is StatBonuses.Agility
                or StatBonuses.Strength
                or StatBonuses.Stamina
                or StatBonuses.Intellect
                or StatBonuses.Spirit;
        }

        static string GetCanonicalSingleStatSuffixName(StatBonuses aStatBonus)
        {
            return aStatBonus switch
            {
                StatBonuses.Agility => "of Agility",
                StatBonuses.Strength => "of Strength",
                StatBonuses.Intellect => "of Intellect",
                StatBonuses.Stamina => "of Stamina",
                StatBonuses.Spirit => "of Spirit",
                StatBonuses.AttackPower => "of Power",
                StatBonuses.FrostSpellDamage => "of Frozen Wrath",
                StatBonuses.FireSpellDamage => "of Fiery Wrath",
                StatBonuses.ArcaneSpellDamage => "of Arcane Wrath",
                StatBonuses.NatureSpellDamage => "of Nature's Wrath",
                StatBonuses.ShadowSpellDamage => "of Shadow Wrath",
                StatBonuses.HolySpellDamage => "of Holy Wrath",
                StatBonuses.FrostResist => "of Frozen Protection",
                StatBonuses.FireResist => "of Fiery Protection",
                StatBonuses.ArcaneResist => "of Arcane Protection",
                StatBonuses.NatureResist => "of Nature's Protection",
                StatBonuses.ShadowResist => "of Shadow Protection",
                StatBonuses.HolyResist => "of Holy Protection",
                _ => null
            };
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

        static int CalculateArmorFromFormula(Equipment.Type aSlot, GearType aMaterial, Item.Quality aQuality, int aItemLevel)
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

        static double GetArmorClassBase(Equipment.Type aSlot, GearType aMaterial, int aItemLevel)
        {
            // Cloaks are treated as cloth for armor in this model.
            if (aSlot == Equipment.Type.Back)
            {
                return 1.19 * aItemLevel + 5.1;
            }

            return aMaterial switch
            {
                GearType.Cloth => 1.19 * aItemLevel + 5.1,
                GearType.Leather => 2.22 * aItemLevel + 10,
                GearType.Mail => 4.9 * aItemLevel + 29,
                GearType.Plate => 9.0 * aItemLevel + 23,
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
                double rawStatValue = weightedStatValue / GetStatModifierValue(aSuffixStats[i]);
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
                EnsurePositiveSuffixValues(statValues, aSuffixStats);
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
                EnsurePositiveSuffixValues(statValues, aSuffixStats);
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

            EnsurePositiveSuffixValues(statValues, aSuffixStats);
            return statValues;
        }

        static void EnsurePositiveSuffixValues(ReadOnlySpan<int> aStatValues, ReadOnlySpan<StatBonuses> aSuffixStats)
        {
            for (int i = 0; i < aStatValues.Length; i++)
            {
                if (aStatValues[i] <= 0)
                {
                    throw new InvalidOperationException($"Suffix stat '{aSuffixStats[i]}' rolled invalid value {aStatValues[i]}.");
                }
            }
        }

        double GetAvailableSuffixPoweredBudget()
        {
            return RequiredPoweredSumArgument(this) - ComputePoweredStatSum(this);
        }

        (int templateIndex, int selectableTemplateCount, bool isForcedMinimumFallback) SelectSuffixTemplateIndex(int aHash, double aAvailablePoweredBudget)
        {
            int[] rollableTemplates = GetRollableSuffixTemplateIndices(aAvailablePoweredBudget);
            if (rollableTemplates.Length == 0)
            {
                int[] minimumFallbackTemplates = GetMinimumFallbackSuffixTemplateIndices();
                if (minimumFallbackTemplates.Length == 0)
                {
                    return (-1, 0, false);
                }

                int selectedFallbackIndex = (int)(unchecked((uint)aHash) % (uint)minimumFallbackTemplates.Length);
                return (minimumFallbackTemplates[selectedFallbackIndex], minimumFallbackTemplates.Length, true);
            }

            int selectedIndex = (int)(unchecked((uint)aHash) % (uint)rollableTemplates.Length);
            return (rollableTemplates[selectedIndex], rollableTemplates.Length, false);
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

        int[] GetMinimumFallbackSuffixTemplateIndices()
        {
            int[] templateIndices = new int[suffixTemplates.Length];
            int count = 0;
            for (int i = 0; i < suffixTemplates.Length; i++)
            {
                if (!IsMinimumFallbackTemplate(suffixTemplates[i].stats))
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

        static bool IsMinimumFallbackTemplate(ReadOnlySpan<StatBonuses> aSuffixStats)
        {
            if (aSuffixStats.Length != 1)
            {
                return false;
            }

            return IsAllowedMinimumFallbackStat(aSuffixStats[0]);
        }

        static bool IsAllowedMinimumFallbackStat(StatBonuses aStatBonus)
        {
            return aStatBonus switch
            {
                StatBonuses.Agility or
                StatBonuses.Strength or
                StatBonuses.Stamina or
                StatBonuses.Intellect or
                StatBonuses.Spirit or
                StatBonuses.FrostSpellDamage or
                StatBonuses.FireSpellDamage or
                StatBonuses.ArcaneSpellDamage or
                StatBonuses.NatureSpellDamage or
                StatBonuses.ShadowSpellDamage or
                StatBonuses.HolySpellDamage or
                StatBonuses.FrostResist or
                StatBonuses.FireResist or
                StatBonuses.ArcaneResist or
                StatBonuses.NatureResist or
                StatBonuses.ShadowResist or
                StatBonuses.HolyResist => true,
                _ => false
            };
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
                double rawStatValue = weightedStatValue / GetStatModifierValue(aSuffixStats[i]);
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
                double weightedMinimum = MinimumRollableSuffixValue * GetStatModifierValue(aSuffixStats[i]);
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
