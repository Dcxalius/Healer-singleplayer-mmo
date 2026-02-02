using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Spells;
using System.Collections.Generic;
using Project_1.GameObjects.Entities.Friendlies;
using Project_1.GameObjects.Entities.Friendlies.Npcs;
using Microsoft.Xna.Framework;

namespace Project_1.Messaging.Events
{
    internal readonly struct EntityUiSnapshot
    {
        public EntityUiSnapshot(
            int renderId,
            string name,
            Relation.RelationToPlayer relationToPlayer,
            Color relationColor,
            int level,
            double currentHealth,
            double maxHealth,
            float currentResource,
            float maxResource,
            Color resourceColor,
            WorldSpace feetPosition,
            int worldHeight)
        {
            RenderId = renderId;
            Name = name;
            RelationToPlayer = relationToPlayer;
            RelationColor = relationColor;
            Level = level;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            CurrentResource = currentResource;
            MaxResource = maxResource;
            ResourceColor = resourceColor;
            FeetPosition = feetPosition;
            WorldHeight = worldHeight;
        }

        public int RenderId { get; }
        public string Name { get; }
        public Relation.RelationToPlayer RelationToPlayer { get; }
        public Color RelationColor { get; }
        public int Level { get; }
        public double CurrentHealth { get; }
        public double MaxHealth { get; }
        public float CurrentResource { get; }
        public float MaxResource { get; }
        public Color ResourceColor { get; }
        public WorldSpace FeetPosition { get; }
        public int WorldHeight { get; }
    }

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
        public TargetChanged(Relation.RelationToPlayer ownerRelation, EntityUiSnapshot? targetSnapshot)
        {
            OwnerRelation = ownerRelation;
            TargetSnapshot = targetSnapshot;
        }

        public Relation.RelationToPlayer OwnerRelation { get; }
        public EntityUiSnapshot? TargetSnapshot { get; }
    }

    internal readonly struct TargetRequested
    {
        public TargetRequested(int? targetRenderId)
        {
            TargetRenderId = targetRenderId;
        }

        public int? TargetRenderId { get; }
    }

    internal readonly struct PlateRefreshRequested
    {
        public PlateRefreshRequested(EntityUiSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public EntityUiSnapshot Snapshot { get; }
    }

    internal readonly struct NamePlateAdded
    {
        public NamePlateAdded(EntityUiSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public EntityUiSnapshot Snapshot { get; }
    }

    internal readonly struct NamePlateRemoved
    {
        public NamePlateRemoved(int renderId)
        {
            RenderId = renderId;
        }

        public int RenderId { get; }
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

    internal readonly struct PlayerUiSnapshot
    {
        public PlayerUiSnapshot(bool valid, bool inCombatOrPartyInCombat, int gold, bool offGlobalCooldown, double globalCooldownRatio, WorldSpace playerFeet, bool hasTarget, WorldSpace targetFeet)
        {
            Valid = valid;
            InCombatOrPartyInCombat = inCombatOrPartyInCombat;
            Gold = gold;
            OffGlobalCooldown = offGlobalCooldown;
            GlobalCooldownRatio = globalCooldownRatio;
            PlayerFeet = playerFeet;
            HasTarget = hasTarget;
            TargetFeet = targetFeet;
        }

        public bool Valid { get; }
        public bool InCombatOrPartyInCombat { get; }
        public int Gold { get; }
        public bool OffGlobalCooldown { get; }
        public double GlobalCooldownRatio { get; }
        public WorldSpace PlayerFeet { get; }
        public bool HasTarget { get; }
        public WorldSpace TargetFeet { get; }
    }

    internal enum InviteStatus
    {
        Pending,
        Accepted
    }

    internal readonly struct GuildInviteStatusUpdated
    {
        public GuildInviteStatusUpdated(IList<string> memberNames, IList<InviteStatus> statuses)
        {
            MemberNames = memberNames;
            Statuses = statuses;
        }
        public IList<string> MemberNames { get; }
        public IList<InviteStatus> Statuses { get; }
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
        public GossipOpened(GameObjects.Entities.Friendlies.Npcs.GossipData data)
        {
            Data = data;
        }
        public GameObjects.Entities.Friendlies.Npcs.GossipData Data { get; }
    }

    internal readonly struct GossipClosed
    {
    }
}
