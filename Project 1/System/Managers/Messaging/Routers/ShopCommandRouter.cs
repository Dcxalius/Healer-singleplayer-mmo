using Project_1.GameObjects;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;

namespace Project_1.Items
{
    internal static class ShopCommandRouter
    {
        static bool initialized;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;

            Mailboxes.RegisterSimCommandType<ShopPurchaseRequested>();
            Mailboxes.Sim.Subscribe<ShopPurchaseRequested>(HandleShopPurchaseRequested);
        }

        static void HandleShopPurchaseRequested(ShopPurchaseRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;

            Item item = ItemFactory.CreateItem(e.ItemId, e.Count);
            if (item == null) return;
            if (item.Cost > player.Gold) return;

            player.ChangeGold(-item.Cost);
            player.Inventory.AddItem(item);
        }
    }
}
