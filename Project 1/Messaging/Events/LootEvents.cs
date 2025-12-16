using Project_1.Items;

namespace Project_1.Messaging.Events
{
    internal readonly struct LootOpened
    {
        public LootOpened(LootDrop drop, Item[] snapshot, LootContext context)
        {
            Drop = drop;
            Snapshot = snapshot;
            Context = context;
        }
        public LootDrop Drop { get; }
        public Item[] Snapshot { get; }
        public LootContext Context { get; }
    }

    internal readonly struct LootContext
    {
        public LootContext(int id, WorldSpace position, float allowedDistance, bool despawned)
        {
            Id = id;
            Position = position;
            AllowedDistance = allowedDistance;
            Despawned = despawned;
        }
        public int Id { get; }
        public WorldSpace Position { get; }
        public float AllowedDistance { get; }
        public bool Despawned { get; }
    }

    internal readonly struct LootSlotChanged
    {
        public LootSlotChanged(int slot, Item itemSnapshot, int amountRemoved)
        {
            Slot = slot;
            ItemSnapshot = itemSnapshot;
            AmountRemoved = amountRemoved;
        }
        public int Slot { get; }
        public Item ItemSnapshot { get; }
        public int AmountRemoved { get; }
    }

    internal readonly struct LootSlotRemoved
    {
        public LootSlotRemoved(int slot)
        {
            Slot = slot;
        }
        public int Slot { get; }
    }
}
