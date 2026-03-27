using Project_1.GameObjects.Entities;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells.Buff
{
    internal class Periodic : Buff
    {
        int tickCounter;
        readonly double tickScalar;
        Spell spell;
        public override GfxPath GfxPath => OverTime.GfxPath;
        OverTime OverTime { get => effect as OverTime; }

        public override double Duration => OverTime.Duration;


        public Periodic(Entity aCaster, OverTime aOverTime, Spell aSpell) : base(aCaster, aOverTime, aSpell)
        {
            ThreadAffinity.AssertSimThread();
            tickCounter = 0;
            tickScalar = 1.0 / aOverTime.TickCount;
            spell = aSpell;
        }

        public override void Recast(Buff buff)
        {
            ThreadAffinity.AssertSimThread();
            base.Recast(buff);

            tickCounter = 0;
        }

        public override void Update(Entity aEntity)
        {
            ThreadAffinity.AssertSimThread();
            base.Update(aEntity);
            while (tickCounter < OverTime.TickCount && createTime + OverTime.TickRate * (tickCounter + 1) <= TimeManager.TotalFrameTime)
            {
                int effectIndex = Math.Min(OverTime.Effects.Length - 1, tickCounter);
                Instant instant = OverTime.Effects[effectIndex];
                instant.TriggerPeriodic(caster, aEntity, spell, OverTime.TickCount);

                aEntity.AddEffect(new VisualEffect(OverTime.HitGfxPath, 500));
                tickCounter++;
            }
        }
    }
}
