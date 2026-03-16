using Newtonsoft.Json;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Stats;
using System;

namespace Project_1.Items.SubTypes
{
    internal partial class EquipmentData : ItemData
    {
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

        public Equipment.GearType Material => material;
        Equipment.GearType material;

        public Equipment.Type Slot => slot;
        Equipment.Type slot;

        public EquipmentData(int id, string gfxName, string name, string description, Equipment.Type slot, ItemType itemType, Item.Quality quality, int cost, Equipment.GearType material, SecondayStatBonus<int>[] secondaryStatsInt, SecondayStatBonus<float>[] secondaryStatsFloat, int itemLevel = 1)
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
        public EquipmentData(int id, string gfxName, string name, string description, Equipment.Type slot, Item.Quality quality, int cost, Equipment.GearType material, SecondayStatBonus<int>[] secondaryStatsInt, SecondayStatBonus<float>[] secondayStatFloat, int itemLevel = 1)
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
    }
}
