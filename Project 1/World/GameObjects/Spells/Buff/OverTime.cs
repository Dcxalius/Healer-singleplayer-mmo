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

        public int TickCount => tickRate <= 0 ? 1 : Math.Max(1, (int)Math.Floor(duration / tickRate));


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

            Debug.Assert(this.duration > this.tickRate);
        }

        public override bool Trigger(Entity aCaster, Entity aTarget, double aScalar = 1.0)
        {
            ThreadAffinity.AssertSimThread();
            double perTickScalar = 1.0 / TickCount;
            Periodic periodic = new Periodic(aCaster, this, perTickScalar);
            aTarget.AddBuff(periodic);
            return true;
        }

        public bool TriggerRanked(Entity aCaster, Entity aTarget, SpellData spellData, int spellRank)
        {
            ThreadAffinity.AssertSimThread();
            Periodic periodic = new Periodic(aCaster, this, spellData, spellRank);
            aTarget.AddBuff(periodic);
            return true;
        }
    }
}
