using Newtonsoft.Json;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells.Buff
{
    internal class OverTime : SpellEffect
    {
        public double Duration { get => duration; }
        double duration;

        public double TickRate { get => tickRate; }
        double tickRate;

        public Instant[] Effects { get => effects; }
        Instant[] effects;

        public GfxPath GfxPath { get => gfxPath; }
        GfxPath gfxPath;

        public GfxPath HitGfxPath { get => hitEffectPath; }
        GfxPath hitEffectPath;

        public bool Numerable;


        public int TickCount => (int)Math.Floor(duration / tickRate);

        public override string Description
        {
            get
            {
                string description = $"Applies the following effects every {tickRate / 1000} seconds for {duration / 1000} seconds:\n";
                foreach (Instant effect in effects)
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
                $"Applies every {tickRate / 1000:0.##} seconds for {duration / 1000:0.##} seconds:"
            };

            for (int i = 0; i < effects.Length; i++)
            {
                Instant effect = effects[i];
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
                Instant effect = effects[i];
                if (effect == null) continue;
                power += effect.CalculatePower(aSpellData, aRank);
            }

            return power * TickCount;
        }

        [JsonConstructor]
        public OverTime(string name, string gfxName, string hitEffectGfx, string[] effectNames, double duration, double tickRate, bool isBinary, HashSet<SpellSchool> spellSchools) : base(name, isBinary, spellSchools)
        {
            this.duration = duration * 1000;
            this.tickRate = tickRate * 1000;
            effects = new Instant[effectNames.Length];
            for (int i = 0; i < effectNames.Length; i++)
            {
                effects[i] = SpellFactory.GetSpellEffect(effectNames[i]) as Instant;
            }

            gfxPath = new GfxPath(GfxType.SpellImage, gfxName);
            hitEffectPath = new GfxPath(GfxType.Effect, hitEffectGfx);

            Debug.Assert(this.duration > this.tickRate && tickRate > 0);
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
