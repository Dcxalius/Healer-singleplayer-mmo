using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;

namespace Project_1.GameObjects.Spells
{
    internal static class SpellCastRouter
    {
        static bool initialized;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;

            MailboxManager.RegisterSimCommandType<SpellCastRequested>();
            MailboxManager.Sim.Subscribe<SpellCastRequested>(HandleSpellCastRequested);
        }

        static void HandleSpellCastRequested(SpellCastRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            if (!player.SpellBook.TryGetSpell(e.SpellName, out Spell spell)) return;

            if (spell.RequiresGroundTarget)
            {
                GroundTargetingController.Begin(spell);
                return;
            }

            GroundTargetingController.Cancel();
            player.StartCast(spell);
        }
    }
}
