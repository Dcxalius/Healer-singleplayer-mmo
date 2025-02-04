using Project_1.GameObjects.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Project_1.Items.SubTypes
{
    internal class WeaponData : EquipmentData
    {
        public Attack Attack => attack;
        Attack attack;


        public WeaponData(int id, string gfxName, string name, string description, Equipment.Type slot, int armor, int[] baseStats, int minAttackDamage, int maxAttackDamage, float attackSpeed) : base(id, gfxName, name, description, slot, ItemType.Weapon, armor, baseStats)
        {
            attack = new Attack(minAttackDamage, maxAttackDamage, attackSpeed);
        }

    }
}
