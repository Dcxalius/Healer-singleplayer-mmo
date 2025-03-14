using Newtonsoft.Json;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Stats;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items.SubTypes
{
    internal class EquipmentData : ItemData
    {
        PairReport StatReport
        {
            get
            {
                PairReport report = new PairReport();

                if (baseStats.TotalArmor != 0) report.AddLine("Armor", baseStats.TotalArmor);
                baseStats.AppendToExistingReport(ref report);

                return report;
            }
        }

        public EquipmentStats BaseStats => baseStats;
        EquipmentStats baseStats;

        public Equipment.Type Slot { get => slot; } //TODO: Find better name
        Equipment.Type slot;

        public EquipmentData(int id, string gfxName, string name, string description, Equipment.Type slot, ItemType itemType, int armor, int[] baseStats, Item.Quality quality) : base(id, gfxName, name, description, 1, itemType, quality)
        {
            this.slot = slot;
            //this.baseStats = new EquipmentStats(baseStats);
            //DEBUG
            this.baseStats = new EquipmentStats(baseStats, armor);

        }

        [JsonConstructor]
        public EquipmentData(int id, string gfxName, string name, string description, Equipment.Type slot, int armor, int[] baseStats, Item.Quality quality) : this(id, gfxName, name, description, slot, ItemType.Equipment, armor, baseStats, quality) { }
    }
}
