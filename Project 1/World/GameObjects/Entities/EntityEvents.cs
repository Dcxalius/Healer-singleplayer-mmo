using Microsoft.Xna.Framework;
using Project_1.GameObjects.Unit.Stats;
using Project_1.World.GameObjects.Spells.SpellEffects;

namespace Project_1.GameObjects.Entities
{
    internal abstract partial class Entity
    {
        public EntityEventBus Events { get; } = new EntityEventBus();
        //TODO: Missing events should be implemented
        void PublishDodgeEvent(Entity aAttacker, Unit.Attack aAttack)
        {
            //TODO: Make the colors settable
            //TODO: Make the spawning text disableable
            SpawnFlyingText("Dodge", GetDirOfFloatingText(aAttacker.FeetPosition), Color.DarkGray, Color.Black, 1f);
            PublishToDefender(new DodgeEvent(aAttacker, this, aAttack));
            PublishToAttacker(aAttacker, new AttackDodgedEvent(aAttacker, this, aAttack));
        }

        void PublishParryEvent(Entity aAttacker, Unit.Attack aAttack)
        {
            //TODO: Make the colors settable
            //TODO: Make the spawning text disableable
            SpawnFlyingText("Parry", GetDirOfFloatingText(aAttacker.FeetPosition), Color.DarkSlateGray, Color.Black, 1f);
            PublishToDefender(new ParryEvent(aAttacker, this, aAttack));
            PublishToAttacker(aAttacker, new AttackParriedEvent(aAttacker, this, aAttack));
        }

        void PublishBlockEvent(Entity aAttacker, Unit.Attack aAttack, bool aFullyBlocked)
        {
            //TODO: Make the colors settable
            //TODO: Make the spawning text disableable
            //TODO: Seperate events for block and full block
            PublishToDefender(new BlockEvent(aAttacker, this, aAttack, aFullyBlocked));
            PublishToAttacker(aAttacker, new AttackBlockedEvent(aAttacker, this, aAttack, aFullyBlocked));
        }

        void PublishMissEvent(Entity aAttacker, Unit.Attack aAttack)
        {
            //TODO: Make the colors settable
            //TODO: Make the spawning text disableable
            SpawnFlyingText("Miss", GetDirOfFloatingText(aAttacker.FeetPosition), Color.Gray, Color.Black, 1f);
            PublishToAttacker(aAttacker, new AttackMissedEvent(aAttacker, this, aAttack));
            PublishToDefender(new MissedByEvent(aAttacker, this, aAttack));
        }

        void PublishHitEvent(Entity aAttacker, Unit.Attack aAttack, HitTable.HitResult aResult, Damage aDamageTaken)
        {
            var snapshot = new Damage(aDamageTaken); //Q: Why are we doing this?
            PublishToAttacker(aAttacker, new AttackHitEvent(aAttacker, this, aAttack, aResult, snapshot));
            PublishToDefender(new HitTakenEvent(aAttacker, this, aAttack, aResult, new Damage(snapshot)));
        }

        void PublishOutcomeEvent(Entity aAttacker, Unit.Attack aAttack, HitTable.HitResult aResult, Damage aDamageTaken)
        {
            var snapshot = new Damage(aDamageTaken); //Q: Why are we doing this?
            switch (aResult)
            {
                case HitTable.HitResult.Glancing:
                    PublishToAttacker(aAttacker, new AttackGlancedEvent(aAttacker, this, aAttack, snapshot));
                    PublishToDefender(new GlancingTakenEvent(aAttacker, this, aAttack, new Damage(snapshot)));
                    break;
                case HitTable.HitResult.Crit:
                    PublishToAttacker(aAttacker, new AttackCritEvent(aAttacker, this, aAttack, snapshot));
                    PublishToDefender(new CritTakenEvent(aAttacker, this, aAttack, new Damage(snapshot)));
                    break;
                case HitTable.HitResult.Crushing:
                    PublishToAttacker(aAttacker, new AttackCrushedEvent(aAttacker, this, aAttack, snapshot));
                    PublishToDefender(new CrushingTakenEvent(aAttacker, this, aAttack, new Damage(snapshot)));
                    break;
                default:
                    break;
            }
        }

        void PublishSpellHitEvent(Entity aCaster, SpellEffect aSpellEffect)
        {
            PublishToAttacker(aCaster, new SpellHitEvent(aCaster, this, aSpellEffect));
        }

        void PublishSpellCritEvent(Entity aCaster, SpellEffect aSpellEffect)
        {
            PublishToAttacker(aCaster, new SpellCritEvent(aCaster, this, aSpellEffect));
        }

        void PublishSpellResistEvent(Entity aCaster, SpellEffect aSpellEffect, bool aImmune)
        {
            PublishToAttacker(aCaster, new SpellResistEvent(aCaster, this, aSpellEffect, aImmune));
        }

        void PublishSpellHitTakenEvent(Entity aCaster, SpellEffect aSpellEffect)
        {
            var evt = new SpellHitTakenEvent(aCaster, this, aSpellEffect);
            Events.Publish(evt);
        }

        void PublishSpellCritTakenEvent(Entity aCaster, SpellEffect aSpellEffect)
        {
            var evt = new SpellCritTakenEvent(aCaster, this, aSpellEffect);
            Events.Publish(evt);
        }

        void PublishSpellResistedByTargetEvent(Entity aCaster, SpellEffect aSpellEffect, bool aImmune)
        {
            var evt = new SpellResistedByTargetEvent(aCaster, this, aSpellEffect, aImmune);
            Events.Publish(evt);
        }

        void PublishToDefender<T>(T aEvent)
        {
            Events.Publish(aEvent);
        }

        void PublishToAttacker<T>(Entity aAttacker, T aEvent)
        {
            if (aAttacker == null) return;
            aAttacker.Events.Publish(aEvent);
        }
    }
}
