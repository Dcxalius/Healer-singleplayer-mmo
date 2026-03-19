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
}
