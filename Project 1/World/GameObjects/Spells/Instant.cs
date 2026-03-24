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
                return RollValue(minValue, maxValue);
            }
        }

        public override string Description => BuildDescription(minValue, maxValue);

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

        //Deprectated?
        //public override bool Trigger(Entity aCaster, Entity aTarget, double aScalar = 1.0)
        //{
        //    return TriggerCore(aCaster, aTarget, minValue, maxValue, aScalar);
        //}

        public override string GetRankDescription(SpellData spellData, int spellRank)
        {
            int rankedMin = spellData.ScaleInstantValueForRank(minValue, spellRank);
            int rankedMax = spellData.ScaleInstantValueForRank(maxValue, spellRank);
            return BuildDescription(rankedMin, rankedMax);
        }

        public string GetRankDescriptionAsOverTimeTick(SpellData spellData, int spellRank, int tickCount)
        {
            int rankedMin = spellData.ScaleOverTimeTickValueForRank(minValue, tickCount, spellRank);
            int rankedMax = spellData.ScaleOverTimeTickValueForRank(maxValue, tickCount, spellRank);
            return BuildDescription(rankedMin, rankedMax);
        }

        public string GetRankDescriptionAsOverTimeTickWithTotal(SpellData spellData, int spellRank, int tickCount)
        {
            int safeTickCount = Math.Max(1, tickCount);
            int rankedTickMin = spellData.ScaleOverTimeTickValueForRank(minValue, safeTickCount, spellRank);
            int rankedTickMax = spellData.ScaleOverTimeTickValueForRank(maxValue, safeTickCount, spellRank);
            int totalMin = rankedTickMin * safeTickCount;
            int totalMax = rankedTickMax * safeTickCount;

            return BuildOverTimeDescription(rankedTickMin, rankedTickMax, totalMin, totalMax);
        }

        public bool TriggerRanked(Entity aCaster, Entity aTarget, SpellData spellData, int spellRank, double aScalar = 1.0, bool scaleAsTotalOverTime = false, int tickCount = 1)
        {
            int rankedMin = scaleAsTotalOverTime
                ? spellData.ScaleOverTimeTickValueForRank(minValue, tickCount, spellRank)
                : spellData.ScaleInstantValueForRank(minValue, spellRank);
            int rankedMax = scaleAsTotalOverTime
                ? spellData.ScaleOverTimeTickValueForRank(maxValue, tickCount, spellRank)
                : spellData.ScaleInstantValueForRank(maxValue, spellRank);

            return TriggerCore(aCaster, aTarget, rankedMin, rankedMax, aScalar);
        }

        bool TriggerCore(Entity aCaster, Entity aTarget, int currentMinValue, int currentMaxValue, double aScalar)
        {
            ThreadAffinity.AssertSimThread();
            Type currentType = currentMinValue > 0 ? Type.Heal : Type.Attack;
            int randomValue = RollValue(currentMinValue, currentMaxValue);
            if (currentType == Type.Attack)
            {
                double finalValue = randomValue;
                if (StatSource == AbilityStatSource.Spell)
                {
                    int spellPower = GetSpellPower(aCaster);
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

        static int RollValue(int minValue, int maxValue)
        {
            if (minValue == maxValue) return Math.Abs(minValue);

            if (minValue > 0)
            {
                int low = Math.Min(minValue, maxValue);
                int high = Math.Max(minValue, maxValue);
                return RandomManager.RollInt(low, high);
            }

            int minMagnitude = Math.Min(Math.Abs(minValue), Math.Abs(maxValue));
            int maxMagnitude = Math.Max(Math.Abs(minValue), Math.Abs(maxValue));
            return RandomManager.RollInt(minMagnitude, maxMagnitude);
        }

        string BuildDescription(int currentMinValue, int currentMaxValue)
        {
            Type currentType = currentMinValue > 0 ? Type.Heal : Type.Attack;
            if (currentType == Type.Attack)
            {
                string damageTypeString = damageType.ToString();
                if (Math.Abs(currentMinValue) == Math.Abs(currentMaxValue))
                {
                    return $"Deals {Math.Abs(currentMinValue)} {damageTypeString} damage.";
                }

                int low = Math.Min(Math.Abs(currentMinValue), Math.Abs(currentMaxValue));
                int high = Math.Max(Math.Abs(currentMinValue), Math.Abs(currentMaxValue));
                return $"Deals {low} to {high} {damageTypeString} damage.";
            }

            if (Math.Abs(currentMinValue) == Math.Abs(currentMaxValue))
            {
                return $"Heals for {Math.Abs(currentMinValue)}.";
            }

            int healLow = Math.Min(Math.Abs(currentMinValue), Math.Abs(currentMaxValue));
            int healHigh = Math.Max(Math.Abs(currentMinValue), Math.Abs(currentMaxValue));
            return $"Heals for {healLow} to {healHigh}.";
        }

        string BuildOverTimeDescription(int tickMinValue, int tickMaxValue, int totalMinValue, int totalMaxValue)
        {
            Type currentType = tickMinValue > 0 ? Type.Heal : Type.Attack;
            int tickLow = Math.Min(Math.Abs(tickMinValue), Math.Abs(tickMaxValue));
            int tickHigh = Math.Max(Math.Abs(tickMinValue), Math.Abs(tickMaxValue));
            int totalLow = Math.Min(Math.Abs(totalMinValue), Math.Abs(totalMaxValue));
            int totalHigh = Math.Max(Math.Abs(totalMinValue), Math.Abs(totalMaxValue));

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
