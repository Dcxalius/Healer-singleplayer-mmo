using Project_1.GameObjects;
using Project_1.Items;
using Project_1.Managers;
using Project_1.Messaging.Events;
using System.Linq;
using Project_1.GameObjects.Entities.Friendlies.Players;

namespace Project_1.Messaging
{
    /// <summary>
    /// Shared loot state for the currently opened loot drop. Publishes slot change/remove events on mutation.
    /// </summary>
    internal static class LootState
    {
        public static LootDrop Current { get; private set; }
        public static int CurrentContextId { get; private set; }
        static bool closedFromEmpty;

        public static void Update()
        {
            ThreadAffinity.AssertSimThread();
            if (Current == null) return;
            if (ObjectManager.Player == null) return;
            if (Current.Despawned || Current.IsEmpty || !Current.InDistance)
            {
                int contextId = CurrentContextId;
                Current = null;
                CurrentContextId = 0;
                closedFromEmpty = true;
                Mailboxes.Ui.Publish(new LootClosed(contextId));
            }
        }

        public static Item[] Open(LootDrop drop)
        {
            ThreadAffinity.AssertSimThread();
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
                CurrentContextId = drop.Id;
                closedFromEmpty = false;
                return cloned;
            }
            Current = drop;
            CurrentContextId = drop?.Id ?? 0;
            closedFromEmpty = false;
            return drop?.Drop;
        }

        public static LootContext BuildContext(LootDrop drop)
        {
            ThreadAffinity.AssertSimThread();
            if (drop == null) return default;
            float allowed = drop.DropperHalfHeight + (float)ObjectManager.Player.FeetSize.Y / 2f;
            return new LootContext(drop.Id, drop.DropperFeet, allowed, drop.Despawned);
        }

        public static Item Peek(int slot)
        {
            ThreadAffinity.AssertSimThread();
            if (Current == null || Current.Drop == null) return null;
            if (slot < 0 || slot >= Current.Drop.Length) return null;
            Item existing = Current.Drop[slot];
            if (existing == null) return null;
            return new Item(existing.ID, existing.Count);
        }

        public static Item Take(int slot, int amount)
        {
            ThreadAffinity.AssertSimThread();
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
            if (Current.Drop.Where(x => x != null).Count() == 0 && !closedFromEmpty)
            {
                closedFromEmpty = true;
                Mailboxes.Ui.Publish(new LootClosed(CurrentContextId));
                Current = null;
                CurrentContextId = 0;
            }
            return new Item(existing.ID, takeAmount);
        }

        public static void Close(int contextId)
        {
            ThreadAffinity.AssertSimThread();
            if (Current != null && CurrentContextId == contextId)
            {
                Current = null;
                CurrentContextId = 0;
                closedFromEmpty = true;
            }
        }
    }
}
