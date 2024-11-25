using Project_1.GameObjects.Entities;
using Project_1.Managers;
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

        public Instant[] Effects;
        Instant[] effects;


        public OverTime(string aName, string[] effectNames, double aDuration, double aTickRate) : base(aName)
        {
            duration = aDuration;

            for (int i = 0; i < effectNames.Length; i++)
            {
                effects[i] = SpellFactory.GetSpellEffect(effectNames[i]) as Instant;
            }

            tickRate = aTickRate;
            Debug.Assert(duration > tickRate);
        }

        
    }
}
