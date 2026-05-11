using Newtonsoft.Json;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Spells.Buff;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.World.GameObjects.Unit.Stats.Secondary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Spell = Project_1.GameObjects.Spells.Spell;

namespace Project_1.World.GameObjects.Spells.SpellEffects
{
    internal class StatusEffect : LastingEffect
    {
        readonly GfxPath gfxPath;
        readonly GfxPath hitGfxPath;
        readonly InstantEffect[] periodicEffects;
        readonly double tickRate;
        readonly (int min, int max)? absorbValueRange;
        readonly DamageType? absorbSchoolFilter;
        readonly StatusModifier[] statModifiers;
        readonly string stackingCategory;
        readonly double movementSpeedModifier;
        readonly bool hasMovementSpeedModifier;
        readonly bool hasControl;
        readonly string[] statusTags;
        readonly float? visualOpacity;
        readonly string[] removeStatusTags;

        public GfxPath GfxPath => gfxPath;
        public GfxPath HitGfxPath => hitGfxPath;
        public InstantEffect[] PeriodicEffects => periodicEffects;
        public double TickRate => tickRate;
        public bool HasPeriodicEffects => periodicEffects.Length > 0 && tickRate > 0;
        public int TickCount => HasPeriodicEffects ? (int)Math.Floor(Duration / tickRate) : 0;
        public bool IsAbsorb => absorbValueRange.HasValue;
        public DamageType? AbsorbSchoolFilter => absorbSchoolFilter;
        public StatusModifier[] StatModifiers => statModifiers;
        public bool HasStatModifiers => statModifiers.Length > 0;
        public string StackingCategory => stackingCategory;
        public bool HasMovementSpeedModifier => hasMovementSpeedModifier;
        public double StatusMovementSpeedModifier => hasMovementSpeedModifier ? movementSpeedModifier : 1d;
        public bool HasControl => hasControl;
        public string[] StatusTags => statusTags;
        public bool HasStatusTag(string tag) => !string.IsNullOrWhiteSpace(tag) && Array.IndexOf(statusTags, tag) >= 0;
        public bool HasVisualOpacity => visualOpacity.HasValue;
        public float VisualOpacity => visualOpacity ?? 1f;
        public string[] RemoveStatusTags => removeStatusTags;

        public override string Description => BuildDescription();

        [JsonConstructor]
        public StatusEffect(
            string name,
            string gfxName,
            string hitEffectGfx,
            string[] effectNames,
            double duration,
            double tickRate,
            int? minValue,
            int? maxValue,
            DamageType? damageType,
            StatusModifier[] statModifiers,
            string stackingCategory,
            string category,
            double? movementSpeedModifier,
            bool? hasControl,
            string[] statusTags,
            float? visualOpacity,
            string[] removeStatusTags,
            bool sourceStackable,
            int maxStackCount,
            bool isBinary,
            HashSet<SpellSchool> spellSchools)
            : base(duration, name, isBinary, spellSchools)
        {
            gfxPath = new GfxPath(GfxType.SpellImage, string.IsNullOrWhiteSpace(gfxName) ? name : gfxName);
            hitGfxPath = new GfxPath(GfxType.Effect, string.IsNullOrWhiteSpace(hitEffectGfx) ? "None" : hitEffectGfx);
            this.tickRate = tickRate * 1000;
            this.statModifiers = statModifiers ?? Array.Empty<StatusModifier>();
            this.stackingCategory = string.IsNullOrWhiteSpace(stackingCategory) ? category : stackingCategory;
            this.movementSpeedModifier = movementSpeedModifier ?? 1d;
            this.hasMovementSpeedModifier = movementSpeedModifier.HasValue;
            this.hasControl = hasControl ?? true;
            this.statusTags = statusTags ?? Array.Empty<string>();
            this.visualOpacity = visualOpacity.HasValue ? Math.Clamp(visualOpacity.Value, 0f, 1f) : null;
            this.removeStatusTags = removeStatusTags ?? Array.Empty<string>();
            this.sourceStackable = sourceStackable;
            MaxStackCount = Math.Max(1, maxStackCount);

            if (effectNames == null || effectNames.Length == 0)
            {
                periodicEffects = Array.Empty<InstantEffect>();
            }
            else
            {
                periodicEffects = new InstantEffect[effectNames.Length];
                for (int i = 0; i < effectNames.Length; i++)
                {
                    periodicEffects[i] = SpellFactory.GetSpellEffect(effectNames[i]) as InstantEffect;
                }

                Debug.Assert(Array.TrueForAll(periodicEffects, x => x != null), "Status periodic effect failed to resolve.");
            }

            if (minValue.HasValue || maxValue.HasValue)
            {
                int min = minValue ?? maxValue.Value;
                int max = maxValue ?? minValue.Value;
                absorbValueRange = (min, max);
                absorbSchoolFilter = damageType;
            }
        }

