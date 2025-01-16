using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.GameObjects.Unit.Stats.BasePrimaryStats;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class PrimaryStats
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
        public void AppendToExistingReport(ref PairReport report)
        {
            for (int i = 0; i < (int)PrimaryStat.Count; i++)
            {
                report.AddLine(stats[i].GetType().Name.ToString(), stats[i].Value);
            }

        }

        public PairReport NewReport
        {
            get
            {
                PairReport report = new PairReport();

                AppendToExistingReport(ref report);

                return report;
            }
        }

        public Strength Strength => stats[(int)PrimaryStat.Strength] as Strength;
        public Agility Agility => stats[(int)PrimaryStat.Agility] as Agility;
        public Intellect Intellect => stats[(int)PrimaryStat.Intellect] as Intellect;
        public Spirit Spirit => stats[(int)PrimaryStat.Spirit] as Spirit;
        public Stamina Stamina => stats[(int)PrimaryStat.Stamina] as Stamina;

        public int[] Stats => new int[] { Strength, Agility, Intellect, Spirit, Stamina };
        protected Stat[] stats;

        public PrimaryStats(int[] aStats)
        {
            stats = new Stat[(int)PrimaryStat.Count];
            SetStats(aStats);
        }

        protected void SetStats(int[] aStats)
        {
            Debug.Assert(stats.Length == aStats.Length);
            stats[(int)PrimaryStat.Strength] = new Strength(aStats[(int)PrimaryStat.Strength]);
            stats[(int)PrimaryStat.Agility] = new Agility(aStats[(int)PrimaryStat.Agility]);
            stats[(int)PrimaryStat.Intellect] = new Intellect(aStats[(int)PrimaryStat.Intellect]);
            stats[(int)PrimaryStat.Spirit] = new Spirit(aStats[(int)PrimaryStat.Spirit]);
            stats[(int)PrimaryStat.Stamina] = new Stamina(aStats[(int)PrimaryStat.Stamina]);
        }
    }
}
