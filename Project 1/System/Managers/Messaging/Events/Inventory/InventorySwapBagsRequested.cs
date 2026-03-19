namespace Project_1.Messaging.Events
{
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
}
