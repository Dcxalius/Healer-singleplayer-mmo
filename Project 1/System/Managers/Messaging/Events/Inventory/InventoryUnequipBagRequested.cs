namespace Project_1.Messaging.Events
{
    internal readonly struct InventoryUnequipBagRequested
    {
        public InventoryUnequipBagRequested(int bagSlot, (int, int)? toSlot)
        {
            BagSlot = bagSlot;
            ToSlot = toSlot;
        }

        public int BagSlot { get; }
        public (int, int)? ToSlot { get; }
    }
}
