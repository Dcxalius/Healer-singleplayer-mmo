using Newtonsoft.Json;
using Project_1.GameObjects.Doodads;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Tiles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using static Project_1.Items.SubTypes.Equipment;

namespace Project_1.Items.SubTypes
{
    internal class EquipmentData : ItemData
    {
        
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

        //ItemValue = [(StatValue[1]*StatMod[1])^1.5 + (StatValue[2]*StatMod[2])^1.5 + ...]^2/3 / 100
        //ItemSlotValue = ItemValue * SlotMod
        //Green: ilvl = (ItemSlotValue + 9.8) / 1.21
        //Blue: ilvl = (ItemSlotValue + 4.2) / 1.42
        //Epic: ilvl = (ItemSlotValue - 11.2) / 1.64

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

        public (string name, (StatBonuses stat, int value)[] stats) Suffix(int aHash)
        {
            (string name, (StatBonuses stat, int value)[] stats) result;

            //TODO: Missing implentation of suffixes that aren't primary stat bonuses.
            switch (SuffixID(aHash))
            {
                case 0:
                    result.name = "of the Tiger";
                    result.stats = new (StatBonuses stat, int value)[2];
                    result.stats[0] = (StatBonuses.Strength, SuffixStatValue(0, aHash));
                    result.stats[1] = (StatBonuses.Agility, SuffixStatValue(1, aHash));
                    break;
                case 1:
                    result.name = "of the Bear";
                    result.stats = new (StatBonuses stat, int value)[2];
                    result.stats[0] = (StatBonuses.Strength, SuffixStatValue(0, aHash));
                    result.stats[1] = (StatBonuses.Stamina, SuffixStatValue(1, aHash));
                    break;
                case 2: 
                    result.name = "of the Gorilla";
                    result.stats = new (StatBonuses stat, int value)[2];
                    result.stats[0] = (StatBonuses.Strength, SuffixStatValue(0, aHash));
                    result.stats[1] = (StatBonuses.Intellect, SuffixStatValue(1, aHash));
                    break;
                case 3:
                    result.name = "of the Boar";
                    result.stats = new (StatBonuses stat, int value)[2];
                    result.stats[0] = (StatBonuses.Spirit, SuffixStatValue(0, aHash));
                    result.stats[1] = (StatBonuses.Strength, SuffixStatValue(1, aHash));
                    break;
                case 4:
                    result.name = "of the Monkey";
                    result.stats = new (StatBonuses stat, int value)[2];
                    result.stats[0] = (StatBonuses.Agility, SuffixStatValue(0, aHash));
                    result.stats[1] = (StatBonuses.Stamina, SuffixStatValue(1, aHash));
                    break;
                case 5:
                    result.name = "of the Falcon";
                    result.stats = new (StatBonuses stat, int value)[2];
                    result.stats[0] = (StatBonuses.Agility, SuffixStatValue(0, aHash));
                    result.stats[1] = (StatBonuses.Intellect, SuffixStatValue(1, aHash));
                    break;
                case 6:
                    result.name = "of the Wolf";
                    result.stats = new (StatBonuses stat, int value)[2];
                    result.stats[0] = (StatBonuses.Agility, SuffixStatValue(0, aHash));
                    result.stats[1] = (StatBonuses.Spirit, SuffixStatValue(1, aHash));
                    break;
                case 7:
                    result.name = "of the Owl";
                    result.stats = new (StatBonuses stat, int value)[2];
                    result.stats[0] = (StatBonuses.Intellect, SuffixStatValue(0, aHash));
                    result.stats[1] = (StatBonuses.Spirit, SuffixStatValue(1, aHash));
                    break;
                case 8:
                    result.name = "of the Whale";
                    result.stats = new (StatBonuses stat, int value)[2];
                    result.stats[0] = (StatBonuses.Stamina, SuffixStatValue(0, aHash));
                    result.stats[1] = (StatBonuses.Strength, SuffixStatValue(1, aHash));
                    break;

                case 9:
                    result.name = "of the Eagle";
                    result.stats = new (StatBonuses stat, int value)[2];
                    result.stats[0] = (StatBonuses.Intellect, SuffixStatValue(0, aHash));
                    result.stats[1] = (StatBonuses.Stamina, SuffixStatValue(1, aHash));
                    break;
                case 10:
                    result.name = "of Agility";
                    result.stats = new (StatBonuses stat, int value)[1];
                    result.stats[0] = (StatBonuses.Agility, SuffixStatValue(0, aHash));
                    break;
                case 11:
                    result.name = "of Strength";
                    result.stats = new (StatBonuses stat, int value)[1];
                    result.stats[0] = (StatBonuses.Strength, SuffixStatValue(0, aHash));
                    break;
                case 12:
                    result.name = "of Intellect";
                    result.stats = new (StatBonuses stat, int value)[1];
                    result.stats[0] = (StatBonuses.Intellect, SuffixStatValue(0, aHash));
                    break;
                case 13:
                    result.name = "of Stamina";
                    result.stats = new (StatBonuses stat, int value)[1];
                    result.stats[0] = (StatBonuses.Stamina, SuffixStatValue(0, aHash));
                    break;
                case 14:
                    result.name = "of Spirit";
                    result.stats = new (StatBonuses stat, int value)[1];
                    result.stats[0] = (StatBonuses.Spirit, SuffixStatValue(0, aHash));
                    break;
                default: 
                    throw new Exception($"Invalid suffix ID {SuffixID(aHash)}");
            }

            return result;
        }

