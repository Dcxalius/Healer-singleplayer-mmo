using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spells;
using Project_1.World.GameObjects.Unit.Stats.Secondary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.World.GameObjects.Spells.SpellEffects
{
    internal class CrowdControlEffect : LastingEffect
    {
        Type type;

        public CrowdControlEffect(double aDuration, string aName, bool aIsBinary, HashSet<SpellSchool> aSpellSchools) : base(aDuration, aName, aIsBinary, aSpellSchools)
        {
        }

        public enum Type
        {
            Stun, //Total loss of control and movement
            Root, //Loss of movement but not control
            Slow, //Movement speed reduced by a percentage
            Silence, //Loss of ability to cast spells, but not loss of movement or other control
            Disarm, // Loss of ability to attack, but not loss of movement or other control
            SchoolLock, //Loss of ability to use spells of a certain school, but not loss of movement or other control
            Charm, // Unit is charmed and fights for the enemy
            Confusion, //Unit is randomly moving, loss of control
            Fear, //Unit is moving away from the source of the fear, loss of control
            Knockback, //Unit is pushed away from the source of the knockback
            Polymorph, //Unit is transformed into a different creature
            Sleep, //Unit is asleep and cannot move or take actions until hit
            Taunt, //Unit is forced to attack the source of the taunt
            Hex, //Unit is transformed into a critter and cannot attack or cast spells, but can move
            Banish //Unit is banished to another dimension and cannot interact or be interacted with
        }

        public static bool HasControl(Type t)
        {
            return t switch
            {
                Type.Stun => false,
                Type.Root => true,
                Type.Slow => true,
                Type.Silence => true,
                Type.Disarm => true,
                Type.SchoolLock => true,
                Type.Charm => false,
                Type.Confusion => false,
                Type.Fear => false,
                Type.Knockback => false,
                Type.Polymorph => false,
                Type.Sleep => false,
                Type.Taunt => true,
                Type.Hex => false,
                Type.Banish => false,
                _ => throw new ArgumentOutOfRangeException(nameof(t), $"Not expected crowd control type: {t}")
            };
        }

        public static bool CanMove(Type t)
        {
            return t switch
            {
                Type.Stun => false,
                Type.Root => false,
                Type.Slow => true,
                Type.Silence => true,
                Type.Disarm => true,
                Type.SchoolLock => true,
                Type.Charm => true,
                Type.Confusion => true,
                Type.Fear => true,
                Type.Knockback => true,
                Type.Polymorph => true,
                Type.Sleep => false,
                Type.Taunt => true,
                Type.Hex => true,
                Type.Banish => false,
                _ => throw new ArgumentOutOfRangeException(nameof(t), $"Not expected crowd control type: {t}")
            };
        }

        public override double CalculatePower(Spell aSpell, int aRank)
        {
            throw new NotImplementedException();
        }

        public override string GetRankDescription(Spell aSpell, int aRank)
        {
            throw new NotImplementedException();
        }

        public override bool Trigger(Entity aCaster, Entity aTarget, Spell aSpell)
        {
            throw new NotImplementedException();
        }
    }
}
