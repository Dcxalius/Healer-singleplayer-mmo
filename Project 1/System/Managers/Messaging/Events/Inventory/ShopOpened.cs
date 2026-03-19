namespace Project_1.Messaging.Events
{
    internal readonly struct ShopOpened
    {
        public ShopOpened(int[] itemIds, string shopkeeperName)
        {
            ItemIds = itemIds;
            ShopkeeperName = shopkeeperName;
        }

        public int[] ItemIds { get; }
        public string ShopkeeperName { get; }
    }
}
