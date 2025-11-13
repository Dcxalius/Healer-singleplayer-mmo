using Newtonsoft.Json;
using Project_1.GameObjects.Entities;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class SpellResitance
    {
        int baseResitance;
        Dictionary<SpellSchool, int> resitanceBySchool;
        public int BaseResitance => baseResitance;
        public int[] ResitanceBySchool
        {
            get
            {
                int maxEnumValue = Enum.GetValues(typeof(SpellSchool)).Cast<int>().Max();
                int[] result = new int[maxEnumValue + 1];

                foreach (var kvp in resitanceBySchool)
                {
                    result[(int)kvp.Key] = kvp.Value;
                }

                return result;
            }
        }

        public int GetResitance(SpellSchool aSchool) => baseResitance + (resitanceBySchool.TryGetValue(aSchool, out int schoolResitance) ? schoolResitance : 0);
        //TODO: code bellow needs think time, should resitance be treated like this if no ratios are given?
        public int GetResitance(SpellSchool[] aSchools) => baseResitance + aSchools.Select(school => resitanceBySchool.TryGetValue(school, out int schoolResitance) ? (int)(schoolResitance / aSchools.Count()) : 0).Sum();
        public int GetResitance(SpellSchool[] aSchools, double[] aRatio) => (int)(baseResitance + (aSchools.Select((school, index) => (resitanceBySchool.TryGetValue(school, out int schoolResitance) ? schoolResitance : 0) * aRatio[index]).Sum()));
        public SpellResitance(int aBaseResitance, Dictionary<SpellSchool, int> aResitanceBySchool)
        {
            baseResitance = aBaseResitance;
            resitanceBySchool = aResitanceBySchool;
        }
        public SpellResitance(int aBaseResitance) : this(aBaseResitance, new Dictionary<SpellSchool, int>())
        {

        }
        public SpellResitance() : this(0, new Dictionary<SpellSchool, int>())
        {

        }

        [JsonConstructor]
        public SpellResitance(int baseResitance, int[] resitanceBySchool) : this(baseResitance, new Dictionary<SpellSchool, int>())
        {
            for (int i = 0; i < resitanceBySchool.Length; i++)
            {
                this.resitanceBySchool[(SpellSchool)i] = resitanceBySchool[i];
            }
        }

        //public static double CalculateDamageReductionSpellResitance(int spellResitance, int casterLevel)
        //{
        //    double damageReduction;
        //    //effective resistance rating = Rb + max((Lt - Lc) * 5, 0) - min(P, Rb)
        //    //Rb - target base resistance
        //    //Lt - target level
        //    //Lc - caster level
        //    //P - caster spell penetration
        //    //Damage reduction percentage = 100 % *effective resistance rating / (K + effective resistance rating )

        //    //Resistance Score             |50  100 150 200 250
        //    //Chance to Resist 100% Damage |0%  1%	1%	11%	25%
        //    //Chance to Resist 75 % Damage |2%  6%  18% 34% 55%
        //    //Chance to Resist 50 % Damage |11% 24% 48% 40% 16%
        //    //Chance to Resist 25 % Damage |33% 49% 26% 14% 3%
        //    //Chance to Take Full Damage   |54% 20% 7% 1% 1%

        //    if (damageReduction < 0)
        //        damageReduction = 0;

        //    return damageReduction;
        //}
        public static double CalculateDamageReductionSpellResistance(Entity aTarget, Entity aCaster, Spells.Spell aCastSpell)
        {
            // effective resistance rating = Rb + max((Lt - Lc) * 5, 0) - min(P, Rb)
            int levelDifference = aTarget.CurrentLevel - aCaster.CurrentLevel;

            int effectiveResistance = aTarget.SecondaryStats.Defense.SpellResitance.GetResitance(aCastSpell.SpellSchools) + Math.Max(levelDifference * 5, 0) - Math.Min(aCaster.SecondaryStats.Spell.FlatPenetration, );

            if (effectiveResistance <= 0)
                return 0.0;

            // Damage reduction percentage = 100% * effective resistance / (K + effective resistance)
            double damageReduction = 100.0 * effectiveResistance / (k + effectiveResistance);

            // Clamp just in case of odd inputs
            if (damageReduction < 0.0)
                damageReduction = 0.0;
            else if (damageReduction > 100.0)
                damageReduction = 100.0;

            return damageReduction;
        }
    }

}
