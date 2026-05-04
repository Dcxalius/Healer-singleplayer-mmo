using Newtonsoft.Json;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Spells.Buff;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.World.GameObjects.Unit.Stats.Secondary;
using System.Collections.Generic;
using Spell = Project_1.GameObjects.Spells.Spell;

namespace Project_1.World.GameObjects.Spells.SpellEffects
{
    internal class AbsorbEffect : LastingEffect
    {
        readonly (int min, int max) valueRange;
        readonly GfxPath gfxPath;


        // Null means absorb all damage schools. Set to a specific type for ward-style absorbs.
        //TODO: Ponder if set to true should be all instead of null
        public DamageType? SchoolFilter => schoolFilter;
        readonly DamageType? schoolFilter;

        public GfxPath GfxPath => gfxPath;

        [JsonConstructor]
        public AbsorbEffect(
            string name,
            string gfxName,
            int minValue,
            int maxValue,
            double duration,
            DamageType? damageType,
            bool isBinary,
            HashSet<SpellSchool> spellSchools)
            : base(duration, name, isBinary, spellSchools)
        {
            valueRange = (minValue, maxValue);
            schoolFilter = damageType;
            gfxPath = new GfxPath(GfxType.SpellImage, gfxName ?? name);
        }

        public override double CalculatePower(Spell aSpell, int aRank)
        {
            (int min, int max) ranked = aSpell.ScaleInstantValueForRankAndTalent(valueRange, aRank);
            return (ranked.min + ranked.max) / 2.0;
        }

        public override string GetRankDescription(Spell aSpell, int aRank)
        {
            (int min, int max) ranked = aSpell.ScaleInstantValueForRankAndTalent(valueRange, aRank);
            string schoolText = schoolFilter.HasValue ? $" {schoolFilter.Value}" : string.Empty;
            string amount = ranked.min == ranked.max
                ? $"{ranked.min}"
                : $"{ranked.min} to {ranked.max}";
            return $"Absorbs up to {amount}{schoolText} damage for {Duration / 1000:0.##} seconds.";
        }

        public override bool Trigger(Entity aCaster, Entity aTarget, Spell aSpell)
        {
            ThreadAffinity.AssertSimThread();
            aTarget.AddBuff(new AbsorbBuff(aCaster, this, aSpell));
            return true;
        }
    }
}
