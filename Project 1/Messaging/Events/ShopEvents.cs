namespace Project_1.Messaging.Events
{
    internal readonly struct ShopRefundRequest
    {
        public ShopRefundRequest(int bagIndex, int slotIndex)
        {
            BagIndex = bagIndex;
            SlotIndex = slotIndex;
        }
        public int BagIndex { get; }
        public int SlotIndex { get; }
    }

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

    internal readonly struct ShopOpened
    {
        public ShopOpened(int[] itemIds)
        {
            ItemIds = itemIds;
        }
        public int[] ItemIds { get; }
    }

    internal readonly struct ShopClosed
    {
    }
}
