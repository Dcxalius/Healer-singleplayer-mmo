using Project_1.Items;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit
{
    internal class Equipment
    {
        public enum Slot
        {
            Helmet,
            Shoulders,
            Cape,
            Chest,
            Hand,
            Legs,
            Boots,
            MainHand,
            OffHand,
            Range,
            Count
        }
        public enum AttackStyle
        {
            None,
            OneHander,
            TwoHander,
            DualWielding
        }

        Item[] equipped;

        public Equipment(int?[] aItemsEquiped)
        {
            equipped = new Item[(int)Slot.Count];

            Debug.Assert(aItemsEquiped.Length == equipped.Length);

            for (int i = 0; i < aItemsEquiped.Length; i++)
            {
                if (!aItemsEquiped[i].HasValue) continue;

                equipped[i] = ItemFactory.CreateItem(ItemFactory.GetItemData(aItemsEquiped[i].Value), 1);
            }
        }

        public (AttackStyle, Attack, Attack) GetWeaponAttacks() //TODO: Change this so it changes Attackstyle when a weapon is equiped
        {
            Attack mh = null;
            
            if (equipped[(int)Slot.MainHand] != null)
            {
                Weapon mhw = (equipped[(int)Slot.MainHand] as Weapon);
                mh = mhw.attack;

                if (mhw.handRequirement == Weapon.HandRequirement.TwoHand) return (AttackStyle.TwoHander, mh, null);
            }

            Attack oh = null;

            if (equipped[(int)Slot.OffHand] == null) 
            {
                if (mh == null) return (AttackStyle.None, null, null);
                return (AttackStyle.OneHander, mh, null);
            }

            oh = (equipped[(int)Slot.OffHand] as Weapon).attack;

            if (mh == null) return (AttackStyle.OneHander, null, oh);

            return (AttackStyle.DualWielding, mh, oh);
        }
    }
}
