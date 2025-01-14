using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items.SubTypes
{
    internal class WeaponData : EquipmentData
    {
        public WeaponData(int id, string gfxName, string name, string description, Equipment.Type slot, int armor, int[] baseStats) : base(id, gfxName, name, description, slot, ItemType.Weapon, armor, baseStats)
        {
        }
    }
}
