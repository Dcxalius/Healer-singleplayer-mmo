using Newtonsoft.Json;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Items;
using Project_1.Items.SubTypes;
using Project_1.UI.HUD;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
            Finger1,
            Finger2, 
            Trinket1,
            Trinket2,
            MainHand,
            OffHand,
            Ranged,
            Count
        }

        Entity owner;

        Items.SubTypes.Equipment[] equipped;

        public int MeleeAttackPower
        {
            set
            {
                if (equipped[(int)Slot.MainHand] != null) (equipped[(int)Slot.MainHand] as Weapon).Attack.AttackPower = value;
                if (equipped[(int)Slot.OffHand] != null)
                {
                    if (equipped[(int)Slot.OffHand] is Weapon) (equipped[(int)Slot.OffHand] as Weapon).Attack.AttackPower = value;
                }
            }
        }

        public int?[] GetEquipementAsIds
        {
            get
            {
                int?[] returnable = new int?[(int)Slot.Count];
                for (int i = 0; i < equipped.Length; i++)
                {
                    if (equipped[i] == null)
                    {
                        returnable[i] = null;
                        continue;
                    }
                    returnable[i] = equipped[i].ID;
                }
                return returnable;
            }
        }

        [JsonIgnore]
        public EquipmentStats EquipmentStats => equipmentStats;
        EquipmentStats equipmentStats;

        public Equipment(int?[] aItemsEquiped)
        {
            equipped = new Items.SubTypes.Equipment[(int)Slot.Count];
            Debug.Assert(aItemsEquiped.Length == equipped.Length);

            for (int i = 0; i < aItemsEquiped.Length; i++)
            {
                if (!aItemsEquiped[i].HasValue) continue;

                equipped[i] = ItemFactory.CreateItem(ItemFactory.GetItemData(aItemsEquiped[i].Value), 1) as Items.SubTypes.Equipment;
            }

            RefreshStatsFromEquipment();
        }

        public void SetOwner(Entity aOwner) => owner = aOwner;

        void RefreshStatsFromEquipment()
        {
            int[] totalStats = new int[(int)PrimaryStats.PrimaryStat.Count];
            int armor = 0;
            for(int i = 0; i < equipped.Length; i++)
            {
                if (equipped[i] == null) continue;
                for (int j = 0; j < totalStats.Length; j++)
                {
                    totalStats[j] += equipped[i].Stats.Stats[j];
                }
                armor += equipped[i].Stats.TotalArmor;
            }
            equipmentStats = new EquipmentStats(totalStats, armor);
        }

        static public Slot SlotToSlot(Items.SubTypes.Equipment.Type aType) //TODO: FIX SHITE NAME
        {
            switch (aType)
            {
                case Items.SubTypes.Equipment.Type.Head:
                case Items.SubTypes.Equipment.Type.Neck:
                case Items.SubTypes.Equipment.Type.Shoulders:
                case Items.SubTypes.Equipment.Type.Back:
                case Items.SubTypes.Equipment.Type.Chest:
                case Items.SubTypes.Equipment.Type.Wrist:
                case Items.SubTypes.Equipment.Type.Hands:
                case Items.SubTypes.Equipment.Type.Belt:
                case Items.SubTypes.Equipment.Type.Legs:
                case Items.SubTypes.Equipment.Type.Feet:
                    return (Slot)aType;

                case Items.SubTypes.Equipment.Type.Trinket:
                    return Slot.Trinket1;

                case Items.SubTypes.Equipment.Type.Finger:
                    return Slot.Finger1;

                case Items.SubTypes.Equipment.Type.TwoHander:
                    return Slot.MainHand;

                case Items.SubTypes.Equipment.Type.OneHander:
                    return Slot.MainHand;

                case Items.SubTypes.Equipment.Type.MainHander:
                    return Slot.MainHand;

                case Items.SubTypes.Equipment.Type.OffHander:
                    return Slot.OffHand;
                    
                case Items.SubTypes.Equipment.Type.Ranged:
                    return Slot.Ranged;

                default:
                    throw new NotImplementedException();
            }
        }

        static public bool FitsInSlot(Items.SubTypes.Equipment.Type aType, Slot aSlot)
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
                    return aType == Items.SubTypes.Equipment.Type.Trinket;

                case Slot.Finger1:
                case Slot.Finger2:
                    return aType == Items.SubTypes.Equipment.Type.Finger;

                case Slot.MainHand:
                    return aType == Items.SubTypes.Equipment.Type.MainHander || aType == Items.SubTypes.Equipment.Type.OneHander || aType == Items.SubTypes.Equipment.Type.TwoHander;

                case Slot.OffHand:
                    return aType == Items.SubTypes.Equipment.Type.OffHander || aType == Items.SubTypes.Equipment.Type.OneHander;

                case Slot.Ranged:
                    return aType == Items.SubTypes.Equipment.Type.Ranged;

                default:
                    throw new NotImplementedException();
            }
        }




        public Item EquipedInSlot(Slot aSlot) => equipped[(int)aSlot];

        public Item EquipInParticularSlot(Items.SubTypes.Equipment aEquipment, Slot aSlot)
        {
            if (aEquipment == null)
            {
                RemoveItem(aSlot);
                return null;
            }
            if (!FitsInSlot(aEquipment.type, aSlot)) return aEquipment;

            Item returnable = EquipInParticularSlotOneHanderWithATwoHanderEquiped(aEquipment, aSlot);

            if (returnable != null) return returnable;

            returnable = SwapItem(aEquipment, aSlot);

            return returnable;
        }

        Item EquipInParticularSlotOneHanderWithATwoHanderEquiped(Items.SubTypes.Equipment aEquipment, Slot aSlot)
        {
            if (aEquipment.type != Items.SubTypes.Equipment.Type.OneHander) return null;
            if (equipped[(int)Slot.MainHand] == null) return null;
            if (equipped[(int)Slot.MainHand].type != Items.SubTypes.Equipment.Type.TwoHander) return null;

            Item returnable = RemoveItem(Slot.MainHand);
            EquipItem(aEquipment, aSlot);
            return returnable;
        }

        public Item Equip(Items.SubTypes.Equipment aEquipment)
        {
            Items.SubTypes.Equipment.Type type = aEquipment.type;
            switch (type)
            {
                case Items.SubTypes.Equipment.Type.Head:
                case Items.SubTypes.Equipment.Type.Neck:
                case Items.SubTypes.Equipment.Type.Shoulders:
                case Items.SubTypes.Equipment.Type.Back:
                case Items.SubTypes.Equipment.Type.Chest:
                case Items.SubTypes.Equipment.Type.Wrist:
                case Items.SubTypes.Equipment.Type.Hands:
                case Items.SubTypes.Equipment.Type.Belt:
                case Items.SubTypes.Equipment.Type.Legs:
                case Items.SubTypes.Equipment.Type.Feet:
                    return SwapItem(aEquipment, (Slot)aEquipment.type);

                case Items.SubTypes.Equipment.Type.Trinket:
                    return CheckDoubleSlot(aEquipment, Slot.Trinket1, Slot.Trinket2);

                case Items.SubTypes.Equipment.Type.Finger:
                    return CheckDoubleSlot(aEquipment, Slot.Finger1, Slot.Finger2);

                case Items.SubTypes.Equipment.Type.TwoHander:
                    throw new NotImplementedException();

                case Items.SubTypes.Equipment.Type.OneHander:
                    return EqiupAndSwapWeapon(aEquipment, Slot.MainHand);

                case Items.SubTypes.Equipment.Type.MainHander:
                    return EqiupAndSwapWeapon(aEquipment, Slot.MainHand);

                case Items.SubTypes.Equipment.Type.OffHander:
                    return EqiupAndSwapWeapon(aEquipment, Slot.OffHand);

                case Items.SubTypes.Equipment.Type.Ranged:
                    return SwapItem(aEquipment, Slot.Ranged);

                default:
                    throw new NotImplementedException();
            }
        }

        Item EqiupAndSwapWeapon(Items.SubTypes.Equipment aEquipment, Slot aSlot) //TODO: Find better name
        {
            Item returnable = EquipAndSwapWeaponWithTwoHander(aEquipment, aSlot);

            if (returnable != null) return returnable;

            if (aEquipment.type == Items.SubTypes.Equipment.Type.OneHander)
            {
                return CheckDoubleSlot(aEquipment, aSlot, Slot.OffHand);
            }

            return SwapItem(aEquipment, aSlot);
        }

        Item EquipAndSwapWeaponWithTwoHander(Items.SubTypes.Equipment aEquipment, Slot aSlot)
        {
            if (equipped[(int)Slot.MainHand] == null) return null;
            if (equipped[(int)Slot.MainHand].type != Items.SubTypes.Equipment.Type.TwoHander) return null;
            
            Item returnable = RemoveItem(Slot.MainHand);

            EquipItem(aEquipment, aSlot);

            return returnable;
        }


        public (Item, Item) EquipTwoHander(Items.SubTypes.Equipment aEquipment)
        {
            Item oh = RemoveItem(Slot.OffHand);
            Item mh = SwapItem(aEquipment, Slot.MainHand);

            if (mh == null) return (oh, null);
            if (oh == null) return (mh, null);
            return (mh, oh);
        }

        Item CheckDoubleSlot(Items.SubTypes.Equipment aEquipment, Slot aSlot, Slot aSlot2)
        {
            Item first = equipped[(int)aSlot];
            Item second = equipped[(int)aSlot2];
            if (first == null)
            {
                EquipItem(aEquipment, aSlot);
                return null;
            }
            if (second == null)
            {
                EquipItem(aEquipment, aSlot2);
                return null;
            }
            Item returnable = SwapItem(aEquipment, aSlot);

            return returnable;
        }

        Item SwapItem(Items.SubTypes.Equipment aEquipment, Slot aSlot)
        {
            Items.SubTypes.Equipment previouslyEquiped = equipped[(int)aSlot];
            if (previouslyEquiped != null) equipmentStats.RemoveStats(previouslyEquiped.Stats);

            equipped[(int)aSlot] = aEquipment;
            equipmentStats.AddStats(aEquipment.Stats);
            
            HUDManager.RefreshCharacterWindowSlot(aSlot, this, owner as Friendly); //TODO: Change this to a system that tracks equipment changed during a frame and then at end sends the refresh command?
            return previouslyEquiped;
        }

        void EquipItem(Items.SubTypes.Equipment aEquipment, Slot aSlot)
        {
            Debug.Assert(equipped[(int)aSlot] == null);
            equipped[(int)aSlot] = aEquipment;
            equipmentStats.AddStats(aEquipment.Stats);
            HUDManager.RefreshCharacterWindowSlot(aSlot, this, owner as Friendly); //TODO: Change this to a system that tracks equipment changed during a frame and then at end sends the refresh command?
        }

        Item RemoveItem(Slot aSlot)
        {
            Items.SubTypes.Equipment item = equipped[(int)aSlot];
            if (item == null) return null;
            equipped[(int)aSlot] = null;
            equipmentStats.RemoveStats(item.Stats);
            HUDManager.RefreshCharacterWindowSlot(aSlot, this, owner as Friendly);
            return item;
        }

        public AttackData GetWeaponAttacks() //TODO: Change this so it changes Attackstyle when a weapon is equiped
        {
            Attack mh = null;
            
            if (equipped[(int)Slot.MainHand] != null)
            {
                Weapon mhw = (equipped[(int)Slot.MainHand] as Weapon);
                mh = mhw.Attack;

                if (mhw.handRequirement == Weapon.HandRequirement.TwoHand) return new AttackData(AttackData.AttackStyle.TwoHander, mh, null);
            }

            if (equipped[(int)Slot.OffHand] == null)
            {
                if (mh == null) return new AttackData(AttackData.AttackStyle.None, null, null);
                return new AttackData(AttackData.AttackStyle.OneHander, mh, null);
            }

            Attack oh = (equipped[(int)Slot.OffHand] as Weapon).Attack;
            
            if (oh.dps == 0)
            {
                if (mh == null) return new AttackData(AttackData.AttackStyle.None, null, null);
                return new AttackData(AttackData.AttackStyle.OneHander, mh, null);
            }

            if (mh == null) return new AttackData(AttackData.AttackStyle.OneHander, null, oh);

            return new AttackData(AttackData.AttackStyle.DualWielding, mh, oh);
        }
    }
}
