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
    internal class Mana : Resource
    {
        public override float Value
        {
            get => manaValue;
            set
            {
                manaValue = value;
                if (manaValue > maxValue)
                {
                    manaValue = maxValue;
                }
            }
        }
        float manaValue;
        public override float MaxValue { get => maxValue; }
        float maxValue;

        public override float RegenValue { get => regenValue; }
        float regenValue;

        double lastCastSpellOrTick;
        double regenTimer = 5000;

        public Mana(float aMaxValue, float aRegenValue) : base(ResourceType.Mana, Color.Cyan)
        {
            Debug.Assert(aMaxValue > 0, "Tried to set max to 0");
            Debug.Assert(aRegenValue > 0, "Tried to set regen to 0");
            lastCastSpellOrTick = double.NegativeInfinity;
            manaValue = aMaxValue;
            maxValue = aMaxValue;
            regenValue = aRegenValue;
        }

        public override void Update()
        {
            if (TimeManager.TotalFrameTime - lastCastSpellOrTick > regenTimer)
            {
                lastCastSpellOrTick = TimeManager.TotalFrameTime;
                base.Update();
            }
        }

        public override void CastSpell(float aCost)
        {
            lastCastSpellOrTick = TimeManager.TotalFrameTime;

            base.CastSpell(aCost);
        }
    }
}
