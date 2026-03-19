namespace Project_1.Messaging.Events
{
    internal readonly struct InventoryEquipBagRequested
    {
        public InventoryEquipBagRequested((int, int) from)
        {
            From = from;
        }

        public (int, int) From { get; }
    }
}
