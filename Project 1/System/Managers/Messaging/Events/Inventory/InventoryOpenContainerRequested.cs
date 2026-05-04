namespace Project_1.Messaging.Events
{
    internal readonly struct InventoryOpenContainerRequested
    {
        public InventoryOpenContainerRequested((int, int) index)
        {
            Index = index;
        }

        public (int, int) Index { get; }
    }
}
