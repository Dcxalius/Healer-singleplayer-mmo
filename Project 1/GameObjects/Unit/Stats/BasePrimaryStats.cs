using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class BasePrimaryStats
    {
        public enum PrimaryStat
        {
            Strength,
            Agility,
            Intellect,
            Spirit,
            Stamina,
            Count
        }

        internal void StatReport(ref Report report)
        {
            for (int i = 0; i < (int)PrimaryStat.Count; i++)
            {
                report.AddLine(stats[i].ToString());
            }

        }

        public Strength Strength => stats[(int)PrimaryStat.Strength] as Strength;
        public Agility Agility => stats[(int)PrimaryStat.Agility] as Agility;
        public Intellect Intellect => stats[(int)PrimaryStat.Intellect] as Intellect;
        public Spirit Spirit => stats[(int)PrimaryStat.Spirit] as Spirit;
        public Stamina Stamina => stats[(int)PrimaryStat.Stamina] as Stamina;


        protected Stat[] stats;

        public BasePrimaryStats(BasePrimaryStats aBaseLevelOfStats, BasePrimaryStats aPerLevelStats, int aLevel)
        {
            stats = new Stat[(int)PrimaryStat.Count];
            stats[(int)PrimaryStat.Strength] = new Strength(aBaseLevelOfStats.Strength + aPerLevelStats.Strength * aLevel);
            stats[(int)PrimaryStat.Agility] = new Agility(aBaseLevelOfStats.Agility + aPerLevelStats.Agility * aLevel);
            stats[(int)PrimaryStat.Intellect] = new Intellect(aBaseLevelOfStats.Intellect + aPerLevelStats.Intellect * aLevel);
            stats[(int)PrimaryStat.Spirit] = new Spirit(aBaseLevelOfStats.Spirit + aPerLevelStats.Spirit * aLevel);
            stats[(int)PrimaryStat.Stamina] = new Stamina(aBaseLevelOfStats.Stamina + aPerLevelStats.Stamina * aLevel);
        }

        public BasePrimaryStats(int[] aStats)
        {
            stats = new Stat[(int)PrimaryStat.Count];
            stats[(int)PrimaryStat.Strength] = new Strength(aStats[(int)PrimaryStat.Strength]);
            stats[(int)PrimaryStat.Agility] = new Agility(aStats[(int)PrimaryStat.Agility]);
            stats[(int)PrimaryStat.Intellect] = new Intellect(aStats[(int)PrimaryStat.Intellect]);
            stats[(int)PrimaryStat.Spirit] = new Spirit(aStats[(int)PrimaryStat.Spirit]);
            stats[(int)PrimaryStat.Stamina] = new Stamina(aStats[(int)PrimaryStat.Stamina]);
        }

        public void LevelUp(BasePrimaryStats aPerLevel)
        {
            for (int i = 0; i < stats.Length; i++)
            {
                stats[i].Value += aPerLevel.stats[i].Value;
            }
        }
    }
}
