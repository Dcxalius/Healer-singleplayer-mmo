using Newtonsoft.Json;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

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

        internal Equipment(EquipmentData aData, int aHash) : base(aData, 1)
        {
            hash = aHash;
            suffix = ItemQuality == Quality.Uncommon
                ? EquipmentData.Suffix(hash)
                : EmptySuffix();
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
            RefreshDerivedStatsFromSuffix();
        }

        public Equipment(EquipmentData aData) : base(aData, 1)
        {
            hash = RandomManager.RollInt();
            suffix = ItemQuality == Quality.Uncommon
                ? EquipmentData.Suffix(hash)
                : EmptySuffix();
            RefreshDerivedStatsFromSuffix();
        }

        static (string name, (EquipmentData.StatBonuses stat, int value)[] stats) EmptySuffix()
        {
            return (string.Empty, Array.Empty<(EquipmentData.StatBonuses stat, int value)>());
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

            stats = new EquipmentStats(combinedPrimaryStats, EquipmentData.BaseStats.Armor);
            secondaryStatsInt = secondaryInt
                .Select(x => new SecondayStatBonus<int>(x.Key, x.Value))
                .ToArray();
            secondaryStatsFloat = secondaryFloat
                .Select(x => new SecondayStatBonus<float>(x.Key, x.Value))
                .ToArray();
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
    }
}
