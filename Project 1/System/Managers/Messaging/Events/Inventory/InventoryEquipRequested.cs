namespace Project_1.Messaging.Events
{
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
}
