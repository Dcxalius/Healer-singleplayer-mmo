using Project_1.GameObjects.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items
{
    internal class Weapon : Item
    {
        public enum HandRequirement
        {
            OneHand,
            TwoHand,
            MainHand,
            OffHand
        }

        public Attack attack;

        public HandRequirement handRequirement;

        public Weapon(LootData aLoot) : base(aLoot)
        {
        }

        public Weapon(ItemData aData, int aCount) : base(aData, aCount)
        {
        }
    }
}
