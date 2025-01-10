using Project_1.Items;
using Project_1.UI.HUD;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using static Project_1.GameObjects.Unit.Equipment;

namespace Project_1.GameObjects.Unit
{
    internal class Equipment
    {
        public enum Slot
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
            Trinket1,
            Trinket2,
            Finger1,
            Finger2,
            MainHand,
            OffHand,
            Ranged,
            Count
        }

        static public Slot SlotToSlot(Items.Equipment.Type aType) //TODO: FIX SHITE NAME
        {
            switch (aType)
            {
                case Items.Equipment.Type.Head:
                case Items.Equipment.Type.Neck:
                case Items.Equipment.Type.Shoulders:
                case Items.Equipment.Type.Back:
                case Items.Equipment.Type.Chest:
                case Items.Equipment.Type.Wrist:
                case Items.Equipment.Type.Hands:
                case Items.Equipment.Type.Belt:
                case Items.Equipment.Type.Legs:
                case Items.Equipment.Type.Feet:
                    return (Slot)aType;

                case Items.Equipment.Type.Trinket:
                    throw new NotImplementedException();

                case Items.Equipment.Type.Finger:
                    throw new NotImplementedException();

                case Items.Equipment.Type.TwoHander:
                    return Slot.MainHand;

                case Items.Equipment.Type.OneHander:
                    throw new NotImplementedException();

                case Items.Equipment.Type.MainHander:
                    return Slot.MainHand;

                case Items.Equipment.Type.OffHander:
                    return Slot.OffHand;
                    
                case Items.Equipment.Type.Ranged:
                    return Slot.Ranged;

                default:
                    throw new NotImplementedException();
            }
        }

        static public bool FitsInSlot(Items.Equipment.Type aType, Slot aSlot)
        {
            switch (aSlot)
            {
                case Slot.Head:
                case Slot.Neck:
                case Slot.Shoulders:
                case Slot.Back:
                case Slot.Chest:
                case Slot.Wrist:
                case Slot.Hands:
                case Slot.Belt:
                case Slot.Legs:
                case Slot.Feet:
                    return (Slot)aType == aSlot;
                    
                case Slot.Trinket1:
                case Slot.Trinket2:
                    return aType == Items.Equipment.Type.Trinket;

                case Slot.Finger1:
                case Slot.Finger2:
                    return aType == Items.Equipment.Type.Finger;

                case Slot.MainHand:
                    return aType == Items.Equipment.Type.MainHander || aType == Items.Equipment.Type.OneHander || aType == Items.Equipment.Type.TwoHander;

                case Slot.OffHand:
                    return aType == Items.Equipment.Type.OffHander || aType == Items.Equipment.Type.OneHander;

                case Slot.Ranged:
                    return aType == Items.Equipment.Type.Ranged;

                default:
                    throw new NotImplementedException();
            }
        }

        public enum AttackStyle
        {
            None,
            OneHander,
            TwoHander,
            DualWielding
        }

        Items.Equipment[] equipped;

        public Item EquipedInSlot(Slot aSlot) => equipped[(int)aSlot];

        public Item EquipInParticularSlot(Items.Equipment aEquipment, Slot aSlot)
        {
            Debug.Assert(FitsInSlot(aEquipment.type, aSlot));
            Item returnable = EquipInParticularSlotOneHanderWithATwoHanderEquiped(aEquipment, aSlot);

            if (returnable != null) return returnable;

            returnable = equipped[(int)aSlot];
            equipped[(int)aSlot] = aEquipment;
            HUDManager.RefreshCharacterWindowSlot(aSlot);

            return returnable;
        }

        Item EquipInParticularSlotOneHanderWithATwoHanderEquiped(Items.Equipment aEquipment, Slot aSlot)
        {
            if (aEquipment.type != Items.Equipment.Type.OneHander) return null;
            if (equipped[(int)Slot.MainHand] == null) return null;
            if (equipped[(int)Slot.MainHand].type != Items.Equipment.Type.TwoHander) return null;

            Item returnable = equipped[(int)Slot.MainHand];
            equipped[(int)Slot.MainHand] = null;
            equipped[(int)aSlot] = aEquipment;
            HUDManager.RefreshCharacterWindowSlot(Slot.MainHand);
            HUDManager.RefreshCharacterWindowSlot(aSlot);
            return returnable;
        }

