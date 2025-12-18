using Project_1.Items.SubTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.GameObjects.Spells.AoE.AreaOfEffectData;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class Spell
    {
        public Spell(UnitData unitData)
        {
            spellDamage = new HashSet<SpellStats>();
            Refresh(unitData);
        }

        HashSet<SpellStats> spellDamage;


        public void Refresh(UnitData unitData)
        {
            (string, Type)[] secondaryStats = SpellStats.SecondaryStatsAsStrings;
            spellDamage.Clear();
            foreach (SpellSchool school in Enum.GetValues(typeof(SpellSchool)))
            {
                var stats = SpellStats.EmptyStats;
                int intStats = 0;
                int doubleStats = 0;
                for (int i = 0; i < secondaryStats.Length; i++)
                {
                    var statName = school.ToString() + secondaryStats[i].Item1;

                    if (secondaryStats[i].Item2 == typeof(int))
                    {
                        stats.Item1[intStats++] += unitData.Equipment.GetSecondaryStat<int>(statName);
                    }
                    else if (secondaryStats[i].Item2 == typeof(double))
                    {
                        stats.Item2[doubleStats++] += unitData.Equipment.GetSecondaryStat<double>(statName);
                    }
                }

                if (stats == SpellStats.EmptyStats)
                    continue;

                spellDamage.Add(new SpellStats(school, unitData));
            }

            if (spellDamage.Where(x => x.SpellSchool == SpellSchool.Base).Count() == 0)
            {
                spellDamage.Add(new SpellStats(SpellSchool.Base));
            }
        }

        public SecondayStatBonus<T> GetSecondaryStat<T>(string aSecondaryStat)
        {
            string[] strings = Enum.GetNames(typeof(SpellSchool));

            for (int i = 1; i < strings.Length; i++)
            {
                if (!aSecondaryStat.StartsWith(strings[i])) continue;
                if (!Enum.TryParse<SpellSchool>(strings[i], out SpellSchool school)) continue;
                var actual = spellDamage.Single(x => x.SpellSchool == school);
                var baseS = spellDamage.Single(x => x.SpellSchool == SpellSchool.Base);
                T value = default;
                value = baseS.GetValue<T>(aSecondaryStat);
                value = (T)(object)(value is null ? 0 : (dynamic)(object)value + (dynamic)(object)actual.GetValue<T>(aSecondaryStat));

                return new SecondayStatBonus<T>(aSecondaryStat, value);
            }
            return new SecondayStatBonus<T>(aSecondaryStat, spellDamage.Single(x => x.SpellSchool == SpellSchool.Base).GetValue<T>(aSecondaryStat));
        }

        public SecondayStatBonus<T> GetSecondaryStat<T>(SpellSchool aSpellSchool, string aSecondaryStat)
        {
            SecondayStatBonus<T> returnable = default;

            returnable = new SecondayStatBonus<T>(aSecondaryStat, spellDamage.Single(x => x.SpellSchool == SpellSchool.Base).GetValue<T>(aSecondaryStat));

            if (spellDamage.Where(x => x.SpellSchool == aSpellSchool).Count() > 0 || aSpellSchool == SpellSchool.Base) return returnable;

            var stats = SpellStats.SecondaryStatsAsStrings;
            var spellStat = spellDamage.Single(x => x.SpellSchool == aSpellSchool);

            returnable = returnable + spellDamage.Single(x => x.SpellSchool == aSpellSchool).GetValue<T>(aSecondaryStat);

            
            
            return returnable;

        }

        public double BonusHitChance(SpellSchool school)
        {
            double returnable = 0;
            if (spellDamage.Where(x => x.SpellSchool == school).Count() > 0)
                returnable += spellDamage.Single(x => x.SpellSchool == SpellSchool.Base).BonusHitChance;
            returnable += spellDamage.Single(x => x.SpellSchool == school).BonusHitChance;
            
            return returnable;
        }


    }
}
