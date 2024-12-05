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

        public override GfxPath GfxPath => OverTime.GfxPath;
        OverTime OverTime { get => effect as OverTime; }

        public override double Duration => OverTime.Duration;

        public Periodic(Entity aCaster, OverTime aOverTime) : base(aCaster, aOverTime)
        {
            tickCounter = 0;
            
        }

        public override void Recast()
        {
            base.Recast();

            tickCounter = 0;
        }

        public override void Update(Entity aEntity)
        {
            base.Update(aEntity);
            if (createTime + OverTime.TickRate * tickCounter < TimeManager.TotalFrameTime)
            {
                tickCounter++;
                OverTime.Effects[Math.Min(OverTime.Effects.Length - 1, tickCounter)].Trigger(caster, aEntity); //TODO: Change this so it doesn't instaproc  

            }
        }
    }
}
