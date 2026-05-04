using Project_1.GameObjects;
using Project_1.GameObjects.Entities.Friendlies;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.Items.SubTypes;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.World.Items.Enchantments;
using Project_1.World.Items.SubTypes;
using System;

namespace Project_1.Items
{
    internal static class InventoryCommandRouter
    {
        static bool initialized;
        static PendingEnchantScroll? pendingEnchantScroll;

        readonly struct PendingEnchantScroll
        {
            public PendingEnchantScroll((int, int) sourceIndex, int enchantmentId, string scrollName)
            {
                SourceIndex = sourceIndex;
                EnchantmentId = enchantmentId;
                ScrollName = scrollName;
            }

            public (int, int) SourceIndex { get; }
            public int EnchantmentId { get; }
            public string ScrollName { get; }
        }

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;

            SubscribeSimCommand<InventorySwapItemsRequested>(HandleInventorySwapItemsRequested);
            SubscribeSimCommand<InventorySwapEquipmentRequested>(HandleInventorySwapEquipmentRequested);
            SubscribeSimCommand<InventoryEquipBagRequested>(HandleInventoryEquipBagRequested);
            SubscribeSimCommand<InventoryUnequipBagRequested>(HandleInventoryUnequipBagRequested);
            SubscribeSimCommand<InventorySwapBagsRequested>(HandleInventorySwapBagsRequested);
            SubscribeSimCommand<InventorySwapBagSlotsRequested>(HandleInventorySwapBagSlotsRequested);
            SubscribeSimCommand<LootItemRequested>(HandleLootItemRequested);
            SubscribeSimCommand<InventoryOpenContainerRequested>(HandleInventoryOpenContainerRequested);
            SubscribeSimCommand<InventoryEquipRequested>(HandleInventoryEquipRequested);
            SubscribeSimCommand<InventoryConsumeRequested>(HandleInventoryConsumeRequested);
            SubscribeSimCommand<InventoryEnchantTargetRequested>(HandleInventoryEnchantTargetRequested);
            SubscribeSimCommand<EquipmentEnchantRequested>(HandleEquipmentEnchantRequested);
            SubscribeSimCommand<EquipmentSwapRequested>(HandleEquipmentSwapRequested);
            SubscribeSimCommand<EquipmentMoveToInventoryRequested>(HandleEquipmentMoveToInventoryRequested);
        }

        static void SubscribeSimCommand<T>(Action<T> handler)
        {
            MailboxManager.RegisterSimCommandType<T>();
            MailboxManager.Sim.Subscribe(handler);
        }

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

        static void HandleInventoryOpenContainerRequested(InventoryOpenContainerRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;

            Item item = player.Inventory.GetItemInSlot(e.Index);
            if (item is not Container container) return;

            Item[] loot = container.GetLoot();
            if (loot == null || loot.Length == 0) return;

            LootDrop drop = new LootDrop(loot, player);
            ItemUiSnapshot[] snapshot = LootState.Open(drop);
            LootContext context = LootState.BuildContext(drop);
            player.Inventory.ConsumeOneFromSlot(e.Index);
            MailboxManager.PublishUiEvent(new LootOpened(snapshot, context));
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

            Item item = player.Inventory.GetItemInSlot(e.Index);
            if (item is Consumable consumable && consumable.RequiresItemTarget)
            {
                BeginEnchantTargeting(e.Index, consumable);
                return;
            }

            Friendly target = ResolveFriendlyTarget(e.TargetRenderId, player);
            player.Inventory.ConsumeItem(e.Index, target);
        }

        static void HandleInventoryEnchantTargetRequested(InventoryEnchantTargetRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            if (!pendingEnchantScroll.HasValue) return;

            Item item = player.Inventory.GetItemInSlot(e.Index);
            if (item is not Project_1.Items.SubTypes.Equipment equipment)
            {
                PublishEnchantSystemMessage("That item cannot be enchanted.");
                return;
            }

            TryApplyPendingEnchantment(player, equipment, onApplied: () =>
            {
                if (!equipment.ApplyPermanentEnchantment(EnchantmentFactory.GetData(pendingEnchantScroll.Value.EnchantmentId)))
                {
                    return false;
                }

                player.Inventory.RefreshSlot(e.Index);
                return true;
            });
        }

