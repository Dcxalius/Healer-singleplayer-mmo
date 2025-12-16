using Project_1.GameObjects.Entities.Npcs;
using Project_1.UI.HUD.Windows.Gossip;

namespace Project_1.Messaging.Events
{
    internal readonly struct ShopRefundRequest
    {
        public ShopRefundRequest(int bagIndex, int slotIndex)
        {
            BagIndex = bagIndex;
            SlotIndex = slotIndex;
        }
        public int BagIndex { get; }
        public int SlotIndex { get; }
    }

    internal readonly struct ShopOpened
    {
        public ShopOpened(ShopGossipOption shop, Npc npc)
        {
            Shop = shop;
            Npc = npc;
        }
        public ShopGossipOption Shop { get; }
        public Npc Npc { get; }
    }
}
