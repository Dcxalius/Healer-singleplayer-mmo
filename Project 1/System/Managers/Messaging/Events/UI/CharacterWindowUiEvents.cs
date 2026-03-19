using Project_1.Camera;

namespace Project_1.Messaging.Events
{
    internal readonly struct CharacterWindowSet
    {
        public CharacterWindowSet(CharacterWindowSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public CharacterWindowSnapshot Snapshot { get; }
    }

    internal readonly struct CharacterWindowSnapshot
    {
        public CharacterWindowSnapshot(EntityUiSnapshot ownerSnapshot, StatReportSnapshot primaryStats, StatReportSnapshot secondaryStats, int currentLevel, int currentExperience, ItemUiSnapshot[] equippedItems)
        {
            OwnerSnapshot = ownerSnapshot;
            PrimaryStats = primaryStats;
            SecondaryStats = secondaryStats;
            CurrentLevel = currentLevel;
            CurrentExperience = currentExperience;
            EquippedItems = equippedItems;
        }

        public EntityUiSnapshot OwnerSnapshot { get; }
        public StatReportSnapshot PrimaryStats { get; }
        public StatReportSnapshot SecondaryStats { get; }
        public int CurrentLevel { get; }
        public int CurrentExperience { get; }
        public ItemUiSnapshot[] EquippedItems { get; }
    }

    internal readonly struct PlayerUiSnapshot
    {
        public PlayerUiSnapshot(bool valid, bool inCombatOrPartyInCombat, int gold, bool offGlobalCooldown, double globalCooldownRatio, WorldSpace playerFeet, bool hasTarget, WorldSpace targetFeet, SpellUiSnapshot[] spellSnapshots)
        {
            Valid = valid;
            InCombatOrPartyInCombat = inCombatOrPartyInCombat;
            Gold = gold;
            OffGlobalCooldown = offGlobalCooldown;
            GlobalCooldownRatio = globalCooldownRatio;
            PlayerFeet = playerFeet;
            HasTarget = hasTarget;
            TargetFeet = targetFeet;
            SpellSnapshots = spellSnapshots ?? System.Array.Empty<SpellUiSnapshot>();
        }

        public bool Valid { get; }
        public bool InCombatOrPartyInCombat { get; }
        public int Gold { get; }
        public bool OffGlobalCooldown { get; }
        public double GlobalCooldownRatio { get; }
        public WorldSpace PlayerFeet { get; }
        public bool HasTarget { get; }
        public WorldSpace TargetFeet { get; }
        public SpellUiSnapshot[] SpellSnapshots { get; }
    }
}
