using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities.Resources
{
    internal class Energy : Resource
    {
        public override float Value
        {
            get => energyValue;
            protected set
            {
                energyValue = value;
                if (energyValue > maxValue)
                {
                    energyValue = maxValue;
                }
            }
        }
        float energyValue;
        public override float MaxValue { get => maxValue; }
        float maxValue;

        public override float RegenValue { get => regenValue; }
        float regenValue;

        double lastRegenTick;
        double regenTimer = 1000;

        public Energy(float aMaxValue) : base(ResourceType.Energy, Color.Yellow)
        {
            Debug.Assert(aMaxValue > 0, "Tried to set max to 0");
            maxValue = aMaxValue;
            energyValue = aMaxValue;
            regenValue = 20.0f;
        }

        public override void CastSpell(float aCost)
        {
            base.CastSpell(aCost);
        }

        public override void Update()
        {
            if (TimeManager.TotalFrameTime - lastRegenTick < regenTimer)
            {
                lastRegenTick = TimeManager.TotalFrameTime;
                base.Update();
            }
        }
    }
}
