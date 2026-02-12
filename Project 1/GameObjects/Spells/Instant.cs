using Newtonsoft.Json;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells
{
    internal class Instant : SpellEffect
    {
        enum Type
        {
            Heal,
            Attack
        }

        int RandomValue
        {
            get
            {
                if (minValue == maxValue) return Math.Abs(minValue);
                if (type == Type.Heal)
                {
                    return RandomManager.RollInt(minValue, maxValue);
                }
                if (type == Type.Attack)
                {
                    return RandomManager.RollInt(Math.Abs(minValue), Math.Abs(maxValue));
                }
                throw new NotImplementedException();
            }
        }

        DamageType damageType;
        int minValue;
        int maxValue;
        Type type => minValue > 0 ? Type.Heal : Type.Attack;
        public override AbilityStatSource StatSource => type == Type.Attack && damageType == DamageType.Physical
            ? AbilityStatSource.Attack
            : AbilityStatSource.Spell;

        [JsonConstructor]
        public Instant(string name, int minValue, int maxValue, DamageType damageType, bool isBinary, HashSet<SpellSchool> spellSchools) : base(name, isBinary, spellSchools)
        {
            Debug.Assert(maxValue != 0, "Tried to make effect with no effect.");
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.damageType = damageType;
        }

        public override bool Trigger(Entity aCaster, Entity aTarget, double aScalar = 1.0)
        {
            if (type == Type.Attack)
            {
                double finalValue = RandomValue;
                if (StatSource == AbilityStatSource.Spell)
                {
                    int spellPower = GetSpellPower(aCaster);
                    finalValue += spellPower * Math.Clamp(aScalar, 0.0, 1.0);
                }

                aTarget.RecieveSpellAttack(aCaster, this, new Damage(Math.Max(1, finalValue), damageType));
                return true;
            }

            if (type == Type.Heal)
            {
                int spellPower = GetSpellPower(aCaster);
                double finalValue = RandomValue + spellPower * Math.Clamp(aScalar, 0.0, 1.0);
                aTarget.TakeHealing(aCaster, (float)Math.Max(1, finalValue));
                return true;

            }
            return false;
        }

        int GetSpellPower(Entity aCaster)
        {
            if (type == Type.Heal)
            {
                if (SpellSchools != null && SpellSchools.Count > 0)
                {
                    return aCaster.SecondaryStats.Spell.SpellDamageForSchools(SpellSchools);
                }

                return aCaster.SecondaryStats.Spell.SpellDamageForSchool(SpellSchool.Healing);
            }

            if (SpellSchools == null || SpellSchools.Count == 0)
            {
                return 0;
            }

            return aCaster.SecondaryStats.Spell.SpellDamageForSchools(SpellSchools);
        }
    }
}
