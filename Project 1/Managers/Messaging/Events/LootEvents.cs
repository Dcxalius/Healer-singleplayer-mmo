using Project_1.Camera;

namespace Project_1.Messaging.Events
{
    internal readonly struct LootOpened
    {
        public LootOpened(ItemUiSnapshot[] snapshot, LootContext context)
        {
            Snapshot = snapshot;
            Context = context;
        }
        public ItemUiSnapshot[] Snapshot { get; }
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

    internal readonly struct LootClosed
    {
        public LootClosed(int contextId)
        {
            ContextId = contextId;
        }
        public int ContextId { get; }
    }

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

    internal readonly struct LootSlotRemoved
    {
        public LootSlotRemoved(int slot)
        {
            Slot = slot;
        }
        public int Slot { get; }
    }
}
