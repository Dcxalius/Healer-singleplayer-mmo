using Newtonsoft.Json;
using Project_1.GameObjects.Entities;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells
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


        [JsonConstructor]
        public OverTime(string name, string gfxName, string[] effectNames, double duration, double tickRate) : base(name)
        {
            this.duration = duration * 1000;
            this.tickRate = tickRate * 1000;
            effects = new Instant[effectNames.Length]; 
            for (int i = 0; i < effectNames.Length; i++)
            {
                effects[i] = SpellFactory.GetSpellEffect(effectNames[i]) as Instant;
            }

            gfxPath = new GfxPath(GfxType.SpellImage, gfxName);
            Debug.Assert(this.duration > this.tickRate);
        }

        public override bool Trigger(Entity aCaster, Entity aTarget)
        {
            Periodic xdd = new Periodic(aCaster, this);
            aTarget.AddBuff(xdd);
            return true;
        }
    }
}
