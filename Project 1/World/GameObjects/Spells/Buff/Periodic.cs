using Project_1.GameObjects.Entities;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.World.GameObjects.Unit.Talents;
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

        //TODO: Check if this is the correct way to apply talent changes to periodic effects
        //Biggest concern is if this should be applying the talent changes to each tick or find a way to snapshot at cast of the buff
        public override double Duration => (OverTime.Duration + spell.TalentFlatChange(TalentChange.Duration)) * (1.0 + spell.TalentPercentChange(TalentChange.Duration));


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
