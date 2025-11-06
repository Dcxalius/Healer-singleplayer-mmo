using Project_1.GameObjects.Unit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public Weapon.WeaponType WeaponType => weaponType;
        Weapon.WeaponType weaponType;


        public WeaponData(int id, string gfxName, string name, string description, Equipment.Type slot, int armor, int[] baseStats, int minAttackDamage, int maxAttackDamage, float attackSpeed, Item.Quality quality, Weapon.WeaponType weaponType, int cost) : base(id, gfxName, name, description, slot, ItemType.Weapon, armor, baseStats, quality, cost, Equipment.GearType.None)
        {
            attack = new Attack(minAttackDamage, maxAttackDamage, attackSpeed, weaponType);
            this.weaponType = weaponType;
            Debug.Assert(weaponType != Weapon.WeaponType.None);
        }
    }
}
