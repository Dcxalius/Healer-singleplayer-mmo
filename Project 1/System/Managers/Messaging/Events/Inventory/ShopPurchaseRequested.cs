namespace Project_1.Messaging.Events
{
    internal readonly struct ShopPurchaseRequested
    {
        public ShopPurchaseRequested(int itemId, int count)
        {
            ItemId = itemId;
            Count = count;
        }

        public int ItemId { get; }
        public int Count { get; }
    }
}
