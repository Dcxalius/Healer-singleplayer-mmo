namespace Project_1.Messaging.Events
{
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
}
