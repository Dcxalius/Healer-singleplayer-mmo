using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.World.GameObjects.Spells.SpellEffects;
using Project_1.World.GameObjects.Unit.Talents;

namespace Project_1.GameObjects.Spells.Buff
{
    internal class AbsorbBuff : Buff
    {
        double remainingAbsorb;
        readonly Spell spell;

        AbsorbEffect AbsorbEffect => effect as AbsorbEffect;

        public double RemainingAbsorb => remainingAbsorb;
        public bool IsDepleted => remainingAbsorb <= 0;

        public override GfxPath GfxPath => AbsorbEffect.GfxPath;
        public override double Duration =>
            (AbsorbEffect.Duration + spell.TalentFlatChange(TalentChange.Duration))
            * (1.0 + spell.TalentPercentChange(TalentChange.Duration));

        public AbsorbBuff(Entity aCaster, AbsorbEffect aEffect, Spell aSpell)
            : base(aCaster, aEffect, aSpell)
        {
            spell = aSpell;
            remainingAbsorb = Power;
        }

        public override void Recast(Buff aBuff)
        {
            base.Recast(aBuff);
            remainingAbsorb = Power; // refresh shield to full on recast
        }

        // Returns how much damage was absorbed. Reduces remainingAbsorb accordingly.
        public double AbsorbDamage(double incoming, DamageType damageType)
        {
            ThreadAffinity.AssertSimThread();
            if (AbsorbEffect.SchoolFilter.HasValue && AbsorbEffect.SchoolFilter.Value != damageType)
                return 0;

            double absorbed = System.Math.Min(remainingAbsorb, incoming);
            remainingAbsorb -= absorbed;
            return absorbed;
        }
    }
}
