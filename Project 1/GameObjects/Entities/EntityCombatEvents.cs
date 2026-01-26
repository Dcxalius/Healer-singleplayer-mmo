using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Unit;

namespace Project_1.GameObjects.Entities
{
    public enum CombatEventType
    {
        Dodge = 0,
        Parry = 1,
        Block = 2,
        Miss = 3,
        Glancing = 4,
        Crit = 5,
        Crushing = 6,
        Hit = 7,
        SpellHit = 8,
        SpellResist = 9,
        SpellHitTaken = 10,
        SpellResistedByTarget = 11,
        AttackDodged = 12,
        AttackParried = 13,
        AttackBlocked = 14,
        MissedBy = 15,
        GlancingTaken = 16,
        CritTaken = 17,
        CrushingTaken = 18,
        HitTaken = 19,
        SpellCrit = 20,
        SpellCritTaken = 21
    }

    internal readonly struct MissEvent
    {
        public MissEvent(Entity attacker, Entity defender, Attack attack)
        {
            Attacker = attacker;
            Defender = defender;
            Attack = attack;
        }

        public Entity Attacker { get; }
        public Entity Defender { get; }
        public Attack Attack { get; }
    }

    internal readonly struct MissedByEvent
    {
        public MissedByEvent(Entity attacker, Entity defender, Attack attack)
        {
            Attacker = attacker;
            Defender = defender;
            Attack = attack;
        }

        public Entity Attacker { get; }
        public Entity Defender { get; }
        public Attack Attack { get; }
    }

    internal readonly struct DodgeEvent
    {
        public DodgeEvent(Entity attacker, Entity defender, Attack attack)
        {
            Attacker = attacker;
            Defender = defender;
            Attack = attack;
        }

        public Entity Attacker { get; }
        public Entity Defender { get; }
        public Attack Attack { get; }
    }

    internal readonly struct AttackDodgedEvent
    {
        public AttackDodgedEvent(Entity attacker, Entity defender, Attack attack)
        {
            Attacker = attacker;
            Defender = defender;
            Attack = attack;
        }

        public Entity Attacker { get; }
        public Entity Defender { get; }
        public Attack Attack { get; }
    }

    internal readonly struct ParryEvent
    {
        public ParryEvent(Entity attacker, Entity defender, Attack attack)
        {
            Attacker = attacker;
            Defender = defender;
            Attack = attack;
        }

        public Entity Attacker { get; }
        public Entity Defender { get; }
        public Attack Attack { get; }
    }

    internal readonly struct AttackParriedEvent
    {
        public AttackParriedEvent(Entity attacker, Entity defender, Attack attack)
        {
            Attacker = attacker;
            Defender = defender;
            Attack = attack;
        }

        public Entity Attacker { get; }
        public Entity Defender { get; }
        public Attack Attack { get; }
    }

    internal readonly struct BlockEvent
    {
        public BlockEvent(Entity attacker, Entity defender, Attack attack, bool fullyBlocked)
        {
            Attacker = attacker;
            Defender = defender;
            Attack = attack;
            FullyBlocked = fullyBlocked;
        }

        public Entity Attacker { get; }
        public Entity Defender { get; }
        public Attack Attack { get; }
        public bool FullyBlocked { get; }
    }

    internal readonly struct AttackBlockedEvent
    {
        public AttackBlockedEvent(Entity attacker, Entity defender, Attack attack, bool fullyBlocked)
        {
            Attacker = attacker;
            Defender = defender;
            Attack = attack;
            FullyBlocked = fullyBlocked;
        }

        public Entity Attacker { get; }
        public Entity Defender { get; }
        public Attack Attack { get; }
        public bool FullyBlocked { get; }
    }

    internal readonly struct HitEvent
    {
        public HitEvent(Entity attacker, Entity defender, Attack attack, Unit.Stats.HitTable.HitResult result, Unit.Stats.Damage damage)
        {
            Attacker = attacker;
            Defender = defender;
            Attack = attack;
            Result = result;
            Damage = damage;
        }

        public Entity Attacker { get; }
        public Entity Defender { get; }
        public Attack Attack { get; }
        public Unit.Stats.HitTable.HitResult Result { get; }
        public Unit.Stats.Damage Damage { get; }
    }

    internal readonly struct HitTakenEvent
    {
        public HitTakenEvent(Entity attacker, Entity defender, Attack attack, Unit.Stats.HitTable.HitResult result, Unit.Stats.Damage damage)
        {
            Attacker = attacker;
            Defender = defender;
            Attack = attack;
            Result = result;
            Damage = damage;
        }

        public Entity Attacker { get; }
        public Entity Defender { get; }
        public Attack Attack { get; }
        public Unit.Stats.HitTable.HitResult Result { get; }
        public Unit.Stats.Damage Damage { get; }
    }

