namespace Project_1.Messaging.Events
{
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
}
