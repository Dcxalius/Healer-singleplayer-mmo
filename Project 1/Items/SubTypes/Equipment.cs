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

        [JsonIgnore]
        public EquipmentStats Stats => EquipmentData.BaseStats;
        [JsonIgnore]
        public Type type { get => EquipmentData.Slot; }
        [JsonIgnore]
        public GearType Material => EquipmentData.Material;

        public Equipment(LootData aLoot) : base(aLoot)
        {
        }

        public Equipment(EquipmentData aData) : base(aData, 1)
        {
        }
    }
}
