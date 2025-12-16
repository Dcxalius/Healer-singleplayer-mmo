using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spells;

namespace Project_1.Messaging.Events
{
    internal readonly struct CastChannelStarted
    {
        public CastChannelStarted(Entity caster, Spell spell, double startTime, double duration)
        {
            Caster = caster;
            Spell = spell;
            StartTime = startTime;
            Duration = duration;
        }

        public Entity Caster { get; }
        public Spell Spell { get; }
        public double StartTime { get; }
        public double Duration { get; }
    }

    internal readonly struct CastChannelProgress
    {
        public CastChannelProgress(Entity caster, Spell spell, float progress01)
        {
            Caster = caster;
            Spell = spell;
            Progress01 = progress01;
        }

        public Entity Caster { get; }
        public Spell Spell { get; }
        public float Progress01 { get; }
    }

    internal readonly struct CastChannelCancelled
    {
        public CastChannelCancelled(Entity caster, Spell spell)
        {
            Caster = caster;
            Spell = spell;
        }

        public Entity Caster { get; }
        public Spell Spell { get; }
    }

    internal readonly struct CastChannelFinished
    {
        public CastChannelFinished(Entity caster, Spell spell)
        {
            Caster = caster;
            Spell = spell;
        }

        public Entity Caster { get; }
        public Spell Spell { get; }
    }
}
