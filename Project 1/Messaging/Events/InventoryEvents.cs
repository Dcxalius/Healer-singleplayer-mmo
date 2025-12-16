namespace Project_1.Messaging.Events
{
    internal readonly struct InventorySlotChanged
    {
        public InventorySlotChanged(int bagIndex, int slotIndex, Items.Inventory inventory)
        {
            BagIndex = bagIndex;
            SlotIndex = slotIndex;
            Inventory = inventory;
        }

        public int BagIndex { get; }
        public int SlotIndex { get; }
        public Items.Inventory Inventory { get; }
    }
}
