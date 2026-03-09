using Project_1.GameObjects;
using Project_1.GameObjects.Entities.Friendlies;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.Items;
using Project_1.Items.SubTypes;
using Project_1.Managers;
using Project_1.Messaging.Events;

namespace Project_1.Managers.States
{
    internal static partial class StateManager
    {
        static void HandleInventorySwapItemsRequested(InventorySwapItemsRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            player.Inventory.SwapItems(e.From, e.To);
        }

        static void HandleInventorySwapEquipmentRequested(InventorySwapEquipmentRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            Friendly target = ResolveFriendlyTarget(e.TargetRenderId, player);
            player.Inventory.SwapEquipment(e.From, e.EquipmentSlot, target);
        }

        static void HandleInventoryEquipBagRequested(InventoryEquipBagRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            player.Inventory.EquipBag(e.From);
        }

        static void HandleInventoryUnequipBagRequested(InventoryUnequipBagRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            if (e.ToSlot.HasValue)
            {
                player.Inventory.UnequipBag(e.BagSlot, e.ToSlot.Value);
                return;
            }
            player.Inventory.UnequipBag(e.BagSlot);
        }

        static void HandleInventorySwapBagsRequested(InventorySwapBagsRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            player.Inventory.SwapBags(e.From, e.BagSlot);
        }

        static void HandleInventorySwapBagSlotsRequested(InventorySwapBagSlotsRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            player.Inventory.SwapPlacesOfBags(e.FromBagSlot, e.ToBagSlot);
        }

        static void HandleLootItemRequested(LootItemRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            if (e.ToSlot.HasValue)
            {
                player.Inventory.LootItem(e.LootSlotIndex, e.ToSlot.Value);
                return;
            }
            player.Inventory.LootItem(e.LootSlotIndex);
        }

        static void HandleInventoryEquipRequested(InventoryEquipRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            Friendly target = ResolveFriendlyTarget(e.TargetRenderId, player);
            player.Inventory.Equip(e.Index, target);
        }

        static void HandleInventoryConsumeRequested(InventoryConsumeRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            Friendly target = ResolveFriendlyTarget(e.TargetRenderId, player);
            player.Inventory.ConsumeItem(e.Index, target);
        }

        static void HandleEquipmentSwapRequested(EquipmentSwapRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            Friendly target = ResolveFriendlyTarget(e.TargetRenderId, player);

            Equipment fromEquip = target.Equipment.EquipedInSlot((GameObjects.Unit.Equipment.Slot)e.FromSlot) as Equipment;
            if (fromEquip == null) return;
            if (!GameObjects.Unit.Equipment.FitsInSlot(fromEquip.type, (GameObjects.Unit.Equipment.Slot)e.ToSlot)) return;

            Equipment toEquip = target.Equipment.EquipedInSlot((GameObjects.Unit.Equipment.Slot)e.ToSlot) as Equipment;
            if (toEquip == null)
            {
                target.EquipInParticularSlot(fromEquip, (GameObjects.Unit.Equipment.Slot)e.ToSlot);
                target.EquipInParticularSlot(null, (GameObjects.Unit.Equipment.Slot)e.FromSlot);
                return;
            }

            if (fromEquip.type != toEquip.type) return;
            if (fromEquip.type >= Equipment.Type.MainHander) return;
            if (toEquip.type >= Equipment.Type.MainHander) return;

            target.EquipInParticularSlot(fromEquip, (GameObjects.Unit.Equipment.Slot)e.ToSlot);
            target.EquipInParticularSlot(toEquip, (GameObjects.Unit.Equipment.Slot)e.FromSlot);
        }

        static void HandleEquipmentMoveToInventoryRequested(EquipmentMoveToInventoryRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            Friendly target = ResolveFriendlyTarget(e.TargetRenderId, player);

            Equipment fromEquip = target.Equipment.EquipedInSlot((GameObjects.Unit.Equipment.Slot)e.EquipmentSlot) as Equipment;
            if (fromEquip == null) return;

            Item destItem = player.Inventory.GetItemInSlot(e.InventorySlot);
            if (destItem == null)
            {
                player.Inventory.AddItem(fromEquip, e.InventorySlot);
                target.EquipInParticularSlot(null, (GameObjects.Unit.Equipment.Slot)e.EquipmentSlot);
                return;
            }

            Equipment destEquip = destItem as Equipment;
            if (destEquip == null) return;
            if (!GameObjects.Unit.Equipment.FitsInSlot(destEquip.type, (GameObjects.Unit.Equipment.Slot)e.EquipmentSlot)) return;

            player.Inventory.AssignItem(fromEquip, e.InventorySlot);
            target.EquipInParticularSlot(destEquip, (GameObjects.Unit.Equipment.Slot)e.EquipmentSlot);
        }

        static Friendly ResolveFriendlyTarget(int? targetRenderId, Player fallback)
        {
            ThreadAffinity.AssertSimThread();
            if (!targetRenderId.HasValue) return fallback;
            if (ObjectManager.TryGetFriendlyByRenderId(targetRenderId.Value, out Friendly target))
            {
                return target;
            }
            return fallback;
        }
    }
}
