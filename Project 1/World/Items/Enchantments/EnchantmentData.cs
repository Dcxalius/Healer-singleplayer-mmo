using Newtonsoft.Json;
using Project_1.Items.SubTypes;
using Project_1.World.GameObjects.Unit.Stats.Primary;
using System;

namespace Project_1.World.Items.Enchantments
{
    internal class EnchantmentData
    {
        public int Id => id;
        int id;

        public string Name => name;
        string name;

        public string Description => description;
        string description;

        public PrimaryStats PrimaryStats => primaryStats;
        PrimaryStats primaryStats;

        public SecondayStatBonus<int>[] SecondaryStatsInt => secondaryStatsInt;
        SecondayStatBonus<int>[] secondaryStatsInt;

        public SecondayStatBonus<float>[] SecondaryStatsFloat => secondaryStatsFloat;
        SecondayStatBonus<float>[] secondaryStatsFloat;

        //TODO: How should this handle weapons?
        public Equipment.Type? Target => target;
        Equipment.Type? target;

        [JsonConstructor]
        public EnchantmentData(
            int id,
            string name,
            string description = null,
            int[] primaryStats = null,
            SecondayStatBonus<int>[] secondaryStatsInt = null,
            SecondayStatBonus<float>[] secondaryStatsFloat = null,
            Equipment.Type? target = null)
        {
            this.id = id;
            this.name = name ?? string.Empty;
            this.description = description ?? string.Empty;
            this.primaryStats = new PrimaryStats(NormalizePrimaryStats(primaryStats));
            this.secondaryStatsInt = secondaryStatsInt ?? Array.Empty<SecondayStatBonus<int>>();
            this.secondaryStatsFloat = secondaryStatsFloat ?? Array.Empty<SecondayStatBonus<float>>();
            this.target = target;
        }

        public bool CanApplyTo(Equipment equipment)
        {
            if (equipment == null) return false;
            if (!target.HasValue) return true;
            return equipment.type == target.Value;
        }

        static int[] NormalizePrimaryStats(int[] stats)
        {
            int[] normalized = new int[(int)PrimaryStats.PrimaryStat.Count];
            if (stats == null)
            {
                return normalized;
            }

            Array.Copy(stats, normalized, Math.Min(stats.Length, normalized.Length));
            return normalized;
        }
    }
}
