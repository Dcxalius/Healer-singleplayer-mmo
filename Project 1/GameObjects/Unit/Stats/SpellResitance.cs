using Newtonsoft.Json;
using Project_1.GameObjects.Entities;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public static double CalculateDamageReductionNonBinary(UnitData aTarget, UnitData aCaster, SpellSchool aResist)
        {
            // effective resistance rating = Rb + max((Lt - Lc) * 5, 0) - min(P, Rb)
            int levelDifference = aTarget.Level.CurrentLevel - aCaster.Level.CurrentLevel;
            int cap = aTarget.Level.CurrentLevel * 5;

            int flatPenetration = Math.Min(aCaster.SecondaryStats.Spell.FlatPenetration, aTarget.SecondaryStats.Defense.SpellResitance.GetResitance(aResist));
            int percentPenetration = (int)(aTarget.SecondaryStats.Defense.SpellResitance.GetResitance(aResist) * aCaster.SecondaryStats.Spell.PercentPenetration);

            int effectiveResistance = aTarget.SecondaryStats.Defense.SpellResitance.GetResitance(aResist) + Math.Max(levelDifference * 5, 0) - flatPenetration - percentPenetration;
            if (aTarget.UnitType >= UnitType.Normal && levelDifference > 0)
            {
                effectiveResistance += (2 / 15 * aCaster.Level.CurrentLevel * levelDifference);
            }

            if (effectiveResistance <= 0)
                return 0.0;
            //Resist stregth will always be one of these, 0%, 25%, 50%, 75%
            //The chance for each one increases linearly within each third of the cap
            //Resist % of cap   | 0%    | 25%   | 50%   | 75%   |Avg Resist
            //0.0 %             | 100%  | 0%    | 0%    | 0%    | 0.00 %
            //33.3 %            | 24%   | 55%   | 18%   | 3%    | 25.00 %
            //Note: chance of 0 % resist appears to be 1 % at just under 2 / 3, 0 % at 2 / 3 and above
            //66.7 %            | 0%    | 22%   | 56%   | 22%   | 50.00 %
            //100.0 %           | 0%    | 4%    | 16%   | 80%   | 69.00 %

            double resistanceRatio = cap > 0 ? Math.Clamp((double)effectiveResistance / cap, 0.0, 1.0) : 0.0;
            double[] anchors = new[] { 0.0, 1.0 / 3.0, 2.0 / 3.0, 1.0 };

            double Interpolate(double[] values)
            {
                for (int i = 0; i < anchors.Length - 1; i++)
                {
                    if (resistanceRatio <= anchors[i + 1])
                    {
                        double t = (resistanceRatio - anchors[i]) / (anchors[i + 1] - anchors[i]);
                        return values[i] + (values[i + 1] - values[i]) * t;
                    }
                }
                return values[^1];
            }

            // Probabilities for 0%, 25%, 50%, 75% damage reduction at the anchor points.
            double[] chance0 = new[] { 1.0, 0.24, 0.0, 0.0 };
            double[] chance25 = new[] { 0.0, 0.55, 0.22, 0.04 };
            double[] chance50 = new[] { 0.0, 0.18, 0.56, 0.16 };
            double[] chance75 = new[] { 0.0, 0.03, 0.22, 0.80 };

            double[] probabilities =
            {
                Interpolate(chance0),
                Interpolate(chance25),
                Interpolate(chance50),
                Interpolate(chance75)
            };

            double probabilitySum = probabilities.Sum();
            if (probabilitySum > 0)
            {
                for (int i = 0; i < probabilities.Length; i++)
                {
                    probabilities[i] /= probabilitySum;
                }
            }

            double roll = RandomManager.RollDouble();
            double cumulative = 0.0;
            double[] reductionValues = { 0.0, 0.25, 0.5, 0.75 };
            double damageReduction = reductionValues[^1];

            for (int i = 0; i < probabilities.Length; i++)
            {
                cumulative += probabilities[i];
                if (roll <= cumulative)
                {
                    return reductionValues[i];
                }
            }

            throw new Exception("How did you get here????");
        }
    }

}
