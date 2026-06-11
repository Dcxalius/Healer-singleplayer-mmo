using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Stats;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.World.GameObjects.Unit.Stats.Primary.BasePrimaryStats;

namespace Project_1.World.GameObjects.Unit.Stats.Primary
{
    internal class PrimaryStats
    {
        public enum PrimaryStat //TODO: This orders is wrong should be a, str, sta, i, sp
        {
            Strength,
            Agility,
            Intellect,
            Spirit,
            Stamina,
            Count
        }
        public void AppendToExistingReport(ref PairReport report) //TODO: Rework this
        {
            for (int i = 0; i < (int)PrimaryStat.Count; i++)
            {
                if (stats[i] == 0) continue;
                report.AddLine(stats[i].GetType().Name.ToString(), stats[i].Value);
            }

        }

        public PairReport NewReport //TODO: Rework this
        {
            get
            {
                PairReport report = new PairReport();

                AppendToExistingReport(ref report);

                return report;
            }
        }

        /// <summary>
        /// Returns the entity's current Strength.
        /// </summary>
        public Strength Strength => stats[(int)PrimaryStat.Strength] as Strength;
        /// <summary>
        /// Returns the entity's current Agility.
        /// </summary>
        public Agility Agility => stats[(int)PrimaryStat.Agility] as Agility;
        /// <summary>
        /// Returns the entity's current Intellect.
        /// </summary>
        public Intellect Intellect => stats[(int)PrimaryStat.Intellect] as Intellect;
        /// <summary>
        /// Returns the entity's current Spirit.
        /// </summary>
        public Spirit Spirit => stats[(int)PrimaryStat.Spirit] as Spirit;
        /// <summary>
        /// Returns the entity's current Stamina.
        /// </summary>
        public Stamina Stamina => stats[(int)PrimaryStat.Stamina] as Stamina;

        /// <summary>
        /// Collapses the current stats into an int array.
        /// </summary>
        public int[] Stats => new int[] { Strength, Agility, Intellect, Spirit, Stamina };
        protected Stat[] stats;

        public PrimaryStats(int[] aStats)
        {
            stats = new Stat[(int)PrimaryStat.Count];
            SetStats(aStats);
        }

        /// <summary>
        /// Overrides the current stats with the given int array.
        /// </summary>
        /// <param name="aStats"></param>
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
