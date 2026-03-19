namespace Project_1.Messaging.Events
{
    internal readonly struct LootSlotChanged
    {
        public LootSlotChanged(int slot, ItemUiSnapshot itemSnapshot, int amountRemoved)
        {
            Slot = slot;
            ItemSnapshot = itemSnapshot;
            AmountRemoved = amountRemoved;
        }

        public int Slot { get; }
        public ItemUiSnapshot ItemSnapshot { get; }
        public int AmountRemoved { get; }
    }
}
