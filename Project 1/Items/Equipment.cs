using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items
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
            Trinket,
            Finger,
            TwoHander,
            OneHander,
            MainHander,
            OffHander,
            Ranged,
            Count
        }

        public Type type { get => (itemData as EquipmentData).Slot; }


        public Equipment(LootData aLoot) : base(aLoot)
        {
        }

        public Equipment(EquipmentData aData) : base(aData, 1)
        {
        }
    }
}