        static void HandleEquipmentEnchantRequested(EquipmentEnchantRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            if (!pendingEnchantScroll.HasValue) return;

            Item item = player.Equipment.EquipedInSlot((GameObjects.Unit.Equipment.Slot)e.EquipmentSlot);
            if (item is not Project_1.Items.SubTypes.Equipment equipment)
            {
                PublishEnchantSystemMessage("That item cannot be enchanted.");
                return;
            }

            TryApplyPendingEnchantment(player, equipment, onApplied: () =>
            {
                return player.ApplyPermanentEnchantment((GameObjects.Unit.Equipment.Slot)e.EquipmentSlot, EnchantmentFactory.GetData(pendingEnchantScroll.Value.EnchantmentId));
            });
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

        static void BeginEnchantTargeting((int, int) sourceIndex, Consumable consumable)
        {
            ThreadAffinity.AssertSimThread();
            if (!consumable.TryGetEnchantmentData(out EnchantmentData enchantmentData))
            {
                PublishEnchantSystemMessage("That scroll is not configured correctly.");
                ClearPendingEnchantTargeting();
                return;
            }

            pendingEnchantScroll = new PendingEnchantScroll(sourceIndex, enchantmentData.Id, consumable.Name);
            MailboxManager.PublishUiEvent(new InventoryEnchantTargetingChanged(true));
            PublishEnchantSystemMessage($"Select an item to enchant with {consumable.Name}.");
        }

        static bool TryApplyPendingEnchantment(Player player, Project_1.Items.SubTypes.Equipment equipment, Func<bool> onApplied)
        {
            ThreadAffinity.AssertSimThread();
            if (!TryGetPendingEnchant(player, out PendingEnchantScroll pending, out EnchantmentData enchantmentData))
            {
                return false;
            }

            if (!equipment.CanApplyPermanentEnchantment(enchantmentData))
            {
                PublishEnchantSystemMessage($"That item cannot be enchanted with {pending.ScrollName}.");
                return false;
            }

            if (onApplied == null)
            {
                return false;
            }

            if (!onApplied())
            {
                return false;
            }

            player.Inventory.ConsumeOneFromSlot(pending.SourceIndex);
            PublishEnchantSystemMessage($"Applied {enchantmentData.Name} to {equipment.Name}.");
            ClearPendingEnchantTargeting();
            return true;
        }

        static bool TryGetPendingEnchant(Player player, out PendingEnchantScroll pending, out EnchantmentData enchantmentData)
        {
            ThreadAffinity.AssertSimThread();
            enchantmentData = null;
            if (!pendingEnchantScroll.HasValue)
            {
                pending = default;
                return false;
            }

            pending = pendingEnchantScroll.Value;
            Item item = player.Inventory.GetItemInSlot(pending.SourceIndex);
            if (item is not Consumable consumable || !consumable.RequiresItemTarget || consumable.EnchantmentId != pending.EnchantmentId)
            {
                PublishEnchantSystemMessage("That enchantment scroll is no longer available.");
                ClearPendingEnchantTargeting();
                return false;
            }

            enchantmentData = EnchantmentFactory.GetData(pending.EnchantmentId);
            return enchantmentData != null;
        }

        static void ClearPendingEnchantTargeting()
        {
            ThreadAffinity.AssertSimThread();
            pendingEnchantScroll = null;
            MailboxManager.PublishUiEvent(new InventoryEnchantTargetingChanged(false));
        }

        static void PublishEnchantSystemMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            MailboxManager.PublishUiEvent(new ChatMessagePosted(ChatMessageType.System, message));
        }
    }
}
