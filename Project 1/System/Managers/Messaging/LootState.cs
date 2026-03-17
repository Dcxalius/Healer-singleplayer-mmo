using Project_1.GameObjects;
using Project_1.Items;
using Project_1.Items.SubTypes;
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
                Mailboxes.PublishUiEvent(new LootClosed(contextId));
            }
        }

        public static ItemUiSnapshot[] Open(LootDrop drop)
        {
            ThreadAffinity.AssertSimThread();
            if (drop?.Drop != null)
            {
                Item[] cloned = new Item[drop.Drop.Length];
                ItemUiSnapshot[] snapshots = new ItemUiSnapshot[drop.Drop.Length];
                for (int i = 0; i < drop.Drop.Length; i++)
                {
                    Item it = drop.Drop[i];
                    if (it == null) continue;
                    cloned[i] = CloneItem(it);
                    snapshots[i] = ItemUiSnapshot.FromItem(it);
                }
                drop.SetDrop(cloned);
                Current = drop;
                CurrentContextId = drop.Id;
                closedFromEmpty = false;
                return snapshots;
            }
            Current = drop;
            CurrentContextId = drop?.Id ?? 0;
            closedFromEmpty = false;
            return null;
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
            return CloneItem(existing);
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
                Mailboxes.PublishUiEvent(new LootSlotRemoved(slot));
                return existing;
            }

            existing.Count -= takeAmount;
            Mailboxes.PublishUiEvent(new LootSlotChanged(slot, ItemUiSnapshot.FromItem(existing), takeAmount));
            if (Current.Drop.Where(x => x != null).Count() == 0 && !closedFromEmpty)
            {
                closedFromEmpty = true;
                Mailboxes.PublishUiEvent(new LootClosed(CurrentContextId));
                Current = null;
                CurrentContextId = 0;
            }
            return CloneItemWithCount(existing, takeAmount);
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

        static Item CloneItem(Item source)
        {
            if (source == null) return null;
            if (source is Weapon weapon)
            {
                return new Weapon(weapon.WeaponData, weapon.Hash);
            }

            if (source is Equipment equipment)
            {
                return new Equipment(equipment.EquipmentData, equipment.Hash);
            }

            if (source is Container container)
            {
                return new Container(ItemFactory.GetItemData<ContainerData>(container.ID));
            }

            return ItemFactory.CreateItem(source.ID, source.Count);
        }

        static Item CloneItemWithCount(Item source, int count)
        {
            if (source == null) return null;
            if (source is Weapon || source is Equipment || source is Container)
            {
                return CloneItem(source);
            }

            return ItemFactory.CreateItem(source.ID, count);
        }
    }
}
