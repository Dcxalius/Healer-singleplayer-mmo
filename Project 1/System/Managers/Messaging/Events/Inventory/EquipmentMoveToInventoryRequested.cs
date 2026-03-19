namespace Project_1.Messaging.Events
{
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
