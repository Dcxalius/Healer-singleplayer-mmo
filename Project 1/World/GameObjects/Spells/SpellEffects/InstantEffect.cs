using Newtonsoft.Json;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Spell = Project_1.GameObjects.Spells.Spell;

namespace Project_1.World.GameObjects.Spells.SpellEffects
{
    internal class InstantEffect : SpellEffect
    {
        enum Type
        {
            Heal,
            Attack
        }

        int RandomValue => RollValue(valueRange);

        public override string Description => BuildDescription(valueRange);

        DamageType damageType;
        readonly (int min, int max) valueRange;

        Type type => GetEffectType(valueRange);
        public override AbilityStatSource StatSource => type == Type.Attack && damageType == DamageType.Physical
            ? AbilityStatSource.Attack
            : AbilityStatSource.Spell;

        public override double CalculatePower(Spell aSpell, int aRank)
        {
            (int min, int max) ranked = aSpell.ScaleInstantValueForRankAndTalent(valueRange, aRank);
            return (Math.Abs(ranked.min) + Math.Abs(ranked.max)) / 2.0;
        }

        [JsonConstructor]
        public InstantEffect(string name, int minValue, int maxValue, DamageType damageType, bool isBinary, HashSet<SpellSchool> spellSchools) : base(name, isBinary, spellSchools)
        {
            Debug.Assert(maxValue != 0, "Tried to make Instant effect with no effect.");
            valueRange = (minValue, maxValue);
            Debug.Assert(minValue > 0 ? maxValue >= minValue : maxValue <= minValue, "Min value should be the closest value to 0.");
            this.damageType = damageType;
        }

        //Deprectated?
        //public override bool Trigger(Entity aCaster, Entity aTarget, double aScalar = 1.0)
        //{
        //    return TriggerCore(aCaster, aTarget, minValue, maxValue, aScalar);
        //}

        public override string GetRankDescription(Spell spell, int spellRank) => BuildDescription(spell.ScaleInstantValueForRankAndTalent(valueRange, spellRank));
        public string GetRankDescriptionAsOverTimeTick(Spell spell, int spellRank, int tickCount) => BuildDescription(spell.ScaleOverTimeTickValueForRank(valueRange, tickCount, spellRank));

        public string GetRankDescriptionAsOverTimeTickWithTotal(Spell spell, int spellRank, int tickCount)
        {
            (int min, int max) rankedTick = spell.ScaleOverTimeTickValueForRank(valueRange, tickCount, spellRank);
            return BuildOverTimeDescription(rankedTick, (rankedTick.min * tickCount, rankedTick.max * tickCount));
        }

        public override bool Trigger(Entity aCaster, Entity aTarget, Spell aSpell) => TriggerCore(aCaster, aTarget, aSpell.ScaleInstantValueForRankAndTalent(valueRange, aSpell.Rank), aSpell.GetScalar(this));
        public bool TriggerPeriodic(Entity aCaster, Entity aTarget, Spell aSpell, int maxTicks = 1) => TriggerCore(aCaster, aTarget, aSpell.ScaleOverTimeTickValueForRank(valueRange, maxTicks, aSpell.Rank), aSpell.GetScalar(this));

        bool TriggerCore(Entity aCaster, Entity aTarget, (int min, int max) currentValue, double aScalar)
        {
            ThreadAffinity.AssertSimThread();
            (int min, int max) currentRange = currentValue;
            int randomValue = RollValue(currentRange);
            double finalValue = randomValue;

            if (StatSource == AbilityStatSource.Spell)
            {
                finalValue += GetSpellPower(aCaster) * Math.Clamp(aScalar, 0.0, 1.0);
            }

            if (GetEffectType(currentRange) == Type.Attack)
            {
                aTarget.RecieveSpellAttack(aCaster, this, new Damage(Math.Max(1, finalValue), damageType));
                return true;
            }

            aTarget.TakeHealing(aCaster, (float)Math.Max(1, finalValue));
            return true;
        }

        static int RollValue((int min, int max) valueRange)
        {
            (int min, int max) magnitudeRange = GetMagnitudeRange(valueRange);
            if (magnitudeRange.min == magnitudeRange.max) return magnitudeRange.min;
            return RandomManager.RollInt(magnitudeRange.min, magnitudeRange.max);
        }

        string BuildDescription((int currentMin, int currentMax) value)
        {
            Type currentType = GetEffectType((value.currentMin, value.currentMax));
            (int min, int max) magnitudeRange = GetMagnitudeRange((value.currentMin, value.currentMax));
            if (currentType == Type.Attack)
            {
                string damageTypeString = damageType.ToString();
                if (magnitudeRange.min == magnitudeRange.max)
                {
                    return $"Deals {magnitudeRange.min} {damageTypeString} damage.";
                }

                return $"Deals {magnitudeRange.min} to {magnitudeRange.max} {damageTypeString} damage.";
            }

            if (magnitudeRange.min == magnitudeRange.max)
            {
                return $"Heals for {magnitudeRange.min}.";
            }

            return $"Heals for {magnitudeRange.min} to {magnitudeRange.max}.";
        }

        string BuildOverTimeDescription((int min, int max) tick, (int min, int max) total)
        {
            Type currentType = GetEffectType(tick);
            (int min, int max) tickMagnitudeRange = GetMagnitudeRange(tick);
            (int min, int max) totalMagnitudeRange = GetMagnitudeRange(total);

            if (currentType == Type.Attack)
            {
                string damageTypeString = damageType.ToString();
                if (tickMagnitudeRange.min == tickMagnitudeRange.max && totalMagnitudeRange.min == totalMagnitudeRange.max)
                {
                    return $"Deals {tickMagnitudeRange.min} {damageTypeString} damage every tick ({totalMagnitudeRange.min} total).";
                }

                return $"Deals {tickMagnitudeRange.min} to {tickMagnitudeRange.max} {damageTypeString} damage every tick ({totalMagnitudeRange.min} to {totalMagnitudeRange.max} total).";
            }

            if (tickMagnitudeRange.min == tickMagnitudeRange.max && totalMagnitudeRange.min == totalMagnitudeRange.max)
            {
                return $"Heals for {tickMagnitudeRange.min} every tick ({totalMagnitudeRange.min} total).";
            }

            return $"Heals for {tickMagnitudeRange.min} to {tickMagnitudeRange.max} every tick ({totalMagnitudeRange.min} to {totalMagnitudeRange.max} total).";
        }

        static Type GetEffectType((int min, int max) valueRange) => valueRange.min > 0 ? Type.Heal : Type.Attack;

        static (int min, int max) GetMagnitudeRange((int min, int max) valueRange)
        {
            Debug.Assert(valueRange.min > 0 ? valueRange.max >= valueRange.min : valueRange.max <= valueRange.min, "Min value should be the closest value to 0.");
            return GetEffectType(valueRange) == Type.Heal
                ? valueRange
                : (-valueRange.min, -valueRange.max);
        }

        int GetSpellPower(Entity aCaster)
        {
            ThreadAffinity.AssertSimThread();
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
