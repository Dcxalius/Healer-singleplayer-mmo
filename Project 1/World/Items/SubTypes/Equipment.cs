using Newtonsoft.Json;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Managers;
using Project_1.World.Items.Enchantments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Project_1.Items.SubTypes
{
    internal class Equipment : Item
    {
        public enum Type //TODO: Find better name
        {
            Head,
            Neck,
            Shoulders,
            Back,
            Chest,
            Wrist,
            Hands,
            Belt,
            Legs,
            Feet,
            Finger,
            Trinket,
            OneHander,
            MainHander,
            OffHander,
            TwoHander,
            Ranged,
            Count
        }

        public enum GearType
        {
            Cloth,
            Leather,
            Mail,
            Plate,
            Count,
            None
        }
        [JsonIgnore]
        public EquipmentData EquipmentData => itemData as EquipmentData;


        [JsonProperty]
        PermanentEnchantment permanentEnchantment;
        [JsonProperty]
        TemporaryEnchantment temporaryEnchantment;

        [JsonProperty]
        public int Hash => hash;
        int hash;

        [JsonIgnore]
        public override string Name
        {
            get
            {
                if (ItemQuality != Quality.Uncommon || string.IsNullOrEmpty(suffix.name)) return EquipmentData.Name;
                return $"{EquipmentData.Name} {suffix.name}";
            }
        }

        [JsonIgnore]
        public (string name, (EquipmentData.StatBonuses stat, int value)[] stats) Suffix => suffix;
        (string name, (EquipmentData.StatBonuses stat, int value)[] stats) suffix;

        [JsonIgnore]
        public EquipmentStats Stats => stats;
        EquipmentStats stats;

        [JsonIgnore]
        public SecondayStatBonus<int>[] SecondaryStatsInt => secondaryStatsInt;
        SecondayStatBonus<int>[] secondaryStatsInt = Array.Empty<SecondayStatBonus<int>>();

        [JsonIgnore]
        public SecondayStatBonus<float>[] SecondaryStatsFloat => secondaryStatsFloat;
        SecondayStatBonus<float>[] secondaryStatsFloat = Array.Empty<SecondayStatBonus<float>>();

        [JsonIgnore]
        public Type type { get => EquipmentData.Slot; }
        [JsonIgnore]
        public GearType Material => EquipmentData.Material;
        [JsonIgnore]
        public bool HasPermanentEnchantment => permanentEnchantment != null;
        [JsonIgnore]
        public bool HasBonusStatReport => BonusStatReport.Count > 0;
        [JsonIgnore]
        public PairReport StatReport
        {
            get
            {
                var report = EquipmentData.StatReport;
                if (ItemQuality == Item.Quality.Uncommon)
                {
                    var suffix = Suffix;
                    foreach (var stat in suffix.stats)
                    {
                        report.AddLine(Project_1.Items.SubTypes.EquipmentData.GetStatBonusDisplayName(stat.stat), stat.value);
                    }
                }
                return report;
            }
        }
        [JsonIgnore]
        public PairReport BonusStatReport
        {
            get
            {
                PairReport report = new PairReport();
                AppendEnchantmentStats(ref report, permanentEnchantment);
                AppendEnchantmentStats(ref report, temporaryEnchantment);
                return report;
            }
        }

        internal Equipment(EquipmentData aData, int aHash) : base(aData, 1)
        {
            hash = aHash;
            suffix = ItemQuality == Quality.Uncommon
                ? EquipmentData.Suffix(hash)
                : EmptySuffix();
            ValidateSuffixOrThrow();
            RefreshDerivedStatsFromSuffix();
        }

        [JsonConstructor]
        Equipment(int id, int hash) : this(ItemFactory.GetItemData<EquipmentData>(id), hash)
        {
        }


        public Equipment(LootData aLoot) : base(aLoot)
        {
            hash = RandomManager.RollInt();
            suffix = ItemQuality == Quality.Uncommon
                ? EquipmentData.Suffix(hash)
                : EmptySuffix();
            ValidateSuffixOrThrow();
            RefreshDerivedStatsFromSuffix();
        }

        public Equipment(EquipmentData aData) : base(aData, 1)
        {
            hash = RandomManager.RollInt();
            suffix = ItemQuality == Quality.Uncommon
                ? EquipmentData.Suffix(hash)
                : EmptySuffix();
            ValidateSuffixOrThrow();
            RefreshDerivedStatsFromSuffix();
        }

        static (string name, (EquipmentData.StatBonuses stat, int value)[] stats) EmptySuffix()
        {
            return (string.Empty, Array.Empty<(EquipmentData.StatBonuses stat, int value)>());
        }

        void ValidateSuffixOrThrow()
        {
            if (ItemQuality != Quality.Uncommon)
            {
                if (!string.IsNullOrEmpty(suffix.name) || (suffix.stats != null && suffix.stats.Length > 0))
                {
                    throw new InvalidOperationException($"Non-uncommon item '{EquipmentData.Name}' cannot have a suffix.");
                }

                return;
            }

            if (!EquipmentData.IsValidSuffixDefinition(suffix, out string suffixError))
            {
                throw new InvalidOperationException($"Invalid suffix on '{EquipmentData.Name}': {suffixError}");
            }

            EquipmentData.AssertSuffixDeterminism(hash);
        }

        void RefreshDerivedStatsFromSuffix()
        {
            int[] combinedPrimaryStats = EquipmentData.BaseStats.Stats;
            var secondaryInt = new Dictionary<string, int>(StringComparer.Ordinal);
            var secondaryFloat = new Dictionary<string, float>(StringComparer.Ordinal);

            AddSecondaryStats(secondaryInt, EquipmentData.SecondayStatsInt);
            AddSecondaryStats(secondaryFloat, EquipmentData.SecondayStatsFloat);

            for (int i = 0; i < suffix.stats.Length; i++)
            {
                var suffixStat = suffix.stats[i];
                if (TryGetPrimaryStatIndex(suffixStat.stat, out int primaryStatIndex))
                {
                    combinedPrimaryStats[primaryStatIndex] += suffixStat.value;
                    continue;
                }

                if (TryGetSecondaryIntStatName(suffixStat.stat, out string secondaryStatName))
                {
                    AddSecondaryStatValue(secondaryInt, secondaryStatName, suffixStat.value);
                }
            }

            for (int i = 0; i < combinedPrimaryStats.Length; i++)
            {
                combinedPrimaryStats[i] += permanentEnchantment?.PrimaryStats.Stats[i] ?? 0;
                combinedPrimaryStats[i] += temporaryEnchantment?.PrimaryStats.Stats[i] ?? 0;
            }

            AddSecondaryStats(secondaryInt, permanentEnchantment?.SecondaryStatsInt);
            AddSecondaryStats(secondaryFloat, permanentEnchantment?.SecondaryStatsFloat);
            AddSecondaryStats(secondaryInt, temporaryEnchantment?.SecondaryStatsInt);
            AddSecondaryStats(secondaryFloat, temporaryEnchantment?.SecondaryStatsFloat);

            stats = new EquipmentStats(combinedPrimaryStats, EquipmentData.BaseStats.Armor);
            secondaryStatsInt = secondaryInt
                .Select(x => new SecondayStatBonus<int>(x.Key, x.Value))
                .ToArray();
            secondaryStatsFloat = secondaryFloat
                .Select(x => new SecondayStatBonus<float>(x.Key, x.Value))
                .ToArray();

        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            suffix = ItemQuality == Quality.Uncommon
                ? EquipmentData.Suffix(hash)
                : EmptySuffix();
            ValidateSuffixOrThrow();
            RefreshDerivedStatsFromSuffix();
        }

        static void AddSecondaryStats(Dictionary<string, int> aTarget, SecondayStatBonus<int>[] aSource)
        {
            if (aSource == null)
            {
                return;
            }

            for (int i = 0; i < aSource.Length; i++)
            {
                AddSecondaryStatValue(aTarget, aSource[i].SecondaryStat, aSource[i].Value);
            }
        }

        static void AddSecondaryStats(Dictionary<string, float> aTarget, SecondayStatBonus<float>[] aSource)
        {
            if (aSource == null)
            {
                return;
            }

            for (int i = 0; i < aSource.Length; i++)
            {
                AddSecondaryStatValue(aTarget, aSource[i].SecondaryStat, aSource[i].Value);
            }
        }

        static void AddSecondaryStatValue(Dictionary<string, int> aTarget, string aName, int aValue)
        {
            if (string.IsNullOrWhiteSpace(aName))
            {
                return;
            }

            if (aTarget.TryGetValue(aName, out int existingValue))
            {
                aTarget[aName] = existingValue + aValue;
            }
            else
            {
                aTarget[aName] = aValue;
            }
        }

        static void AddSecondaryStatValue(Dictionary<string, float> aTarget, string aName, float aValue)
        {
            if (string.IsNullOrWhiteSpace(aName))
            {
                return;
            }

            if (aTarget.TryGetValue(aName, out float existingValue))
            {
                aTarget[aName] = existingValue + aValue;
            }
            else
            {
                aTarget[aName] = aValue;
            }
        }

        static bool TryGetPrimaryStatIndex(EquipmentData.StatBonuses aStatBonus, out int aStatIndex)
        {
            aStatIndex = aStatBonus switch
            {
                EquipmentData.StatBonuses.Strength => (int)PrimaryStats.PrimaryStat.Strength,
                EquipmentData.StatBonuses.Agility => (int)PrimaryStats.PrimaryStat.Agility,
                EquipmentData.StatBonuses.Intellect => (int)PrimaryStats.PrimaryStat.Intellect,
                EquipmentData.StatBonuses.Spirit => (int)PrimaryStats.PrimaryStat.Spirit,
                EquipmentData.StatBonuses.Stamina => (int)PrimaryStats.PrimaryStat.Stamina,
                _ => -1
            };

            return aStatIndex >= 0;
        }

        static bool TryGetSecondaryIntStatName(EquipmentData.StatBonuses aStatBonus, out string aStatName)
        {
            aStatName = aStatBonus switch
            {
                EquipmentData.StatBonuses.FrostSpellDamage => "FrostSpellDamage",
                EquipmentData.StatBonuses.FireSpellDamage => "FireSpellDamage",
                EquipmentData.StatBonuses.ArcaneSpellDamage => "ArcaneSpellDamage",
                EquipmentData.StatBonuses.NatureSpellDamage => "NatureSpellDamage",
                EquipmentData.StatBonuses.ShadowSpellDamage => "ShadowSpellDamage",
                EquipmentData.StatBonuses.HolySpellDamage => "HolySpellDamage",
                EquipmentData.StatBonuses.AttackPower => "AttackPower",
                EquipmentData.StatBonuses.SpellHealing => "HealingSpellDamage",
                EquipmentData.StatBonuses.FrostResist => "FrostResist",
                EquipmentData.StatBonuses.FireResist => "FireResist",
                EquipmentData.StatBonuses.ArcaneResist => "ArcaneResist",
                EquipmentData.StatBonuses.NatureResist => "NatureResist",
                EquipmentData.StatBonuses.ShadowResist => "ShadowResist",
                EquipmentData.StatBonuses.HolyResist => "HolyResist",
                _ => string.Empty
            };

            return !string.IsNullOrEmpty(aStatName);
        }

        static void AppendEnchantmentStats(ref PairReport report, Enchantment enchantment)
        {
            if (enchantment == null)
            {
                return;
            }

            int[] primaryStats = enchantment.PrimaryStats?.Stats;
            if (primaryStats != null)
            {
                for (int i = 0; i < primaryStats.Length; i++)
                {
                    if (primaryStats[i] == 0) continue;
                    report.AddLine(GetPrimaryStatDisplayName((PrimaryStats.PrimaryStat)i), primaryStats[i]);
                }
            }

            AppendSecondaryStats(ref report, enchantment.SecondaryStatsInt);
            AppendSecondaryStats(ref report, enchantment.SecondaryStatsFloat);
        }

        static void AppendSecondaryStats(ref PairReport report, SecondayStatBonus<int>[] secondaryStats)
        {
            if (secondaryStats == null)
            {
                return;
            }

            for (int i = 0; i < secondaryStats.Length; i++)
            {
                if (secondaryStats[i].Value == 0) continue;
                report.AddLine(GetSecondaryStatDisplayName(secondaryStats[i].SecondaryStat), secondaryStats[i].Value);
            }
        }

        static void AppendSecondaryStats(ref PairReport report, SecondayStatBonus<float>[] secondaryStats)
        {
            if (secondaryStats == null)
            {
                return;
            }

            for (int i = 0; i < secondaryStats.Length; i++)
            {
                if (secondaryStats[i].Value == 0f) continue;
                report.AddLine(GetSecondaryStatDisplayName(secondaryStats[i].SecondaryStat), secondaryStats[i].Value);
            }
        }

        static string GetPrimaryStatDisplayName(PrimaryStats.PrimaryStat primaryStat)
        {
            return primaryStat switch
            {
                PrimaryStats.PrimaryStat.Strength => "Strength",
                PrimaryStats.PrimaryStat.Agility => "Agility",
                PrimaryStats.PrimaryStat.Intellect => "Intellect",
                PrimaryStats.PrimaryStat.Spirit => "Spirit",
                PrimaryStats.PrimaryStat.Stamina => "Stamina",
                _ => primaryStat.ToString()
            };
        }

        static string GetSecondaryStatDisplayName(string secondaryStat)
        {
            if (string.IsNullOrWhiteSpace(secondaryStat))
            {
                return string.Empty;
            }

            return secondaryStat switch
            {
                "AttackPower" => "Attack Power",
                "HealingSpellDamage" => "Healing",
                "FrostSpellDamage" => "Frost Spell Damage",
                "FireSpellDamage" => "Fire Spell Damage",
                "ArcaneSpellDamage" => "Arcane Spell Damage",
                "NatureSpellDamage" => "Nature Spell Damage",
                "ShadowSpellDamage" => "Shadow Spell Damage",
                "HolySpellDamage" => "Holy Spell Damage",
                "FrostResist" => "Frost Resist",
                "FireResist" => "Fire Resist",
                "ArcaneResist" => "Arcane Resist",
                "NatureResist" => "Nature Resist",
                "ShadowResist" => "Shadow Resist",
                "HolyResist" => "Holy Resist",
                _ => SplitPascalCase(secondaryStat)
            };
        }

        static string SplitPascalCase(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length < 2)
            {
                return text ?? string.Empty;
            }

            StringBuilder builder = new StringBuilder(text.Length + 8);
            builder.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]) && !char.IsUpper(text[i - 1]))
                {
                    builder.Append(' ');
                }
                builder.Append(text[i]);
            }

            return builder.ToString();
        }

        internal void RemoveTemporaryEnchant()
        {
            temporaryEnchantment = null;

            RefreshDerivedStatsFromSuffix();
        }

        internal bool CanApplyPermanentEnchantment(EnchantmentData enchantmentData)
        {
            if (enchantmentData == null) return false;
            if (permanentEnchantment != null) return false;
            return enchantmentData.CanApplyTo(this);
        }

        internal bool ApplyPermanentEnchantment(EnchantmentData enchantmentData)
        {
            if (!CanApplyPermanentEnchantment(enchantmentData)) return false;

            permanentEnchantment = new PermanentEnchantment(enchantmentData);
            RefreshDerivedStatsFromSuffix();
            return true;
        }
    }
}
