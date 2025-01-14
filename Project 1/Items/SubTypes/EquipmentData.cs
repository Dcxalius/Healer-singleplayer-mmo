using Newtonsoft.Json;
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
        Report StatReport
        {
            get
            {
                Report report = new Report();

                if (armor != 0) report.AddLine(armor + " Armor");
                baseStats.StatReport(ref report);

                return report;
            }
        }

        public BasePrimaryStats BaseStats => baseStats; 
        BasePrimaryStats baseStats;

        public int Armor => armor;
        int armor;

        public Equipment.Type Slot { get => slot; } //TODO: Find better name
        Equipment.Type slot;

        public EquipmentData(int id, string gfxName, string name, string description, Equipment.Type slot, ItemType itemType, int armor, int[] baseStats) : base(id, gfxName, name, description, 1, itemType)
        {
            this.slot = slot;
            this.armor = armor;
            //this.baseStats = new BasePrimaryStats(baseStats);
        }

        [JsonConstructor]
        public EquipmentData(int id, string gfxName, string name, string description, Equipment.Type slot, int armor, int[] baseStats) : this(id, gfxName, name, description, slot, ItemType.Equipment, armor, baseStats) { }
    }
}
