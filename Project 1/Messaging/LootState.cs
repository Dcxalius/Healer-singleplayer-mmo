using Project_1.Items;
using Project_1.Messaging.Events;

namespace Project_1.Messaging
{
    /// <summary>
    /// Shared loot state for the currently opened loot drop. Publishes slot change/remove events on mutation.
    /// </summary>
    internal static class LootState
    {
        public static LootDrop Current { get; private set; }

        public static Item[] Open(LootDrop drop)
        {
            if (drop?.Drop != null)
            {
                Item[] cloned = new Item[drop.Drop.Length];
                for (int i = 0; i < drop.Drop.Length; i++)
                {
                    Item it = drop.Drop[i];
                    if (it == null) continue;
                    cloned[i] = new Item(it.ID, it.Count);
                }
                drop.SetDrop(cloned);
                Current = drop;
                return cloned;
            }
            Current = drop;
            return drop?.Drop;
        }

        public static LootContext BuildContext(LootDrop drop)
        {
            if (drop == null) return default;
            float allowed = drop.DropperHalfHeight + (float)ObjectManager.Player.FeetSize.Y / 2f;
            return new LootContext(drop.GetHashCode(), drop.DropperFeet, allowed, drop.Despawned);
        }

        public static Item Peek(int slot)
        {
            if (Current == null || Current.Drop == null) return null;
            if (slot < 0 || slot >= Current.Drop.Length) return null;
            Item existing = Current.Drop[slot];
            if (existing == null) return null;
            return new Item(existing.ID, existing.Count);
        }

        public static Item Take(int slot, int amount)
        {
            if (Current == null || Current.Drop == null) return null;
            if (slot < 0 || slot >= Current.Drop.Length) return null;

            Item existing = Current.Drop[slot];
            if (existing == null) return null;

            int takeAmount = amount <= 0 ? existing.Count : amount;
            if (takeAmount >= existing.Count)
            {
                Current.Drop[slot] = null;
                Mailboxes.Ui.Publish(new LootSlotRemoved(slot));
                return existing;
            }

            existing.Count -= takeAmount;
            Mailboxes.Ui.Publish(new LootSlotChanged(slot, new Item(existing.ID, existing.Count), takeAmount));
            return new Item(existing.ID, takeAmount);
        }
    }
}
