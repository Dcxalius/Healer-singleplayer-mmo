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
}
