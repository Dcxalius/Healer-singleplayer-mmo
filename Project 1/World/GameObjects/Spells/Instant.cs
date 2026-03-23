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
            return TriggerCore(aCaster, aTarget, minValue, maxValue, aScalar);
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
