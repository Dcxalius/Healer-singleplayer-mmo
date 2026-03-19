namespace Project_1.Messaging.Events
{
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
}
