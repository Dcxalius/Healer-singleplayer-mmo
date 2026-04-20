namespace Project_1.Messaging.Events
{
    internal readonly struct InventoryEnchantTargetRequested
    {
        public InventoryEnchantTargetRequested((int, int) index)
        {
            Index = index;
        }

        public (int, int) Index { get; }
    }
}
