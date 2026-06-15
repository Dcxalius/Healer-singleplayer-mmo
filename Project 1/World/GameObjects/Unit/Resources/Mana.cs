using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.World.GameObjects.Unit.Stats.Primary;
using Project_1.GameObjects.Unit.Classes;

namespace Project_1.GameObjects.Unit.Resources
{
    internal class Mana : Resource
    {
        static void AssertSimThread() => ThreadAffinity.AssertSimThread();
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

        double lastCastSpellOrTick; //TODO: This should be saved
        double regenTimer = 5000; //TODO: This should be pulled from somewhere

        protected override float PerLevel => 10;

        public Mana(ClassData aClass, TotalPrimaryStats aStats, float aCurrentValue, int aLevel) : base(ResourceType.Mana, Color.Cyan)
        {
            BaseMaxValue = aClass.BaseMana + aClass.PerLevelMana * (aLevel - 1); //TODO: The per level needs to be designed
            
            lastCastSpellOrTick = double.NegativeInfinity; //TODO: Implement loading from file
            CalculateMaxValue(aStats, aClass);
            Value = aCurrentValue;
        }

        public override void Update()
        {
            AssertSimThread();
        }

        public bool CheckIfTicked()
        {
            AssertSimThread();
            if (TimeManager.TotalFrameTime - lastCastSpellOrTick > regenTimer)
            {
                //TODO: Add support to things like talents that give sprit regen per 5 even if spells have been cast.
                //TODO: Add suport for raw mp5. Raw mp5 ticks no matter what.
                //Unsure how to implement that design wise. Should the two timers be synced? If so a small mp5 will possibly lower the average mp5 due to timing windows
                //That might be a bug with the current system
                lastCastSpellOrTick = TimeManager.TotalFrameTime;
                Value += regenValue;
                return true;
            }

            return false;
        }
        public override void TickRegen(bool aInCombat)
        {
            AssertSimThread();
        }

        public override void CastSpell(double aCost)
        {
            AssertSimThread();
            if (aCost < 0) return; //Casting a 0 mana spell doesn't restart the timer.
            lastCastSpellOrTick = TimeManager.TotalFrameTime; //TODO: When spirit per 5 while casting has been implemented, this should not reset the timer any more.

            base.CastSpell(aCost);
        }

        public override void Refresh(TotalPrimaryStats aStats, ClassData aClass)
        {
            AssertSimThread();
            CalculateMaxValue(aStats, aClass);
        }

        void CalculateMaxValue(TotalPrimaryStats aStats, ClassData aClass)
        {
            regenValue = aClass.BaseMp5 + aStats.Spirit.GetMp5Bonus(aClass);
            maxValue = BaseMaxValue + aStats.Intellect * 15;
        }
    }
}
