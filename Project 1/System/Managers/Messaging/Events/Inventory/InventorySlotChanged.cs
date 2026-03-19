namespace Project_1.Messaging.Events
{
    internal readonly struct InventorySlotChanged
    {
        public InventorySlotChanged(int bagIndex, int slotIndex, InventoryUiSnapshot snapshot)
        {
            BagIndex = bagIndex;
            SlotIndex = slotIndex;
            Snapshot = snapshot;
        }

        public int BagIndex { get; }
        public int SlotIndex { get; }
        public InventoryUiSnapshot Snapshot { get; }
    }
}
