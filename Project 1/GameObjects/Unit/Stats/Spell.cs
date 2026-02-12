using Project_1.Items.SubTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class Spell
    {
        readonly bool useAttackFallback;
        readonly Attack attackFallback;

        public float CriticalChance => CriticalChanceForSchool(SpellSchool.Base);
        public float CriticalDamage => CriticalDamageForSchool(SpellSchool.Base);
        public int FlatPenetration => FlatPenetrationForSchool(SpellSchool.Base);
        public float PercentPenetration => PercentPenetrationForSchool(SpellSchool.Base);
        public float BonusHitChance => (float)BonusHitChanceForSchool(SpellSchool.Base);

        HashSet<SpellStats> spellDamage;

        public Spell(UnitData unitData)
        {
            spellDamage = new HashSet<SpellStats>();
            Refresh(unitData);
        }

        public Spell(Attack aAttackFallback)
        {
            attackFallback = aAttackFallback ?? throw new ArgumentNullException(nameof(aAttackFallback));
            useAttackFallback = true;
            spellDamage = new HashSet<SpellStats> { new SpellStats(SpellSchool.Base) };
        }

        public void Refresh(UnitData unitData)
        {
            if (useAttackFallback)
            {
                return;
            }

            spellDamage.Clear();
            foreach (SpellSchool school in Enum.GetValues(typeof(SpellSchool)))
            {
                SpellStats stats = new SpellStats(school, unitData);
                if (!HasAnyStat(stats))
                {
                    continue;
                }

                spellDamage.Add(stats);
            }

            if (!spellDamage.Any(x => x.SpellSchool == SpellSchool.Base))
            {
                spellDamage.Add(new SpellStats(SpellSchool.Base));
            }
        }

        public float CriticalChanceForSchool(SpellSchool aSchool)
        {
            if (useAttackFallback)
            {
                return Clamp01(attackFallback.CriticalChance);
            }

            return Clamp01((float)GetValueWithBase<double>(aSchool, "SpellCritChance"));
        }

        public float CriticalChanceForSchools(HashSet<SpellSchool> aSchools)
        {
            if (useAttackFallback)
            {
                return Clamp01(attackFallback.CriticalChance);
            }

            SpellSchool[] schools = GetEffectiveSchools(aSchools);
            double sum = 0;
            for (int i = 0; i < schools.Length; i++)
            {
                sum += GetValueWithBase<double>(schools[i], "SpellCritChance");
            }

            return Clamp01((float)(sum / schools.Length));
        }

        public float CriticalDamageForSchool(SpellSchool aSchool)
        {
            if (useAttackFallback)
            {
                return attackFallback.CriticalDamage;
            }

            return (float)GetValueWithBase<double>(aSchool, "SpellCritDamage");
        }

        public float CriticalDamageForSchools(HashSet<SpellSchool> aSchools)
        {
            if (useAttackFallback)
            {
                return attackFallback.CriticalDamage;
            }

            SpellSchool[] schools = GetEffectiveSchools(aSchools);
            double sum = 0;
            for (int i = 0; i < schools.Length; i++)
            {
                sum += GetValueWithBase<double>(schools[i], "SpellCritDamage");
            }

            return (float)(sum / schools.Length);
        }

        public int FlatPenetrationForSchool(SpellSchool aSchool)
        {
            if (useAttackFallback)
            {
                return attackFallback.FlatPenetration;
            }

            return GetValueWithBase<int>(aSchool, "SpellFlatPenetration");
        }

        public int SpellDamageForSchool(SpellSchool aSchool)
        {
            if (useAttackFallback)
            {
                return 0;
            }

            return GetValueWithBase<int>(aSchool, "SpellDamage");
        }

        public int SpellDamageForSchools(HashSet<SpellSchool> aSchools)
        {
            if (useAttackFallback)
            {
                return 0;
            }

            SpellSchool[] schools = GetEffectiveSchools(aSchools);
            double sum = 0;
            for (int i = 0; i < schools.Length; i++)
            {
                sum += GetValueWithBase<int>(schools[i], "SpellDamage");
            }

            return (int)Math.Round(sum / schools.Length, MidpointRounding.AwayFromZero);
        }

        public int FlatPenetrationForSchools(HashSet<SpellSchool> aSchools)
        {
            if (useAttackFallback)
            {
                return attackFallback.FlatPenetration;
            }

            SpellSchool[] schools = GetEffectiveSchools(aSchools);
            double sum = 0;
            for (int i = 0; i < schools.Length; i++)
            {
                sum += GetValueWithBase<int>(schools[i], "SpellFlatPenetration");
            }

            return (int)Math.Round(sum / schools.Length, MidpointRounding.AwayFromZero);
        }

        public float PercentPenetrationForSchool(SpellSchool aSchool)
        {
            if (useAttackFallback)
            {
                return Clamp01(attackFallback.PercentPenetration);
            }

            return Clamp01((float)GetValueWithBase<double>(aSchool, "SpellPercentPenetration"));
        }

        public float PercentPenetrationForSchools(HashSet<SpellSchool> aSchools)
        {
            if (useAttackFallback)
            {
                return Clamp01(attackFallback.PercentPenetration);
            }

            SpellSchool[] schools = GetEffectiveSchools(aSchools);
            double sum = 0;
            for (int i = 0; i < schools.Length; i++)
            {
                sum += GetValueWithBase<double>(schools[i], "SpellPercentPenetration");
            }

            return Clamp01((float)(sum / schools.Length));
        }

        public double BonusHitChanceForSchool(SpellSchool aSchool)
        {
            if (useAttackFallback)
            {
                return Clamp01(attackFallback.BonusHitChance);
            }

            return Clamp01(GetSpellBonusHitChance(aSchool));
        }

        public double BonusHitChanceForSchools(HashSet<SpellSchool> aSchools)
        {
            if (useAttackFallback)
            {
                return Clamp01(attackFallback.BonusHitChance);
            }

            SpellSchool[] schools = GetEffectiveSchools(aSchools);
            double sum = 0;
            for (int i = 0; i < schools.Length; i++)
            {
                sum += GetSpellBonusHitChance(schools[i]);
            }

            return Clamp01(sum / schools.Length);
        }

        public SecondayStatBonus<T> GetSecondaryStat<T>(string aSecondaryStat)
        {
            if (useAttackFallback)
            {
                return new SecondayStatBonus<T>(aSecondaryStat, GetFallbackValue<T>(aSecondaryStat));
            }

            foreach (SpellSchool school in Enum.GetValues(typeof(SpellSchool)))
            {
                string prefix = school.ToString();
                if (!aSecondaryStat.StartsWith(prefix, StringComparison.Ordinal))
                {
                    continue;
                }

                string key = aSecondaryStat.Substring(prefix.Length);
                T value = GetValueWithBase<T>(school, key);
                return new SecondayStatBonus<T>(aSecondaryStat, value);
            }

            return new SecondayStatBonus<T>(aSecondaryStat, default);
        }

        public SecondayStatBonus<T> GetSecondaryStat<T>(SpellSchool aSpellSchool, string aSecondaryStat)
        {
            if (useAttackFallback)
            {
                return new SecondayStatBonus<T>(aSecondaryStat, GetFallbackValue<T>(aSecondaryStat));
            }

            T value = GetValueWithBase<T>(aSpellSchool, aSecondaryStat);
            return new SecondayStatBonus<T>(aSecondaryStat, value);
        }

        double GetSpellBonusHitChance(SpellSchool school)
        {
            SpellStats baseStats = GetSpellStats(SpellSchool.Base);
            if (school == SpellSchool.Base)
            {
                return baseStats.BonusHitChance;
            }

            SpellStats schoolStats = GetSpellStats(school);
            return baseStats.BonusHitChance + schoolStats.BonusHitChance;
        }

        T GetValueWithBase<T>(SpellSchool school, string key)
        {
            SpellStats baseStats = GetSpellStats(SpellSchool.Base);
            T baseValue = baseStats.GetValue<T>(key);
            if (school == SpellSchool.Base)
            {
                return baseValue;
            }

            SpellStats schoolStats = GetSpellStats(school);
            T schoolValue = schoolStats.GetValue<T>(key);
            return (T)(object)((dynamic)(object)baseValue + (dynamic)(object)schoolValue);
        }

        SpellStats GetSpellStats(SpellSchool school)
        {
            foreach (SpellStats stats in spellDamage)
            {
                if (stats.SpellSchool == school)
                {
                    return stats;
                }
            }

            return new SpellStats(school);
        }

        static SpellSchool[] GetEffectiveSchools(HashSet<SpellSchool> aSchools)
        {
            if (aSchools == null || aSchools.Count == 0)
            {
                return new[] { SpellSchool.Base };
            }

            SpellSchool[] filtered = aSchools
                .Where(x => x != SpellSchool.Base)
                .Distinct()
                .ToArray();

            if (filtered.Length == 0)
            {
                return new[] { SpellSchool.Base };
            }

            return filtered;
        }

        static bool HasAnyStat(SpellStats aStats)
        {
            return aStats.SpellDamageValue != 0
                || Math.Abs(aStats.CritChance) > double.Epsilon
                || Math.Abs(aStats.CritDamage) > double.Epsilon
                || aStats.FlatPenetration != 0
                || Math.Abs(aStats.PercentPenetration) > double.Epsilon
                || Math.Abs(aStats.Haste) > double.Epsilon
                || Math.Abs(aStats.Vampirism) > double.Epsilon
                || Math.Abs(aStats.BonusHitChance) > double.Epsilon;
        }

        static float Clamp01(float value) => Math.Clamp(value, 0f, 1f);
        static double Clamp01(double value) => Math.Clamp(value, 0d, 1d);

        T GetFallbackValue<T>(string aSecondaryStat)
        {
            if (typeof(T) == typeof(int))
            {
                if (aSecondaryStat.EndsWith("SpellFlatPenetration", StringComparison.Ordinal))
                {
                    return (T)(object)attackFallback.FlatPenetration;
                }

                return (T)(object)0;
            }

            if (typeof(T) == typeof(double))
            {
                if (aSecondaryStat.EndsWith("SpellCritChance", StringComparison.Ordinal))
                {
                    return (T)(object)(double)attackFallback.CriticalChance;
                }

                if (aSecondaryStat.EndsWith("SpellCritDamage", StringComparison.Ordinal))
                {
                    return (T)(object)(double)attackFallback.CriticalDamage;
                }

                if (aSecondaryStat.EndsWith("SpellPercentPenetration", StringComparison.Ordinal))
                {
                    return (T)(object)(double)attackFallback.PercentPenetration;
                }

                if (aSecondaryStat.EndsWith("SpellBonusHitChance", StringComparison.Ordinal))
                {
                    return (T)(object)(double)attackFallback.BonusHitChance;
                }

                return (T)(object)0d;
            }

            return default;
        }
    }
}
