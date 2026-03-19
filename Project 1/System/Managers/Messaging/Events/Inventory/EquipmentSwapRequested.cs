namespace Project_1.Messaging.Events
{
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
}
