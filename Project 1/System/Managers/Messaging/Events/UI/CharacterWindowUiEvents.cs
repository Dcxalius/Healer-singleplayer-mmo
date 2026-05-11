using Project_1.Camera;
using Project_1.Items.SubTypes;
using System;

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
        public CharacterWindowSnapshot(EntityUiSnapshot ownerSnapshot, StatReportSnapshot primaryStats, StatReportSnapshot secondaryStats, int currentLevel, int currentExperience, ItemUiSnapshot[] equippedItems, WeaponSkillUiSnapshot[] weaponSkills)
        {
            OwnerSnapshot = ownerSnapshot;
            PrimaryStats = primaryStats;
            SecondaryStats = secondaryStats;
            CurrentLevel = currentLevel;
            CurrentExperience = currentExperience;
            EquippedItems = equippedItems;
            WeaponSkills = weaponSkills ?? Array.Empty<WeaponSkillUiSnapshot>();
        }

        public EntityUiSnapshot OwnerSnapshot { get; }
        public StatReportSnapshot PrimaryStats { get; }
        public StatReportSnapshot SecondaryStats { get; }
        public int CurrentLevel { get; }
        public int CurrentExperience { get; }
        public ItemUiSnapshot[] EquippedItems { get; }
        public WeaponSkillUiSnapshot[] WeaponSkills { get; }
    }

    internal readonly struct WeaponSkillUiSnapshot
    {
        public WeaponSkillUiSnapshot(Weapon.WeaponType weaponType, string displayName, int value, int maxValue)
        {
            WeaponType = weaponType;
            DisplayName = displayName ?? weaponType.ToString();
            Value = value;
            MaxValue = Math.Max(1, maxValue);
        }

        public Weapon.WeaponType WeaponType { get; }
        public string DisplayName { get; }
        public int Value { get; }
        public int MaxValue { get; }
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
            SpellSnapshots = spellSnapshots ?? Array.Empty<SpellUiSnapshot>();
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
