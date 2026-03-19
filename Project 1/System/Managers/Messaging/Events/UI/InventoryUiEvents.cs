namespace Project_1.Messaging.Events
{
    internal readonly struct InventoryAssigned
    {
        public InventoryAssigned(InventoryUiSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public InventoryUiSnapshot Snapshot { get; }
    }

    internal readonly struct InventoryUiSnapshot
    {
        public InventoryUiSnapshot(ItemUiSnapshot[] bagItems, ItemUiSnapshot[][] itemsByBag)
        {
            BagItems = bagItems;
            ItemsByBag = itemsByBag;
        }

        public ItemUiSnapshot[] BagItems { get; }
        public ItemUiSnapshot[][] ItemsByBag { get; }
    }

    internal readonly struct GoldChanged
    {
        public GoldChanged(int gold)
        {
            Gold = gold;
        }

        public int Gold { get; }
    }
}
