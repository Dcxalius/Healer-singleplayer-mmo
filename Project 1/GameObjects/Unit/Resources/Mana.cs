using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.GameObjects.Unit.Stats;

namespace Project_1.GameObjects.Unit.Resources
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
        public override float MaxValue
        {
            get => maxValue;
            protected set
            {
                maxValue = value;
                if (manaValue > maxValue)
                {
                    manaValue = maxValue;
                }
            }
        }
        float maxValue;

        public override float RegenValue { get => regenValue; }
        float regenValue;

        double lastCastSpellOrTick;
        double regenTimer = 5000;

        protected override float PerLevel => 10;

        public Mana(float aBaseValue, TotalPrimaryStats aStats, float aCurrentValue, float aBaseRegen, int aLevel) : base(ResourceType.Mana, Color.Cyan)
        {
            Debug.Assert(aBaseValue > 0, "Tried to set base to 0");
            Debug.Assert(aBaseRegen > 0, "Tried to set regen to 0");

            BaseMaxValue = aBaseValue + PerLevel * (aLevel - 1);
            PerLevel = 10;
            lastCastSpellOrTick = double.NegativeInfinity;
            maxValue = CalculateMaxValue(aStats) ;
            regenValue = aBaseRegen + aStats.Spirit ;
            Value = aCurrentValue;
        }

        public override void Update()
        {
            
        }

        public bool CheckIfTicked()
        {
            if (TimeManager.TotalFrameTime - lastCastSpellOrTick > regenTimer)
            {
                lastCastSpellOrTick = TimeManager.TotalFrameTime;
                base.Update(); //TODO: ????????
                return true;
            }

            return false;
        }

        public override void CastSpell(float aCost)
        {
            lastCastSpellOrTick = TimeManager.TotalFrameTime;

            base.CastSpell(aCost);
        }

        public override float CalculateMaxValue(TotalPrimaryStats aStats) => BaseMaxValue + aStats.Intellect * 15;
    }
}
