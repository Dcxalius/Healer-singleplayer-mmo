using Project_1.Camera;
using Project_1.Managers;
using Project_1.Messaging.Events;

namespace Project_1.UI
{
    internal static class UiPlayerStateCache
    {
        public static bool Valid { get; private set; }
        public static bool InCombatOrPartyInCombat { get; private set; }
        public static int Gold { get; private set; }
        public static bool OffGlobalCooldown { get; private set; }
        public static double GlobalCooldownRatio { get; private set; }
        public static WorldSpace PlayerFeet { get; private set; }
        public static bool HasTarget { get; private set; }
        public static WorldSpace TargetFeet { get; private set; }

        public static void Update(PlayerUiSnapshot snapshot)
        {
            ThreadAffinity.AssertUiThread();
            Valid = snapshot.Valid;
            InCombatOrPartyInCombat = snapshot.InCombatOrPartyInCombat;
            Gold = snapshot.Gold;
            OffGlobalCooldown = snapshot.OffGlobalCooldown;
            GlobalCooldownRatio = snapshot.GlobalCooldownRatio;
            PlayerFeet = snapshot.PlayerFeet;
            HasTarget = snapshot.HasTarget;
            TargetFeet = snapshot.TargetFeet;
        }
    }
}
