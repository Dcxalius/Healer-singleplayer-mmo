using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit
{
    internal class BasePrimaryStats
    {
        public enum PrimaryStat
        {
            Strength,
            Agility,
            Intellect,
            Spirit,
            Stamina
        }

        public int Strength { get => strength; }
        protected int strength;
        
        public int Agility { get => agility; }
        protected int agility;
        
        public int Intellect { get => intellect; }
        protected int intellect;
        
        public int Spirit { get => spirit; }
        protected int spirit;

        public int Stamina { get => stamina; }
        protected int stamina;

        public BasePrimaryStats(BasePrimaryStats aBaseLevelOfStats, BasePrimaryStats aPerLevelStats, int aLevel)
        {
            strength = aBaseLevelOfStats.Strength + aBaseLevelOfStats.Strength * aLevel;
            agility = aBaseLevelOfStats.Agility + aBaseLevelOfStats.Agility * aLevel;
            intellect = aBaseLevelOfStats.Intellect + aBaseLevelOfStats.Intellect * aLevel; 
            spirit = aBaseLevelOfStats.Spirit + aBaseLevelOfStats.Spirit * aLevel; 
            stamina = aBaseLevelOfStats.Stamina + aBaseLevelOfStats.Stamina * aLevel;

            Assert();
        }

        public BasePrimaryStats(int[] aStats)
        {
            strength = aStats[(int)PrimaryStat.Strength];
            agility = aStats[(int)PrimaryStat.Agility];
            intellect = aStats[(int)PrimaryStat.Intellect];
            spirit = aStats[(int)PrimaryStat.Spirit];
            stamina = aStats[(int)PrimaryStat.Stamina];

            Assert();
        }

        void Assert()
        {
            Debug.Assert(strength > 0 && agility > 0 && intellect > 0 && spirit > 0 && Stamina > 0, "Primary stat missing.");

        }
    }
}
