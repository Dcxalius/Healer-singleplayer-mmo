using Project_1.Camera;
using Project_1.Managers;
using Project_1.Messaging.Events;
using System.Collections.Generic;

namespace Project_1.UI
{
    internal static class UiPlayerStateCache
    {
        static readonly Dictionary<string, SpellUiSnapshot> spellsByName = new Dictionary<string, SpellUiSnapshot>();
        static readonly object spellLock = new object();
        static bool valid;
        static bool inCombatOrPartyInCombat;
        static int gold;
        static bool offGlobalCooldown;
        static double globalCooldownRatio;
        static WorldSpace playerFeet;
        static bool hasTarget;
        static WorldSpace targetFeet;

        public static bool Valid
        {
            get
            {
                AssertUiOrMainThread();
                return valid;
            }
        }
        public static bool InCombatOrPartyInCombat
        {
            get
            {
                AssertUiOrMainThread();
                return inCombatOrPartyInCombat;
            }
        }
        public static int Gold
        {
            get
            {
                AssertUiOrMainThread();
                return gold;
            }
        }
        public static bool OffGlobalCooldown
        {
            get
            {
                AssertUiOrMainThread();
                return offGlobalCooldown;
            }
        }
        public static double GlobalCooldownRatio
        {
            get
            {
                AssertUiOrMainThread();
                return globalCooldownRatio;
            }
        }
        public static WorldSpace PlayerFeet
        {
            get
            {
                AssertUiOrMainThread();
                return playerFeet;
            }
        }
        public static bool HasTarget
        {
            get
            {
                AssertUiOrMainThread();
                return hasTarget;
            }
        }
        public static WorldSpace TargetFeet
        {
            get
            {
                AssertUiOrMainThread();
                return targetFeet;
            }
        }

        public static void Update(PlayerUiSnapshot snapshot)
        {
            ThreadAffinity.AssertUiThread();
            valid = snapshot.Valid;
            inCombatOrPartyInCombat = snapshot.InCombatOrPartyInCombat;
            gold = snapshot.Gold;
            offGlobalCooldown = snapshot.OffGlobalCooldown;
            globalCooldownRatio = snapshot.GlobalCooldownRatio;
            playerFeet = snapshot.PlayerFeet;
            hasTarget = snapshot.HasTarget;
            targetFeet = snapshot.TargetFeet;

            lock (spellLock)
            {
                spellsByName.Clear();
                SpellUiSnapshot[] spellSnapshots = snapshot.SpellSnapshots;
                if (spellSnapshots == null) return;

                for (int i = 0; i < spellSnapshots.Length; i++)
                {
                    SpellUiSnapshot spell = spellSnapshots[i];
                    if (string.IsNullOrWhiteSpace(spell.Name)) continue;
                    spellsByName[spell.Name] = spell;
                }
            }
        }

        public static bool TryGetSpellSnapshot(string spellName, out SpellUiSnapshot snapshot)
        {
            AssertUiOrMainThread();
            if (string.IsNullOrWhiteSpace(spellName))
            {
                snapshot = default;
                return false;
            }

            lock (spellLock)
            {
                return spellsByName.TryGetValue(spellName, out snapshot);
            }
        }

        static void AssertUiOrMainThread()
        {
            if (ThreadAffinity.IsMainThread) return;
            ThreadAffinity.AssertUiThread();
        }
    }
}