    internal readonly struct CritEvent
    {
        public CritEvent(Entity attacker, Entity defender, Attack attack, Unit.Stats.Damage damage)
        {
            Attacker = attacker;
            Defender = defender;
            Attack = attack;
            Damage = damage;
        }

        public Entity Attacker { get; }
        public Entity Defender { get; }
        public Attack Attack { get; }
        public Unit.Stats.Damage Damage { get; }
    }

    internal readonly struct CritTakenEvent
    {
        public CritTakenEvent(Entity attacker, Entity defender, Attack attack, Unit.Stats.Damage damage)
        {
            Attacker = attacker;
            Defender = defender;
            Attack = attack;
            Damage = damage;
        }

        public Entity Attacker { get; }
        public Entity Defender { get; }
        public Attack Attack { get; }
        public Unit.Stats.Damage Damage { get; }
    }

    internal readonly struct GlancingEvent
    {
        public GlancingEvent(Entity attacker, Entity defender, Attack attack, Unit.Stats.Damage damage)
        {
            Attacker = attacker;
            Defender = defender;
            Attack = attack;
            Damage = damage;
        }

        public Entity Attacker { get; }
        public Entity Defender { get; }
        public Attack Attack { get; }
        public Unit.Stats.Damage Damage { get; }
    }

    internal readonly struct GlancingTakenEvent
    {
        public GlancingTakenEvent(Entity attacker, Entity defender, Attack attack, Unit.Stats.Damage damage)
        {
            Attacker = attacker;
            Defender = defender;
            Attack = attack;
            Damage = damage;
        }

        public Entity Attacker { get; }
        public Entity Defender { get; }
        public Attack Attack { get; }
        public Unit.Stats.Damage Damage { get; }
    }

    internal readonly struct CrushingEvent
    {
        public CrushingEvent(Entity attacker, Entity defender, Attack attack, Unit.Stats.Damage damage)
        {
            Attacker = attacker;
            Defender = defender;
            Attack = attack;
            Damage = damage;
        }

        public Entity Attacker { get; }
        public Entity Defender { get; }
        public Attack Attack { get; }
        public Unit.Stats.Damage Damage { get; }
    }

    internal readonly struct CrushingTakenEvent
    {
        public CrushingTakenEvent(Entity attacker, Entity defender, Attack attack, Unit.Stats.Damage damage)
        {
            Attacker = attacker;
            Defender = defender;
            Attack = attack;
            Damage = damage;
        }

        public Entity Attacker { get; }
        public Entity Defender { get; }
        public Attack Attack { get; }
        public Unit.Stats.Damage Damage { get; }
    }

    internal readonly struct SpellHitEvent //TODO: Ponder if SpellEffect is the correct arg or if it should be Spell or even SpellData
    {
        public SpellHitEvent(Entity caster, Entity target, SpellEffect effect)
        {
            Caster = caster;
            Target = target;
            Effect = effect;
        }

        public Entity Caster { get; }
        public Entity Target { get; }
        public SpellEffect Effect { get; }
    }

    internal readonly struct SpellCritEvent
    {
        public SpellCritEvent(Entity caster, Entity target, SpellEffect effect)
        {
            Caster = caster;
            Target = target;
            Effect = effect;
        }

        public Entity Caster { get; }
        public Entity Target { get; }
        public SpellEffect Effect { get; }
    }

    internal readonly struct SpellResistEvent
    {
        public SpellResistEvent(Entity caster, Entity target, SpellEffect effect, bool immune)
        {
            Caster = caster;
            Target = target;
            Effect = effect;
            Immune = immune;
        }

        public Entity Caster { get; }
        public Entity Target { get; }
        public SpellEffect Effect { get; }
        public bool Immune { get; }
    }

    internal readonly struct SpellHitTakenEvent
    {
        public SpellHitTakenEvent(Entity caster, Entity target, SpellEffect effect)
        {
            Caster = caster;
            Target = target;
            Effect = effect;
        }

        public Entity Caster { get; }
        public Entity Target { get; }
        public SpellEffect Effect { get; }
    }

    internal readonly struct SpellCritTakenEvent
    {
        public SpellCritTakenEvent(Entity caster, Entity target, SpellEffect effect)
        {
            Caster = caster;
            Target = target;
            Effect = effect;
        }

        public Entity Caster { get; }
        public Entity Target { get; }
        public SpellEffect Effect { get; }
    }

    internal readonly struct SpellResistedByTargetEvent
    {
        public SpellResistedByTargetEvent(Entity caster, Entity target, SpellEffect effect, bool immune)
        {
            Caster = caster;
            Target = target;
            Effect = effect;
            Immune = immune;
        }

        public Entity Caster { get; }
        public Entity Target { get; }
        public SpellEffect Effect { get; }
        public bool Immune { get; }
    }
}
