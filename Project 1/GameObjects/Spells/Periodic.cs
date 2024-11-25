using Project_1.GameObjects.Entities;
using Project_1.Managers;
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
        protected OverTime overTime;

        protected override double Duration => overTime.Duration;

        public Periodic(Entity aCaster) : base(aCaster)
        {
            tickCounter = 0;

        }



        public override void Update(Entity aEntity)
        {
            base.Update(aEntity);
            if (createTime + overTime.TickRate * tickCounter > TimeManager.TotalFrameTime)
            {
                tickCounter++;
                overTime.Effects[Math.Min(overTime.Effects.Length - 1, tickCounter)].Trigger(caster, aEntity);

            }
        }
    }
}
