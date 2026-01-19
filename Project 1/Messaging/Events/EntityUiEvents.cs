using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Spells;
using System.Collections.Generic;

namespace Project_1.Messaging.Events
{
    internal readonly struct EquipmentSlotChanged
    {
        public EquipmentSlotChanged(Friendly friendly, Equipment.Slot slot, GameObjects.Unit.Equipment equipment)
        {
            Friendly = friendly;
            Slot = slot;
            Equipment = equipment;
        }
        public Friendly Friendly { get; }
        public Equipment.Slot Slot { get; }
        public GameObjects.Unit.Equipment Equipment { get; }
    }

    internal readonly struct EquipmentSlotsRefreshed
    {
        public EquipmentSlotsRefreshed(Friendly friendly, GameObjects.Unit.Equipment equipment)
        {
            Friendly = friendly;
            Equipment = equipment;
        }
        public Friendly Friendly { get; }
        public GameObjects.Unit.Equipment Equipment { get; }
    }

    internal readonly struct StatsRefreshed
    {
        public StatsRefreshed(Friendly friendly, PairReport stats)
        {
            Friendly = friendly;
            Stats = stats;
        }
        public Friendly Friendly { get; }
        public PairReport Stats { get; }
    }

    internal readonly struct ExperienceRefreshed
    {
        public ExperienceRefreshed(Friendly friendly)
        {
            Friendly = friendly;
        }
        public Friendly Friendly { get; }
    }

    internal readonly struct TargetChanged
    {
        public TargetChanged(Entity owner, Entity target)
        {
            Owner = owner;
            Target = target;
        }
        public Entity Owner { get; }
        public Entity Target { get; }
    }

    internal readonly struct PlateRefreshRequested
    {
        public PlateRefreshRequested(Entity entity)
        {
            Entity = entity;
        }
        public Entity Entity { get; }
    }

    internal readonly struct NamePlateAdded
    {
        public NamePlateAdded(Entity entity)
        {
            Entity = entity;
        }
        public Entity Entity { get; }
    }

    internal readonly struct NamePlateRemoved
    {
        public NamePlateRemoved(Entity entity)
        {
            Entity = entity;
        }
        public Entity Entity { get; }
    }

    internal readonly struct InventoryAssigned
    {
        public InventoryAssigned(Items.Inventory inventory)
        {
            Inventory = inventory;
        }
        public Items.Inventory Inventory { get; }
    }

    internal readonly struct SpellbookRefreshed
    {
        public SpellbookRefreshed(Friendly owner, Spell[] spells)
        {
            Owner = owner;
            Spells = spells;
        }
        public Friendly Owner { get; }
        public Spell[] Spells { get; }
    }

    internal readonly struct SpellbarLoaded
    {
        public SpellbarLoaded(Friendly owner, Spell[] spells)
        {
            Owner = owner;
            Spells = spells;
        }
        public Friendly Owner { get; }
        public Spell[] Spells { get; }
    }

    internal readonly struct SpellbarSnapshotRequested
    {
        public static readonly SpellbarSnapshotRequested Instance = new SpellbarSnapshotRequested();
    }

    internal readonly struct SpellbarSnapshotUpdated
    {
        public SpellbarSnapshotUpdated(string[] spells)
        {
            Spells = spells;
        }
        public string[] Spells { get; }
    }

    internal readonly struct CharacterWindowSet
    {
        public CharacterWindowSet(Friendly owner)
        {
            Owner = owner;
        }
        public Friendly Owner { get; }
    }

    internal readonly struct PlayerPlateSet
    {
        public PlayerPlateSet(Friendly owner)
        {
            Owner = owner;
        }
        public Friendly Owner { get; }
    }

    internal readonly struct GoldChanged
    {
        public GoldChanged(Friendly owner, int gold)
        {
            Owner = owner;
            Gold = gold;
        }
        public Friendly Owner { get; }
        public int Gold { get; }
    }

    internal readonly struct GuildInviteStatusUpdated
    {
        public GuildInviteStatusUpdated(IList<string> memberNames, IList<Project_1.UI.UIElements.Buttons.TwoStateGFXButton.State> statuses)
        {
            MemberNames = memberNames;
            Statuses = statuses;
        }
        public IList<string> MemberNames { get; }
        public IList<Project_1.UI.UIElements.Buttons.TwoStateGFXButton.State> Statuses { get; }
    }

    internal readonly struct BuffAdded
    {
        public BuffAdded(Entity owner, Project_1.GameObjects.Spells.Buff.Buff buff)
        {
            Owner = owner;
            Buff = buff;
        }
        public Entity Owner { get; }
        public Project_1.GameObjects.Spells.Buff.Buff Buff { get; }
    }

    internal readonly struct GossipOpened
    {
        public GossipOpened(Project_1.UI.HUD.Windows.Gossip.ChatGossipOption start, Project_1.GameObjects.Entities.Npcs.Npc npc)
        {
            Start = start;
            Npc = npc;
        }
        public Project_1.UI.HUD.Windows.Gossip.ChatGossipOption Start { get; }
        public Project_1.GameObjects.Entities.Npcs.Npc Npc { get; }
    }
}
