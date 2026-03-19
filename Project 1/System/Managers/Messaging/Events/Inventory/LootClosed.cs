namespace Project_1.Messaging.Events
{
    internal readonly struct LootClosed
    {
        public LootClosed(int contextId)
        {
            ContextId = contextId;
        }

        public int ContextId { get; }
    }
}
