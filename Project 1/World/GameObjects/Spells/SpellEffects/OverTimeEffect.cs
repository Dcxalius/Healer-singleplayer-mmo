using Newtonsoft.Json;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Spells.Buff;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.World.GameObjects.Unit.Stats.Secondary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spell = Project_1.GameObjects.Spells.Spell;

namespace Project_1.World.GameObjects.Spells.SpellEffects
{
    internal class OverTimeEffect : LastingEffect
    {

        public double TickRate => tickRate;
        double tickRate;

        public InstantEffect[] Effects => effects;
        InstantEffect[] effects;

        public GfxPath GfxPath => gfxPath;
        GfxPath gfxPath;

        public GfxPath HitGfxPath => hitEffectPath;
        GfxPath hitEffectPath;

        public bool Numerable;


        public int TickCount => (int)Math.Floor(Duration / tickRate);

        public override string Description
        {
            get
            {
                string description = $"Applies the following effects every {tickRate / 1000} seconds for {Duration / 1000} seconds:\n";
                foreach (InstantEffect effect in effects)
                {
                    description += $"- {effect.Description}\n";
                }
                return description;
            }
        }

        public override string GetRankDescription(Spell spell, int spellRank)
        {
            List<string> lines = new List<string>
            {
                $"Applies every {tickRate / 1000:0.##} seconds for {Duration / 1000:0.##} seconds:"
            };

            for (int i = 0; i < effects.Length; i++)
            {
                InstantEffect effect = effects[i];
                if (effect == null) continue;
                lines.Add($"- {effect.GetRankDescriptionAsOverTimeTickWithTotal(spell, spellRank, TickCount)}");
            }

            return string.Join("\n", lines);
        }

        public override double CalculatePower(Spell aSpellData, int aRank)
        {
            if (!Numerable) throw new MissingFieldException();
            double power = 0;
            for (int i = 0; i < effects.Length; i++)
            {
                InstantEffect effect = effects[i];
                if (effect == null) continue;
                power += effect.CalculatePower(aSpellData, aRank);
            }

            return power * TickCount;
        }

        [JsonConstructor]
        public OverTimeEffect(string name, string gfxName, string hitEffectGfx, string[] effectNames, double duration, double tickRate, bool isBinary, HashSet<SpellSchool> spellSchools) : base(duration, name, isBinary, spellSchools)
        {
            this.tickRate = tickRate * 1000;
            effects = new InstantEffect[effectNames.Length];
            for (int i = 0; i < effectNames.Length; i++)
            {
                effects[i] = SpellFactory.GetSpellEffect(effectNames[i]) as InstantEffect;
            }

            gfxPath = new GfxPath(GfxType.SpellImage, gfxName);
            hitEffectPath = new GfxPath(GfxType.Effect, hitEffectGfx);

            Debug.Assert(Duration > TickRate && TickRate > 0);
            Debug.Assert(effects.Contains(null) == false);
        }

        // I think this is deprecated
        //public override bool Trigger(Entity aCaster, Entity aTarget, double aSpellPowerScalar = 1.0)
        //{
        //    ThreadAffinity.AssertSimThread();
        //    double perTickScalar = 1.0 / TickCount;
        //    Periodic periodic = new Periodic(aCaster, this, perTickScalar);
        //    aTarget.AddBuff(periodic);
        //    return true;
        //}

        public override bool Trigger(Entity aCaster, Entity aTarget, Spell aSpell)
        {
            ThreadAffinity.AssertSimThread();
            Periodic periodic = new Periodic(aCaster, this, aSpell);
            aTarget.AddBuff(periodic);
            return true;
        }
    }
}
