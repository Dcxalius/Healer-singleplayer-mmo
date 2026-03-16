using System;
using System.Diagnostics;

namespace Project_1.Items.SubTypes
{
    internal partial class EquipmentData
    {
        const double MinimumRollableSuffixValue = 1.0;

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
            ("of Fiery Wrath", new[] { StatBonuses.FireSpellDamage }),
            ("of Arcane Wrath", new[] { StatBonuses.ArcaneSpellDamage }),
            ("of Nature's Wrath", new[] { StatBonuses.NatureSpellDamage }),
            ("of Shadow Wrath", new[] { StatBonuses.ShadowSpellDamage }),
            ("of Holy Wrath", new[] { StatBonuses.HolySpellDamage }),
            ("of Frozen Protection", new[] { StatBonuses.FrostResist }),
            ("of Fiery Protection", new[] { StatBonuses.FireResist }),
            ("of Arcane Protection", new[] { StatBonuses.ArcaneResist }),
            ("of Nature's Protection", new[] { StatBonuses.NatureResist }),
            ("of Shadow Protection", new[] { StatBonuses.ShadowResist }),
            ("of Holy Protection", new[] { StatBonuses.HolyResist })
        };

        static bool initialized;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;
            ValidateSuffixTemplates();
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
    }
}
