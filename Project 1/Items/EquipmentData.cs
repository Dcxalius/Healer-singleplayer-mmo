using Newtonsoft.Json;
using Project_1.GameObjects.Unit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items
{
    internal class EquipmentData : ItemData
    {
        


        public Equipment.Type Slot { get => slot; } //TODO: Find better name
        Equipment.Type slot;

        public EquipmentData(int id, string gfxName, string name, string description, Equipment.Type slot, ItemType itemType) : base(id, gfxName, name, description, 1, itemType)
        {
            this.slot = slot;
        }

        [JsonConstructor]
        public EquipmentData(int id, string gfxName, string name, string description, Equipment.Type slot) : this(id, gfxName, name, description, slot, ItemType.Equipment) { }
    }
}
