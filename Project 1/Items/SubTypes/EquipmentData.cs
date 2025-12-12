using Newtonsoft.Json;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Stats;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.Items.SubTypes.Equipment;

namespace Project_1.Items.SubTypes
{
    internal class EquipmentData : ItemData
    {
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

        public Equipment.Type Slot { get => slot; } //TODO: Find better name
        Equipment.Type slot;

        public EquipmentData(int id, string gfxName, string name, string description, Equipment.Type slot, ItemType itemType, int armor, int[] baseStats, Item.Quality quality, int cost, GearType material, SecondayStatBonus<int>[] secondaryStatsInt, SecondayStatBonus<float>[] secondaryStatsFloat) : base(id, gfxName, name, description, 1, itemType, quality, cost)
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
        public EquipmentData(int id, string gfxName, string name, string description, Equipment.Type slot, int armor, int[] baseStats, Item.Quality quality, int cost, GearType material, SecondayStatBonus<int>[] secondaryStatsInt, SecondayStatBonus<float>[] secondayStatFloat) : this(id, gfxName, name, description, slot, ItemType.Equipment, armor, baseStats, quality, cost, material, secondaryStatsInt, secondayStatFloat) { }
    }
}
