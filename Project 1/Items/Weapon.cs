using Project_1.GameObjects.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items
{
    internal class Weapon : Equipment
    {
        public enum HandRequirement
        {
            OneHand,
            TwoHand,
            MainHand,
            OffHand, 
            Ranged
        }

        public Attack attack;

        public HandRequirement handRequirement;

        public Weapon(LootData aLoot) : base(aLoot)
        {
        }

        public Weapon(WeaponData aData) : base(aData)
        {
        }
    }
}
