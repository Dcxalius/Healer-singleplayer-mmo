using Project_1.Items;

namespace Project_1.Messaging
{
    /// <summary>
    /// Shared loot state for the currently opened loot drop. Publishes slot change/remove events on mutation.
    /// </summary>
    internal static class LootState
    {
        public static LootDrop Current { get; private set; }

        public static void Set(LootDrop drop)
        {
            Current = drop;
        }

        public static Item Peek(int slot)
        {
            if (Current == null || Current.Drop == null) return null;
            if (slot < 0 || slot >= Current.Drop.Length) return null;
            return Current.Drop[slot];
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
                // Take entire stack
                Current.Drop[slot] = null;
                Mailboxes.Ui.Publish(new Events.LootSlotRemoved(slot));
                return existing;
            }

            // Take partial stack
            existing.Count -= takeAmount;
            Mailboxes.Ui.Publish(new Events.LootSlotChanged(slot, existing, takeAmount));
            return new Item(existing.ID, takeAmount);
        }
    }
}
