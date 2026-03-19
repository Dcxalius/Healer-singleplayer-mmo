namespace Project_1.Messaging.Events
{
    internal readonly struct LootSlotRemoved
    {
        public LootSlotRemoved(int slot)
        {
            Slot = slot;
        }

        public int Slot { get; }
    }
}
