using Project_1.GameObjects.Entities.Friendlies;
using Project_1.Items.SubTypes;
using System.Diagnostics;

namespace Project_1.Items
{
    internal partial class Inventory
    {
        internal void Equip((int, int) aIndex, Friendly aFriendly)
        {
            AssertSimThread();
            if (!TryGetEquippableItem(aIndex, aFriendly, out Equipment equipment)) return;

            Project_1.GameObjects.Unit.Equipment wearing = aFriendly.Equipment;
            if (equipment.type == Equipment.Type.TwoHander)
            {
                if (OpenSlots() < 1 &&
                    wearing.EquipedInSlot(Project_1.GameObjects.Unit.Equipment.Slot.MainHand) != null &&
                    wearing.EquipedInSlot(Project_1.GameObjects.Unit.Equipment.Slot.OffHand) != null)
                {
                    return;
                }

                (Item, Item) equipedInSlots = aFriendly.EquipTwoHander(equipment);
                AssignItem(equipedInSlots.Item1, aIndex);
                if (equipedInSlots.Item2 == null) return;
                AddItem(equipedInSlots.Item2);
                return;
            }

            Item equipedInSlot = aFriendly.Equip(equipment);
            AssignItem(equipedInSlot, aIndex);
        }

        internal void SwapEquipment((int, int) aIndex, int aEquipmentSlot, Friendly aFriendly)
        {
            AssertSimThread();
            if (!TryGetEquippableItem(aIndex, aFriendly, out Equipment equipment)) return;

            if (equipment.type == Equipment.Type.TwoHander)
            {
                if (!(Project_1.GameObjects.Unit.Equipment.Slot.MainHand == (Project_1.GameObjects.Unit.Equipment.Slot)aEquipmentSlot ||
                      Project_1.GameObjects.Unit.Equipment.Slot.OffHand == (Project_1.GameObjects.Unit.Equipment.Slot)aEquipmentSlot))
                {
                    return;
                }

                Equip(aIndex, aFriendly);
                return;
            }

            if (equipment.type <= Equipment.Type.Feet || equipment.type >= Equipment.Type.MainHander)
            {
                if (Project_1.GameObjects.Unit.Equipment.EquipmentTypeToSlot(equipment.type) != (Project_1.GameObjects.Unit.Equipment.Slot)aEquipmentSlot) return;
                Equip(aIndex, aFriendly);
                return;
            }

            if (!Project_1.GameObjects.Unit.Equipment.FitsInSlot(equipment.type, (Project_1.GameObjects.Unit.Equipment.Slot)aEquipmentSlot)) return;
            Item equipedInSlot = aFriendly.EquipInParticularSlot(equipment, (Project_1.GameObjects.Unit.Equipment.Slot)aEquipmentSlot);
            AssignItem(equipedInSlot, aIndex);
        }

        bool TryGetEquippableItem((int, int) aIndex, Friendly aFriendly, out Equipment aEquipment)
        {
            Item item = items[aIndex.Item1][aIndex.Item2];
            if (item == null)
            {
                aEquipment = null;
                return false;
            }

            if (!(item.ItemType == ItemData.ItemType.Equipment || item.ItemType == ItemData.ItemType.Weapon))
            {
                aEquipment = null;
                return false;
            }

            aEquipment = item as Equipment;
            Debug.Assert(aEquipment != null, $"Inventory slot ({aIndex.Item1},{aIndex.Item2}) has ItemType {item.ItemType} but runtime type {item.GetType().Name}.");
            if (aEquipment == null)
            {
                return false;
            }

            if (item.ItemType != ItemData.ItemType.Weapon)
            {
                return true;
            }

            Weapon weapon = aEquipment as Weapon;
            if (weapon == null)
            {
                aEquipment = null;
                return false;
            }

            if (!aFriendly.ClassData.WeaponsAllowed.HasFlag(weapon.WeaponData.WeaponType))
            {
                aEquipment = null;
                return false;
            }

            return true;
        }
    }
}