        public Item Equip(Items.Equipment aEquipment)
        {
            Items.Equipment.Type type = aEquipment.type;
            switch (type)
            {
                case Items.Equipment.Type.Head:
                case Items.Equipment.Type.Neck:
                case Items.Equipment.Type.Shoulders:
                case Items.Equipment.Type.Back:
                case Items.Equipment.Type.Chest:
                case Items.Equipment.Type.Wrist:
                case Items.Equipment.Type.Hands:
                case Items.Equipment.Type.Belt:
                case Items.Equipment.Type.Legs:
                case Items.Equipment.Type.Feet:
                    return EquipAndSwap(aEquipment, (Slot)aEquipment.type); //TODO: Fix bad names the slot casted into is different from the slot from equipment

                case Items.Equipment.Type.Trinket:
                    return CheckDoubleSlot(aEquipment, Slot.Trinket1, Slot.Trinket2);

                case Items.Equipment.Type.Finger:
                    return CheckDoubleSlot(aEquipment, Slot.Finger1, Slot.Finger2);

                case Items.Equipment.Type.TwoHander:
                    throw new NotImplementedException();

                case Items.Equipment.Type.OneHander:
                    return EqiupAndSwapWeapon(aEquipment, Slot.MainHand);

                case Items.Equipment.Type.MainHander:
                    return EqiupAndSwapWeapon(aEquipment, Slot.MainHand);

                case Items.Equipment.Type.OffHander:
                    return EqiupAndSwapWeapon(aEquipment, Slot.OffHand);

                case Items.Equipment.Type.Ranged:
                    return EquipAndSwap(aEquipment, Slot.Ranged);

                default:
                    throw new NotImplementedException();
            }
        }

        Item EqiupAndSwapWeapon(Items.Equipment aEquipment, Slot aSlot) //TODO: Find better name
        {
            Item returnable = EquipAndSwapWeaponWithTwoHander(aEquipment, aSlot);

            if (returnable != null) return returnable;

            if (aEquipment.type == Items.Equipment.Type.OneHander)
            {
                return CheckDoubleSlot(aEquipment, aSlot, Slot.OffHand);
            }

            return EquipAndSwap(aEquipment, aSlot);
        }

        Item EquipAndSwapWeaponWithTwoHander(Items.Equipment aEquipment, Slot aSlot)
        {
            if (equipped[(int)Slot.MainHand] == null) return null;

            if ((equipped[(int)Slot.MainHand] as Items.Equipment).type != Items.Equipment.Type.TwoHander) return null;
            
            Item returnable = equipped[(int)Slot.MainHand];

            if (aSlot == Slot.OffHand)
            {
                equipped[(int)Slot.MainHand] = null;
                equipped[(int)Slot.OffHand] = aEquipment;
                HUDManager.RefreshCharacterWindowSlot(Slot.MainHand);
                HUDManager.RefreshCharacterWindowSlot(Slot.OffHand);
                return returnable;
            }

            equipped[(int)Slot.MainHand] = aEquipment;

            HUDManager.RefreshCharacterWindowSlot(Slot.MainHand);

            return returnable;
        }

        Item EquipAndSwap(Items.Equipment aEq, Slot aSlot)
        {
            Item returnItem = equipped[(int)aSlot];
            equipped[(int)aSlot] = aEq;
            HUDManager.RefreshCharacterWindowSlot(aSlot);
            return returnItem;
        }

        public (Item, Item) EquipTwoHander(Items.Equipment aEquipment)
        {
            Item mh = equipped[(int)Slot.MainHand];
            Item oh = equipped[(int)Slot.OffHand];
            equipped[(int)Slot.MainHand] = aEquipment;
            equipped[(int)Slot.OffHand] = null;

            HUDManager.RefreshCharacterWindowSlot(Slot.MainHand);
            HUDManager.RefreshCharacterWindowSlot(Slot.OffHand);

            if (mh == null) return (oh, null);
            if (oh == null) return (mh, null);
            return (mh, oh);
        }

        Item CheckDoubleSlot(Items.Equipment aEquipment, Slot aSlot, Slot aSlot2)
        {
            Item first = equipped[(int)aSlot];
            Item second = equipped[(int)aSlot2];
            if (first == null)
            {
                equipped[(int)aSlot] = aEquipment;
                HUDManager.RefreshCharacterWindowSlot(aSlot);
                return null;
            }
            if (second == null)
            {
                equipped[(int)aSlot2] = aEquipment;
                HUDManager.RefreshCharacterWindowSlot(aSlot2);
                return null;
            }
            equipped[(int)aSlot] = aEquipment;
            HUDManager.RefreshCharacterWindowSlot(aSlot);

            return first;
        }

        public Equipment(int?[] aItemsEquiped)
        {
            equipped = new Items.Equipment[(int)Slot.Count];

            Debug.Assert(aItemsEquiped.Length == equipped.Length);

            for (int i = 0; i < aItemsEquiped.Length; i++)
            {
                if (!aItemsEquiped[i].HasValue) continue;

                equipped[i] = ItemFactory.CreateItem(ItemFactory.GetItemData(aItemsEquiped[i].Value), 1) as Items.Equipment;
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
