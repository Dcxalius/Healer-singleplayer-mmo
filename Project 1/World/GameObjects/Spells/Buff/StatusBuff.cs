using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.World.GameObjects.Spells.SpellEffects;
using Project_1.World.GameObjects.Unit.Talents;
using System;

namespace Project_1.GameObjects.Spells.Buff
{
    internal class StatusBuff : Buff
    {
        int tickCounter;
        double remainingAbsorb;
        readonly Spell spell;

        StatusEffect Status => effect as StatusEffect;

        public override GfxPath GfxPath => Status.GfxPath;
        public override double Duration =>
            (Status.Duration + spell.TalentFlatChange(TalentChange.Duration))
            * (1.0 + spell.TalentPercentChange(TalentChange.Duration));

        public override bool IsDepleted => Status.IsAbsorb && remainingAbsorb <= 0;

        public StatusBuff(Entity aCaster, StatusEffect aEffect, Spell aSpell)
            : base(aCaster, aEffect, aSpell)
        {
            spell = aSpell;
            remainingAbsorb = Power;
        }

        public override void Recast(Buff aBuff)
        {
            base.Recast(aBuff);
            tickCounter = 0;
            if (Status.IsAbsorb)
            {
                remainingAbsorb = Power;
            }
        }

        public override void Update(Entity aEntity)
        {
            ThreadAffinity.AssertSimThread();
            base.Update(aEntity);
            if (!Status.HasPeriodicEffects)
            {
                return;
            }

            while (tickCounter < Status.TickCount && createTime + Status.TickRate * (tickCounter + 1) <= TimeManager.TotalFrameTime)
            {
                int effectIndex = Math.Min(Status.PeriodicEffects.Length - 1, tickCounter);
                Status.PeriodicEffects[effectIndex].TriggerPeriodic(caster, aEntity, spell, Status.TickCount);
                aEntity.AddEffect(new VisualEffect(Status.HitGfxPath, 500));
                tickCounter++;
            }
        }

        public override double AbsorbDamage(double incoming, DamageType damageType)
        {
            ThreadAffinity.AssertSimThread();
            if (!Status.IsAbsorb)
            {
                return 0;
            }

            if (Status.AbsorbSchoolFilter.HasValue && Status.AbsorbSchoolFilter.Value != damageType)
            {
                return 0;
            }

            double absorbed = Math.Min(remainingAbsorb, incoming);
            remainingAbsorb -= absorbed;
            return absorbed;
        }
    }
}
