namespace Project_1.Messaging.Events
{
    internal readonly struct InventorySlotChanged
    {
        public InventorySlotChanged(int bagIndex, int slotIndex, Items.Inventory inventory)
        {
            BagIndex = bagIndex;
            SlotIndex = slotIndex;
            Inventory = inventory;
        }

        public int BagIndex { get; }
        public int SlotIndex { get; }
        public Items.Inventory Inventory { get; }
    }

    internal readonly struct InventorySwapItemsRequested
    {
        public InventorySwapItemsRequested((int, int) from, (int, int) to)
        {
            From = from;
            To = to;
        }
        public (int, int) From { get; }
        public (int, int) To { get; }
    }

    internal readonly struct InventorySwapEquipmentRequested
    {
        public InventorySwapEquipmentRequested((int, int) from, int equipmentSlot, int? targetRenderId)
        {
            From = from;
            EquipmentSlot = equipmentSlot;
            TargetRenderId = targetRenderId;
        }
        public (int, int) From { get; }
        public int EquipmentSlot { get; }
        public int? TargetRenderId { get; }
    }

    internal readonly struct InventoryEquipBagRequested
    {
        public InventoryEquipBagRequested((int, int) from)
        {
            From = from;
        }
        public (int, int) From { get; }
    }

    internal readonly struct InventoryUnequipBagRequested
    {
        public InventoryUnequipBagRequested(int bagSlot, (int, int)? toSlot)
        {
            BagSlot = bagSlot;
            ToSlot = toSlot;
        }
        public int BagSlot { get; }
        public (int, int)? ToSlot { get; }
    }

    internal readonly struct InventorySwapBagsRequested
    {
        public InventorySwapBagsRequested((int, int) from, int bagSlot)
        {
            From = from;
            BagSlot = bagSlot;
        }
        public (int, int) From { get; }
        public int BagSlot { get; }
    }

    internal readonly struct InventorySwapBagSlotsRequested
    {
        public InventorySwapBagSlotsRequested(int fromBagSlot, int toBagSlot)
        {
            FromBagSlot = fromBagSlot;
            ToBagSlot = toBagSlot;
        }
        public int FromBagSlot { get; }
        public int ToBagSlot { get; }
    }

    internal readonly struct LootItemRequested
    {
        public LootItemRequested(int lootSlotIndex, (int, int)? toSlot)
        {
            LootSlotIndex = lootSlotIndex;
            ToSlot = toSlot;
        }
        public int LootSlotIndex { get; }
        public (int, int)? ToSlot { get; }
    }

    internal readonly struct InventoryEquipRequested
    {
        public InventoryEquipRequested((int, int) index, int? targetRenderId)
        {
            Index = index;
            TargetRenderId = targetRenderId;
        }
        public (int, int) Index { get; }
        public int? TargetRenderId { get; }
    }

    internal readonly struct InventoryConsumeRequested
    {
        public InventoryConsumeRequested((int, int) index, int? targetRenderId)
        {
            Index = index;
            TargetRenderId = targetRenderId;
        }
        public (int, int) Index { get; }
        public int? TargetRenderId { get; }
    }

    internal readonly struct EquipmentSwapRequested
    {
        public EquipmentSwapRequested(int fromSlot, int toSlot, int? targetRenderId)
        {
            FromSlot = fromSlot;
            ToSlot = toSlot;
            TargetRenderId = targetRenderId;
        }
        public int FromSlot { get; }
        public int ToSlot { get; }
        public int? TargetRenderId { get; }
    }

    internal readonly struct EquipmentMoveToInventoryRequested
    {
        public EquipmentMoveToInventoryRequested(int equipmentSlot, (int, int) inventorySlot, int? targetRenderId)
        {
            EquipmentSlot = equipmentSlot;
            InventorySlot = inventorySlot;
            TargetRenderId = targetRenderId;
        }
        public int EquipmentSlot { get; }
        public (int, int) InventorySlot { get; }
        public int? TargetRenderId { get; }
    }
}
