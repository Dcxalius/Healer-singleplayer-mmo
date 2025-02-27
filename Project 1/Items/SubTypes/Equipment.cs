using Newtonsoft.Json;
using Project_1.GameObjects.Unit.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [JsonIgnore]
        public EquipmentStats Stats => (itemData as EquipmentData).BaseStats;
        [JsonIgnore]
        public Type type { get => (itemData as EquipmentData).Slot; }


        public Equipment(LootData aLoot) : base(aLoot)
        {
        }

        public Equipment(EquipmentData aData) : base(aData, 1)
        {
        }
    }
}
