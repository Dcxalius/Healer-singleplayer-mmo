using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells
{
    internal class OverTime : SpellEffect
    {
        double createTime;
        double duration;

        double tickRate;
        int tickCounter = 0;

        Instant effect;

        public OverTime(string aName, double aDuration, double aTickRate) : base(aName)
        {
            createTime = TimeManager.TotalFrameTime;
            duration = aDuration;
            
            tickRate = aTickRate;
            Debug.Assert(duration > tickRate);
        }

        public void Update()
        {
            if (createTime + duration < TimeManager.TotalFrameTime)
            {
                //kill me
            }

            if (createTime + tickRate * tickCounter > TimeManager.TotalFrameTime)
            {
                tickCounter++;
                //trigger effect
            }
        }
    }
}
