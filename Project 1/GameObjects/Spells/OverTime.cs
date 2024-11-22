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
        double createTime;
        double duration;

        double tickRate;
        int tickCounter = 0;

        Instant[] effect;

        bool isOver;

        public OverTime(string aName, string[] effectNames, double aDuration, double aTickRate) : base(aName)
        {
            isOver = false;
            duration = aDuration;

            for (int i = 0; i < effectNames.Length; i++)
            {
                effect[i] = SpellFactory.GetSpellEffect(effectNames[i]) as Instant;
            }

            tickRate = aTickRate;
            Debug.Assert(duration > tickRate);
        }

        public void Update(Entity aEntity)
        {
            if (createTime + duration < TimeManager.TotalFrameTime)
            {
                isOver = true;
                return;
            }

            if (createTime + tickRate * tickCounter > TimeManager.TotalFrameTime)
            {
                tickCounter++;
                if (effect.Length == 1)
                {
                    effect[0].Trigger(aEntity);
                }
                else
                {
                    effect[Math.Min(effect.Length, tickCounter)].Trigger(aEntity);
                }
            }
        }

        public override bool Trigger(Entity aCaster, Entity aTarget)
        {
            createTime = TimeManager.TotalFrameTime;
            caster = aCaster;
        }
    }
}