        public int SuffixID(int aHash)
        {
            return aHash % 15; //last case in suffix + 1
        }

        public int SuffixStatValue(int aStatIndex, int aHash)
        {
            int ItemLevel;
            //RequiredStatValuesEqualContribution();
            return aHash + aStatIndex % 1337;
        }
        public EquipmentStats BaseStats => baseStats;
        EquipmentStats baseStats;

        public GearType Material => material;
        GearType material;

        public Equipment.Type Slot { get => slot; }
        Equipment.Type slot;

        public EquipmentData(int id, string gfxName, string name, string description, Equipment.Type slot, ItemType itemType, int armor, int[] baseStats, Item.Quality quality, int cost, GearType material, SecondayStatBonus<int>[] secondaryStatsInt, SecondayStatBonus<float>[] secondaryStatsFloat, int itemLevel = 1) : base(id, gfxName, name, description, 1, itemType, quality, cost, itemLevel)
        {
            this.slot = slot;
            //this.baseStats = new EquipmentStats(baseStats);
            //DEBUG
            this.baseStats = new EquipmentStats(baseStats, armor);
            this.material = material;
            this.secondaryStatsInt = secondaryStatsInt;
            this.secondaryStatsFloat = secondaryStatsFloat;
        }

        [JsonConstructor]
        public EquipmentData(int id, string gfxName, string name, string description, Equipment.Type slot, int armor, int[] baseStats, Item.Quality quality, int cost, GearType material, SecondayStatBonus<int>[] secondaryStatsInt, SecondayStatBonus<float>[] secondayStatFloat, int itemLevel = 1) : this(id, gfxName, name, description, slot, ItemType.Equipment, armor, baseStats, quality, cost, material, secondaryStatsInt, secondayStatFloat, itemLevel) { }



        //Copypasted from chatgpt, needs standardizing
        public static double ComputeItemValue(ReadOnlySpan<double> statValues, ReadOnlySpan<double> statMods)
        {
            if (statValues.Length != statMods.Length)
                throw new ArgumentException("statValues and statMods must have the same length.");
            if (statValues.Length == 0)
                throw new ArgumentException("At least one stat is required.");

            // sum = Σ (StatValue[i] * StatMod[i])^1.5
            double sum = 0.0;
            for (int i = 0; i < statValues.Length; i++)
            {
                double x = statValues[i] * statMods[i];
                if (x < 0) throw new ArgumentException("StatValue[i] * StatMod[i] must be non-negative.");
                sum += Math.Pow(x, 1.5);
            }

            // ItemValue = (sum)^(2/3) / 100
            return Math.Pow(sum, 2.0 / 3.0) / 100.0;
        }

        public static double ComputeItemSlotValue(ReadOnlySpan<double> statValues, ReadOnlySpan<double> statMods, double slotMod)
            => ComputeItemValue(statValues, statMods) * slotMod;

