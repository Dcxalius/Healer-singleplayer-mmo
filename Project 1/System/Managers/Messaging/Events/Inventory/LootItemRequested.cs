namespace Project_1.Messaging.Events
{
    internal readonly struct LootItemRequested
    {
        public LootItemRequested(int lootSlotIndex, (int, int)? toSlot)
        {
            LootSlotIndex = lootSlotIndex;
            ToSlot = toSlot;
        }

        public int LootSlotIndex { get; }
        public (int, int)? ToSlot { get; }
    }
}
