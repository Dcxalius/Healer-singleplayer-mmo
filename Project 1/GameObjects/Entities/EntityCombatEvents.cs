using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Stats;
using Attack = Project_1.GameObjects.Unit.Attack;

namespace Project_1.GameObjects.Entities
{
    public enum CombatEventType
    {
        AttackMissed = 0,
        AttackDodged = 1,
        AttackParried = 2,
        AttackGlanced = 3,
        AttackBlocked = 4,
        AttackCrit = 5,
        AttackCrushed = 6,
        AttackHit = 7,
        SpellHit = 8,
        SpellCrit = 9,
        SpellResist = 10,
        MissedBy = 11,
        Dodge = 12,
        Parry = 13,
        GlancingTaken = 14,
        Block = 15,
        CritTaken = 16,
        CrushingTaken = 17,
        HitTaken = 18,
        SpellHitTaken = 19,
        SpellCritTaken = 20,
        SpellResistedByTarget = 21
    }

    internal readonly struct AttackMissedEvent
    {
        public AttackMissedEvent(Entity attacker, Entity defender, Attack attack)
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

    internal readonly struct AttackHitEvent
    {
        public AttackHitEvent(Entity attacker, Entity defender, Attack attack, HitTable.HitResult result, Damage damage)
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
        public HitTable.HitResult Result { get; }
        public Damage Damage { get; }
    }

    internal readonly struct HitTakenEvent
    {
        public HitTakenEvent(Entity attacker, Entity defender, Attack attack, HitTable.HitResult result, Damage damage)
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
        public HitTable.HitResult Result { get; }
        public Damage Damage { get; }
    }

    internal readonly struct AttackCritEvent
    {
        public AttackCritEvent(Entity attacker, Entity defender, Attack attack, Damage damage)
        {
            Attacker = attacker;
            Defender = defender;
            Attack = attack;
            Damage = damage;
        }

        public Entity Attacker { get; }
        public Entity Defender { get; }
        public Attack Attack { get; }
        public Damage Damage { get; }
    }

    internal readonly struct CritTakenEvent
    {
        public CritTakenEvent(Entity attacker, Entity defender, Attack attack, Damage damage)
        {
            Attacker = attacker;
            Defender = defender;
            Attack = attack;
            Damage = damage;
        }

        public Entity Attacker { get; }
        public Entity Defender { get; }
        public Attack Attack { get; }
        public Damage Damage { get; }
    }

    internal readonly struct AttackGlancedEvent
    {
        public AttackGlancedEvent(Entity attacker, Entity defender, Attack attack, Damage damage)
        {
            Attacker = attacker;
            Defender = defender;
            Attack = attack;
            Damage = damage;
        }

        public Entity Attacker { get; }
        public Entity Defender { get; }
        public Attack Attack { get; }
        public Damage Damage { get; }
    }

    internal readonly struct GlancingTakenEvent
    {
        public GlancingTakenEvent(Entity attacker, Entity defender, Attack attack, Damage damage)
        {
            Attacker = attacker;
            Defender = defender;
            Attack = attack;
            Damage = damage;
        }

        public Entity Attacker { get; }
        public Entity Defender { get; }
        public Attack Attack { get; }
        public Damage Damage { get; }
    }

    internal readonly struct AttackCrushedEvent
    {
        public AttackCrushedEvent(Entity attacker, Entity defender, Attack attack, Damage damage)
        {
            Attacker = attacker;
            Defender = defender;
            Attack = attack;
            Damage = damage;
        }

        public Entity Attacker { get; }
        public Entity Defender { get; }
        public Attack Attack { get; }
        public Damage Damage { get; }
    }

    internal readonly struct CrushingTakenEvent
    {
        public CrushingTakenEvent(Entity attacker, Entity defender, Attack attack, Damage damage)
        {
            Attacker = attacker;
            Defender = defender;
            Attack = attack;
            Damage = damage;
        }

        public Entity Attacker { get; }
        public Entity Defender { get; }
        public Attack Attack { get; }
        public Damage Damage { get; }
    }

    internal readonly struct SpellHitEvent
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
