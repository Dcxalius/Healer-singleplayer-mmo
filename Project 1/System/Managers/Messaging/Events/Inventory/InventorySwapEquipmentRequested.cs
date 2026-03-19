namespace Project_1.Messaging.Events
{
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
}
