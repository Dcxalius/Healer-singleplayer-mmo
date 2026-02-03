using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Spells;
using System.Collections.Generic;
using Project_1.GameObjects.Entities.Friendlies;
using Project_1.GameObjects.Entities.Friendlies.Npcs;
using Microsoft.Xna.Framework;
using Project_1.Textures;

namespace Project_1.Messaging.Events
{
    internal readonly struct EntityUiSnapshot
    {
        public EntityUiSnapshot(
            int renderId,
            string name,
            string className,
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
            ClassName = className;
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
        public string ClassName { get; }
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
        public EquipmentSlotChanged(int ownerRenderId, Relation.RelationToPlayer ownerRelation, Equipment.Slot slot, Items.Item itemSnapshot)
        {
            OwnerRenderId = ownerRenderId;
            OwnerRelation = ownerRelation;
            Slot = slot;
            ItemSnapshot = itemSnapshot;
        }
        public int OwnerRenderId { get; }
        public Relation.RelationToPlayer OwnerRelation { get; }
        public Equipment.Slot Slot { get; }
        public Items.Item ItemSnapshot { get; }
    }

    internal readonly struct EquipmentSlotsRefreshed
    {
        public EquipmentSlotsRefreshed(int ownerRenderId, Relation.RelationToPlayer ownerRelation, Items.Item[] itemSnapshots)
        {
            OwnerRenderId = ownerRenderId;
            OwnerRelation = ownerRelation;
            ItemSnapshots = itemSnapshots;
        }
        public int OwnerRenderId { get; }
        public Relation.RelationToPlayer OwnerRelation { get; }
        public Items.Item[] ItemSnapshots { get; }
    }

    internal readonly struct StatsRefreshed
    {
        public StatsRefreshed(int ownerRenderId, Relation.RelationToPlayer ownerRelation, PairReport primaryStats, PairReport secondaryStats)
        {
            OwnerRenderId = ownerRenderId;
            OwnerRelation = ownerRelation;
            PrimaryStats = primaryStats;
            SecondaryStats = secondaryStats;
        }
        public int OwnerRenderId { get; }
        public Relation.RelationToPlayer OwnerRelation { get; }
        public PairReport PrimaryStats { get; }
        public PairReport SecondaryStats { get; }
    }

    internal readonly struct ExperienceRefreshed
    {
        public ExperienceRefreshed(int ownerRenderId, Relation.RelationToPlayer ownerRelation, int currentLevel, int currentExperience)
        {
            OwnerRenderId = ownerRenderId;
            OwnerRelation = ownerRelation;
            CurrentLevel = currentLevel;
            CurrentExperience = currentExperience;
        }
        public int OwnerRenderId { get; }
        public Relation.RelationToPlayer OwnerRelation { get; }
        public int CurrentLevel { get; }
        public int CurrentExperience { get; }
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
        public InventoryAssigned(InventoryUiSnapshot snapshot)
        {
            Snapshot = snapshot;
        }
        public InventoryUiSnapshot Snapshot { get; }
    }

    internal readonly struct InventoryUiSnapshot
    {
        public InventoryUiSnapshot(Items.Item[] bagItems, Items.Item[][] itemsByBag)
        {
            BagItems = bagItems;
            ItemsByBag = itemsByBag;
        }

        public Items.Item[] BagItems { get; }
        public Items.Item[][] ItemsByBag { get; }
    }

    internal readonly struct SpellbookRefreshed
    {
        public SpellbookRefreshed(int ownerRenderId, string[] spellNames)
        {
            OwnerRenderId = ownerRenderId;
            SpellNames = spellNames;
        }
        public int OwnerRenderId { get; }
        public string[] SpellNames { get; }
    }

    internal readonly struct SpellbarLoaded
    {
        public SpellbarLoaded(int ownerRenderId, string[] spellNames)
        {
            OwnerRenderId = ownerRenderId;
            SpellNames = spellNames;
        }
        public int OwnerRenderId { get; }
        public string[] SpellNames { get; }
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
        public CharacterWindowSet(CharacterWindowSnapshot snapshot)
        {
            Snapshot = snapshot;
        }
        public CharacterWindowSnapshot Snapshot { get; }
    }

    internal readonly struct PlayerPlateSet
    {
        public PlayerPlateSet(EntityUiSnapshot snapshot)
        {
            Snapshot = snapshot;
        }
        public EntityUiSnapshot Snapshot { get; }
    }

    internal readonly struct GoldChanged
    {
        public GoldChanged(int gold)
        {
            Gold = gold;
        }
        public int Gold { get; }
    }

    internal readonly struct CharacterWindowSnapshot
    {
        public CharacterWindowSnapshot(EntityUiSnapshot ownerSnapshot, PairReport primaryStats, PairReport secondaryStats, int currentLevel, int currentExperience, Items.Item[] equippedItems)
        {
            OwnerSnapshot = ownerSnapshot;
            PrimaryStats = primaryStats;
            SecondaryStats = secondaryStats;
            CurrentLevel = currentLevel;
            CurrentExperience = currentExperience;
            EquippedItems = equippedItems;
        }

        public EntityUiSnapshot OwnerSnapshot { get; }
        public PairReport PrimaryStats { get; }
        public PairReport SecondaryStats { get; }
        public int CurrentLevel { get; }
        public int CurrentExperience { get; }
        public Items.Item[] EquippedItems { get; }
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
        public BuffAdded(int ownerRenderId, BuffUiSnapshot buff)
        {
            OwnerRenderId = ownerRenderId;
            Buff = buff;
        }
        public int OwnerRenderId { get; }
        public BuffUiSnapshot Buff { get; }
    }

    internal readonly struct BuffUiSnapshot
    {
        public BuffUiSnapshot(int effectId, GfxPath gfxPath, double durationRemainingMs)
        {
            EffectId = effectId;
            GfxPath = gfxPath;
            DurationRemainingMs = durationRemainingMs;
        }

        public int EffectId { get; }
        public GfxPath GfxPath { get; }
        public double DurationRemainingMs { get; }
    }

    internal readonly struct GossipOpened
    {
        public GossipOpened(GossipUiSnapshot data)
        {
            Data = data;
        }
        public GossipUiSnapshot Data { get; }
    }

    internal readonly struct GossipUiSnapshot
    {
        public GossipUiSnapshot(string[][] options, int[][] linkTree, int startIndex)
        {
            Options = CloneJagged(options);
            LinkTree = CloneJagged(linkTree);
            StartIndex = startIndex;
        }

        public string[][] Options { get; }
        public int[][] LinkTree { get; }
        public int StartIndex { get; }

        static string[][] CloneJagged(string[][] source)
        {
            if (source == null) return null;
            string[][] clone = new string[source.Length][];
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] == null) continue;
                clone[i] = (string[])source[i].Clone();
            }
            return clone;
        }

        static int[][] CloneJagged(int[][] source)
        {
            if (source == null) return null;
            int[][] clone = new int[source.Length][];
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] == null) continue;
                clone[i] = (int[])source[i].Clone();
            }
            return clone;
        }
    }

    internal readonly struct GossipClosed
    {
    }
}
