using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit
{
    internal class Level
    {
        public int CurrentLevel => level;
        int level;

        public int Experience => experience;
        int experience;

        static int[] experienceToLevel = 
            { 400, 900, 1400, 2800, 3600, 4500, 5400, 6500, 7600, 
            8800, 10100, 11400, 12900, 14400, 16000, 17700, 19400, 21300, 23200, 
            25200, 27300, 29400, 31700, 34000, 36400, 38900, 41400, 44300, 47400, 
            50800, 54500, 58600, 62800, 67100, 71600, 76100, 80800, 85700, 90700, 
            95800, 101000, 106300, 111800, 117500, 123200, 129100, 135100, 141200, 147500, 
            153900, 160400, 167100, 173900, 180800, 187900, 195000, 202300, 209800, 217400 };

        public Level(int aLevel, int aExperience)
        {
            level = aLevel;
            experience = aExperience;
            Debug.Assert(level > 0);
        }

        public bool GainExp(int aExpAmount)
        {
            if (level >= 60) return false;
            experience += aExpAmount;

            if (experience > experienceToLevel[level])
            {
                experience -= experienceToLevel[level];
                level++;
                return true;
            }
            return false;
        }

        static public int ZD(int aLevelOfMob)
        {
            if (aLevelOfMob <= 7) return 5;
            if (aLevelOfMob <= 9) return 6;
            if (aLevelOfMob <= 11) return 7;
            if (aLevelOfMob <= 15) return 8;
            if (aLevelOfMob <= 19) return 9;
            if (aLevelOfMob <= 29) return 11;
            if (aLevelOfMob <= 39) return 12;
            if (aLevelOfMob <= 44) return 13;
            if (aLevelOfMob <= 49) return 14;
            if (aLevelOfMob <= 54) return 15;
            if (aLevelOfMob <= 59) return 16;

            throw new Exception("The Level given should not be possible.");
        }


        public int ExpReward(int aLevelOfKiller)
        {
            if (aLevelOfKiller >= 60) return 0;

            int levelOfMob = CurrentLevel;
            int xp = levelOfMob * 5 + 45;
            if (levelOfMob > aLevelOfKiller)
            {
                xp *= (int)(1 + 0.05 * Math.Min(levelOfMob - aLevelOfKiller, 4));
            }
            else if (levelOfMob < aLevelOfKiller)
            {
                xp *= 1 - (aLevelOfKiller - levelOfMob) / ZD(levelOfMob);
            }

            //TODO: Calculate reduction for group

            return xp;
        }
    }
}
