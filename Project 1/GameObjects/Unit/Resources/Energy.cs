﻿using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Resources
{
    internal class Energy : Resource
    {
        public override float Value
        {
            get => energyValue;
            set
            {
                energyValue = value;
                if (energyValue > maxValue)
                {
                    energyValue = maxValue;
                }
            }
        }
        float energyValue;
        public override float MaxValue
        {
            get => maxValue; protected set
            {
                maxValue = BaseMaxValue;
                if (energyValue > maxValue)
                {
                    energyValue = maxValue;
                }
            }
        }
        float maxValue;

        public override float RegenValue { get => regenValue; }
        float regenValue;

        protected override float BaseMaxValue => 100;
        protected override float PerLevel => 0;


        double lastRegenTick;
        double regenTimer = 1000;

        public Energy(float aCurrentResource) : base(ResourceType.Energy, Color.Yellow)
        {
            //Debug.Assert(aMaxValue > 0, "Tried to set max to 0");
            maxValue = 100;
            Value = aCurrentResource;
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

        public override float CalculateMaxValue(PrimaryStats aStats) => maxValue;
    }
}
