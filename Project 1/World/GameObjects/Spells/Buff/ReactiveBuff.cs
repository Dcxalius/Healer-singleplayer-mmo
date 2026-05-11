using Project_1.GameObjects.Entities;
using Project_1.Textures;
using Project_1.World.GameObjects.Spells.SpellEffects;
using Project_1.World.GameObjects.Unit.Stats.Secondary;
using System.Collections.Generic;

namespace Project_1.GameObjects.Spells.Buff
{
    // Applied on the entity that dodged or parried. Signals that Riposte (or any
    // AfterDodgeOrParry spell) is available. Refreshes its duration on repeated
    // dodge/parry within the window, and is consumed immediately when the spell fires.
    internal class ReactiveBuff : Buff
    {
        const double windowDurationMs = 5000;

        // Lazy singleton — always created on the sim thread so AssertSimThread() passes.
        static ReactiveSpellEffect reactiveEffect;
        static ReactiveSpellEffect GetEffect() => reactiveEffect ??= new ReactiveSpellEffect();

        public override double Duration => windowDurationMs;
        public override GfxPath GfxPath => new GfxPath(GfxType.SpellImage, "Riposte");

        public ReactiveBuff(Entity aCaster) : base(aCaster, GetEffect()) { }
    }

    // Sentinel SpellEffect used only to give ReactiveBuff a stable identity in BuffList.
    // It never triggers damage or healing.
    internal sealed class ReactiveSpellEffect : SpellEffect
    {
        public ReactiveSpellEffect()
            : base("Riposte Ready", false, new HashSet<SpellSchool>()) { }

        public override double CalculatePower(Spell aSpell, int aRank) => 0;
        public override string GetRankDescription(Spell aSpell, int aRank) => "Ready to Riposte.";
        public override bool Trigger(Entity aCaster, Entity aTarget, Spell aSpell) => false;
    }
}
