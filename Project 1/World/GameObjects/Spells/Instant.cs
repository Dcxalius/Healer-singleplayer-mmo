using Newtonsoft.Json;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
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

        int RandomValue => RollValue(valueRange);

        public override string Description => BuildDescription(valueRange);

        DamageType damageType;
        readonly (int min, int max) valueRange;

        Type type => valueRange.min > 0 ? Type.Heal : Type.Attack;
        public override AbilityStatSource StatSource => type == Type.Attack && damageType == DamageType.Physical
            ? AbilityStatSource.Attack
            : AbilityStatSource.Spell;

        public override double CalculatePower(Spell aSpell, int aRank)
        {
            int rankedMin = aSpell.ScaleInstantValueForRank(valueRange.min, aRank);
            int rankedMax = aSpell.ScaleInstantValueForRank(valueRange.max, aRank);

            return (Math.Abs(rankedMin) + Math.Abs(rankedMax)) / 2.0;
        }

        [JsonConstructor]
        public Instant(string name, int minValue, int maxValue, DamageType damageType, bool isBinary, HashSet<SpellSchool> spellSchools) : base(name, isBinary, spellSchools)
        {
            Debug.Assert(maxValue != 0, "Tried to make Instant effect with no effect.");
            valueRange = (minValue, maxValue);
            this.damageType = damageType;
        }

        //Deprectated?
        //public override bool Trigger(Entity aCaster, Entity aTarget, double aScalar = 1.0)
        //{
        //    return TriggerCore(aCaster, aTarget, minValue, maxValue, aScalar);
        //}

        public override string GetRankDescription(Spell spell, int spellRank)
        {
            int rankedMin = spell.ScaleInstantValueForRank(valueRange.min, spellRank);
            int rankedMax = spell.ScaleInstantValueForRank(valueRange.max, spellRank);
            return BuildDescription((rankedMin, rankedMax));
        }

        public string GetRankDescriptionAsOverTimeTick(Spell spell, int spellRank, int tickCount) => BuildDescription(spell.ScaleOverTimeTickValueForRank(valueRange, tickCount, spellRank));

        public string GetRankDescriptionAsOverTimeTickWithTotal(Spell spell, int spellRank, int tickCount)
        {
            (int min, int max) rankedTick = spell.ScaleOverTimeTickValueForRank(valueRange, tickCount, spellRank);
            return BuildOverTimeDescription(rankedTick, (rankedTick.min * tickCount, rankedTick.max * tickCount));
        }

        public override bool Trigger(Entity aCaster, Entity aTarget, Spell aSpell)
        {
            int min = aSpell.ScaleInstantValueForRank(valueRange.min, aSpell.Rank);
            int max = aSpell.ScaleInstantValueForRank(valueRange.max, aSpell.Rank);
            return TriggerCore(aCaster, aTarget, min, max, aSpell.GetScalar(this));
        }
        public bool TriggerPeriodic(Entity aCaster, Entity aTarget, Spell aSpell, int maxTicks = 1)
        {
            (int min, int max) values = aSpell.ScaleOverTimeTickValueForRank(valueRange, maxTicks, aSpell.Rank);
            return TriggerCore(aCaster, aTarget, values.min, values.max, aSpell.GetScalar(this));
        }

        bool TriggerCore(Entity aCaster, Entity aTarget, int currentMinValue, int currentMaxValue, double aScalar)
        {
            ThreadAffinity.AssertSimThread();
            Type currentType = currentMinValue > 0 ? Type.Heal : Type.Attack;
            int randomValue = RollValue((currentMinValue, currentMaxValue));
            if (currentType == Type.Attack)
            {
                double finalValue = randomValue;
                if (StatSource == AbilityStatSource.Spell)
                {
                    int spellPower = GetSpellPower(aCaster);
                    //TODO: Check if this clamp is real
                    finalValue += spellPower * Math.Clamp(aScalar, 0.0, 1.0);
                }

                aTarget.RecieveSpellAttack(aCaster, this, new Damage(Math.Max(1, finalValue), damageType));
                return true;
            }

            if (currentType == Type.Heal)
            {
                int spellPower = GetSpellPower(aCaster);
                double finalValue = randomValue + spellPower * Math.Clamp(aScalar, 0.0, 1.0);
                aTarget.TakeHealing(aCaster, (float)Math.Max(1, finalValue));
                return true;

            }
            return false;
        }

        static int RollValue((int min, int max) valueRange)
        {
            if (valueRange.min == valueRange.max) return Math.Abs(valueRange.min);

            if (valueRange.min > 0)
            {
                int low = Math.Min(valueRange.min, valueRange.max);
                int high = Math.Max(valueRange.min, valueRange.max);
                return RandomManager.RollInt(low, high);
            }

            int minMagnitude = Math.Min(Math.Abs(valueRange.min), Math.Abs(valueRange.max));
            int maxMagnitude = Math.Max(Math.Abs(valueRange.min), Math.Abs(valueRange.max));
            return RandomManager.RollInt(minMagnitude, maxMagnitude);
        }

        string BuildDescription((int currentMin, int currentMax) value)
        {
            Type currentType = value.currentMin > 0 ? Type.Heal : Type.Attack;
            if (currentType == Type.Attack)
            {
                string damageTypeString = damageType.ToString();
                if (Math.Abs(value.currentMin) == Math.Abs(value.currentMax))
                {
                    return $"Deals {Math.Abs(value.currentMin)} {damageTypeString} damage.";
                }

                int low = Math.Min(Math.Abs(value.currentMin), Math.Abs(value.currentMax));
                int high = Math.Max(Math.Abs(value.currentMin), Math.Abs(value.currentMax));
                return $"Deals {low} to {high} {damageTypeString} damage.";
            }

            if (Math.Abs(value.currentMin) == Math.Abs(value.currentMax))
            {
                return $"Heals for {Math.Abs(value.currentMin)}.";
            }

            int healLow = Math.Min(Math.Abs(value.currentMin), Math.Abs(value.currentMax));
            int healHigh = Math.Max(Math.Abs(value.currentMin), Math.Abs(value.currentMax));
            return $"Heals for {healLow} to {healHigh}.";
        }

        string BuildOverTimeDescription((int min, int max) tick, (int min, int max) total)
        {
            Type currentType = tick.min > 0 ? Type.Heal : Type.Attack;
            int tickLow = Math.Min(Math.Abs(tick.min), Math.Abs(tick.max));
            int tickHigh = Math.Max(Math.Abs(tick.min), Math.Abs(tick.max));
            int totalLow = Math.Min(Math.Abs(total.min), Math.Abs(total.max));
            int totalHigh = Math.Max(Math.Abs(total.min), Math.Abs(total.max));

            if (currentType == Type.Attack)
            {
                string damageTypeString = damageType.ToString();
                if (tickLow == tickHigh && totalLow == totalHigh)
                {
                    return $"Deals {tickLow} {damageTypeString} damage every tick ({totalLow} total).";
                }

                return $"Deals {tickLow} to {tickHigh} {damageTypeString} damage every tick ({totalLow} to {totalHigh} total).";
            }

            if (tickLow == tickHigh && totalLow == totalHigh)
            {
                return $"Heals for {tickLow} every tick ({totalLow} total).";
            }

            return $"Heals for {tickLow} to {tickHigh} every tick ({totalLow} to {totalHigh} total).";
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
