using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.GameObjects.Unit.Stats.BasePrimaryStats;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class BasePrimaryStats : PrimaryStats
    {



        public BasePrimaryStats(PrimaryStats aBaseLevelOfStats, PrimaryStats aPerLevelStats, int aLevel) 
            : base(new int[] 
            { 
                aBaseLevelOfStats.Strength + aPerLevelStats.Strength * aLevel, 
                aBaseLevelOfStats.Agility + aPerLevelStats.Agility * aLevel,
                aBaseLevelOfStats.Intellect + aPerLevelStats.Intellect * aLevel,
                aBaseLevelOfStats.Spirit + aPerLevelStats.Spirit * aLevel,
                aBaseLevelOfStats.Stamina + aPerLevelStats.Stamina * aLevel
            }) { }


        public void LevelUp(PrimaryStats aPerLevel)
        {
            for (int i = 0; i < stats.Length; i++)
            {
                stats[i].Value += aPerLevel.Stats[i];
            }
        }
    }
}