        static double ComputeItemLevel(ReadOnlySpan<double> statValues, ReadOnlySpan<double> statMods, double slotMod, Item.Quality rarity)
        {
            double itemSlotValue = ComputeItemSlotValue(statValues, statMods, slotMod);
            return rarity switch
            {
                Item.Quality.Uncommon => (itemSlotValue + 9.8) / 1.21,
                Item.Quality.Rare => (itemSlotValue + 4.2) / 1.42,
                Item.Quality.Epic => (itemSlotValue - 11.2) / 1.64,
                _ => throw new ArgumentOutOfRangeException(nameof(rarity))
            };
        }

        // ---------- Inverse: target rarity + ilvl -> required powered-sum argument ----------

        /// <summary>
        /// Returns S = Σ (StatValue[i] * StatMod[i])^1.5 required to reach the target ilvl,
        /// given rarity and slotMod. This is the "argument" inside the 2/3 power (before /100).
        /// </summary>
        static double RequiredPoweredSumArgument(EquipmentData aEq)
        {
            Item.Quality rarity = aEq.Quality;
            double itemLevel = aEq.ItemLevel;
            var slotMod = aEq.Slot switch
            {
                Equipment.Type.Head or Equipment.Type.Chest or Equipment.Type.Legs => 1.0,
                Equipment.Type.Shoulders or Equipment.Type.Hands or Equipment.Type.Belt or Equipment.Type.Feet => 1.35,
                Equipment.Type.Trinket => 1.47,
                Equipment.Type.Wrist or Equipment.Type.Neck or Equipment.Type.Back or Equipment.Type.Finger => 1.85,
                Equipment.Type.TwoHander => 1.0,
                Equipment.Type.OneHander or Equipment.Type.MainHander => 2.44,
                Equipment.Type.Ranged => 3.33,
                Equipment.Type.OffHander => 1.92,
                _ => throw new ArgumentOutOfRangeException($"Unhandled equipment slot {aEq.Slot}"),
            };

            // Invert ilvl = (ItemSlotValue + A) / B   (with sign handled per rarity)
            // => ItemSlotValue = ilvl * B - A
            double itemSlotValue = rarity switch
            {
                Item.Quality.Uncommon => itemLevel * 1.21 - 9.8,
                Item.Quality.Rare => itemLevel * 1.42 - 4.2,
                Item.Quality.Epic => itemLevel * 1.64 + 11.2, // because (ItemSlotValue - 11.2)/1.64
                _ => throw new ArgumentOutOfRangeException(nameof(rarity))
            };

            // ItemValue = ItemSlotValue / SlotMod
            double itemValue = itemSlotValue / slotMod;

            // ItemValue = (S)^(2/3) / 100
            // => (S)^(2/3) = ItemValue * 100
            // => S = (ItemValue * 100)^(3/2)
            double t = itemValue * 100.0;
            if (t < 0) throw new ArgumentException("Computed ItemValue*100 is negative; target ilvl/slotMod implies impossible stats under this model.");

            return Math.Pow(t, 1.5);
        }

        /// <summary>
        /// OPTIONAL helper: produces one concrete statValues vector by assuming each term contributes equally:
        /// (StatValue[i] * StatMod[i])^1.5 = S / n
        /// => StatValue[i] = (S/n)^(2/3) / StatMod[i]
        /// </summary>
        static double[] RequiredStatValuesEqualContribution(EquipmentData aEq, ReadOnlySpan<EquipmentData.StatBonuses> statMods)
        {
            if (statMods.Length == 0) throw new ArgumentException("statMods must have at least one entry.", nameof(statMods));
            for (int i = 0; i < statMods.Length; i++)
                if (statMods[i] <= 0) throw new ArgumentException("Each statMod must be > 0 to invert per-stat.", nameof(statMods));

            double s = RequiredPoweredSumArgument(aEq);
            double perTerm = s / statMods.Length;

            double[] statValues = new double[statMods.Length];
            double baseX = Math.Pow(perTerm, 2.0 / 3.0); // this equals (StatValue[i]*StatMod[i])
            for (int i = 0; i < statMods.Length; i++)
                statValues[i] = baseX / (double)statMods[i];

            //TODO: There could be statValues that will end up negative if the caller tries to use this with a statMod that is very large relative to the others.
            //This needs to be handled.


            return statValues;
        }
    }
}