        public override double CalculatePower(Spell aSpell, int aRank)
        {
            if (IsAbsorb)
            {
                (int min, int max) ranked = ScaleAbsorbValue(aSpell, aRank);
                return (Math.Abs(ranked.min) + Math.Abs(ranked.max)) / 2.0;
            }

            if (HasPeriodicEffects)
            {
                double power = 0;
                for (int i = 0; i < periodicEffects.Length; i++)
                {
                    power += periodicEffects[i].CalculatePower(aSpell, aRank);
                }

                return power * TickCount;
            }

            double statPower = 0;
            for (int i = 0; i < statModifiers.Length; i++)
            {
                statPower += Math.Abs(statModifiers[i].Amount);
            }

            return statPower;
        }

        public (int min, int max) ScaleAbsorbValue(Spell aSpell, int aRank)
        {
            if (!absorbValueRange.HasValue)
            {
                return (0, 0);
            }

            return aSpell.ScaleInstantValueForRankAndTalent(absorbValueRange.Value, aRank);
        }

        public override string GetRankDescription(Spell aSpell, int aRank)
        {
            List<string> lines = new List<string>();

            if (IsAbsorb)
            {
                (int min, int max) ranked = ScaleAbsorbValue(aSpell, aRank);
                string amount = ranked.min == ranked.max ? $"{ranked.min}" : $"{ranked.min} to {ranked.max}";
                string school = absorbSchoolFilter.HasValue ? $" {absorbSchoolFilter.Value}" : string.Empty;
                lines.Add($"Absorbs up to {amount}{school} damage for {Duration / 1000:0.##} seconds.");
            }

            if (HasPeriodicEffects)
            {
                lines.Add($"Applies every {tickRate / 1000:0.##} seconds for {Duration / 1000:0.##} seconds:");
                for (int i = 0; i < periodicEffects.Length; i++)
                {
                    lines.Add($"- {periodicEffects[i].GetRankDescriptionAsOverTimeTickWithTotal(aSpell, aRank, TickCount)}");
                }
            }

            for (int i = 0; i < statModifiers.Length; i++)
            {
                double displayAmount = statModifiers[i].Flat ? Math.Abs(statModifiers[i].Amount) : Math.Abs(statModifiers[i].Amount) * 100d;
                lines.Add($"{(statModifiers[i].Amount >= 0 ? "Increases" : "Reduces")} {statModifiers[i].Stat} by {displayAmount:0.##}{(statModifiers[i].Flat ? string.Empty : "%")}.");
            }

            return string.Join("\n", lines);
        }

        public override bool Trigger(Entity aCaster, Entity aTarget, Spell aSpell)
        {
            ThreadAffinity.AssertSimThread();
            for (int i = 0; i < removeStatusTags.Length; i++)
            {
                aTarget.RemoveStatusBuffsByTag(removeStatusTags[i]);
            }

            if (HasStatusTag("Stealth") && aTarget.HasStatusTag("Stealth"))
            {
                aTarget.RemoveStatusBuffsByTag("Stealth");
                return true;
            }

            aTarget.AddBuff(new StatusBuff(aCaster, this, aSpell));
            return true;
        }

        string BuildDescription()
        {
            if (HasStatModifiers || HasPeriodicEffects || IsAbsorb)
            {
                return "Applies a status effect.";
            }

            return string.Empty;
        }
    }
}
