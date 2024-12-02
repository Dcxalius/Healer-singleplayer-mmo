using Project_1.GameObjects.Entities;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells
{
    internal class Periodic : Buff
    {
        int tickCounter;

        public override GfxPath GfxPath => overTime.GfxPath;
        protected OverTime overTime;


        public override double Duration => overTime.Duration;

        public Periodic(Entity aCaster, OverTime aOverTime) : base(aCaster)
        {
            tickCounter = 0;
            overTime = aOverTime;
            
        }



        public override void Update(Entity aEntity)
        {
            base.Update(aEntity);
            if (createTime + overTime.TickRate * tickCounter < TimeManager.TotalFrameTime)
            {
                tickCounter++;
                overTime.Effects[Math.Min(overTime.Effects.Length - 1, tickCounter)].Trigger(caster, aEntity);

            }
        }
    }
}
