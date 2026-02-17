using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.GameObjects.Unit.Classes;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Managers;

namespace Project_1.GameObjects.Unit
{
    internal class Health
    {
        static void AssertSimThread() => ThreadAffinity.AssertSimThread();
        const double ServerTickSeconds = 2.0;
        const double Hp5WindowSeconds = 5.0;
        const double Hp5ToTickMultiplier = ServerTickSeconds / Hp5WindowSeconds;

        public double MaxHealth 
        { 
            get => maxHealth;
            private set
            {
                maxHealth = value;

                if (currentHealth > maxHealth)
                {
                    currentHealth = maxHealth;
                }
            }
        }

        public double CurrentHealth
        {
            get => currentHealth;
            set
            {
                if (value > maxHealth)
                {
                    currentHealth = maxHealth;
                    return;
                }
                currentHealth = value;
            }
        }

        double maxHealth;
        double currentHealth;

        double baseMaxHealth;

        public Health(ClassData aClassData, BasePrimaryStats aPrimaryStats, int aLevel, double aCurrentHp) 
        {
            Debug.Assert(aCurrentHp > 0);

            
            baseMaxHealth = aClassData.BaseHealth + aClassData.PerLevelHp * (aLevel - 1);
            maxHealth = baseMaxHealth + aPrimaryStats.Stamina * 10;
            CurrentHealth = aCurrentHp;
        }

        public bool HealthRegenTick(bool aInCombat, double aHp5, double aSpiritHp5)
        {
            AssertSimThread();
            double hp5Total = aHp5 + (aInCombat ? 0d : aSpiritHp5);
            double regen = hp5Total * Hp5ToTickMultiplier;
            if (regen <= 0d)
            {
                return false;
            }

            double previousHealth = CurrentHealth;
            CurrentHealth += regen;
            return CurrentHealth > previousHealth;
        }

        public void UpdateStamina(int aStamina)
        {
            AssertSimThread();
            maxHealth = baseMaxHealth + aStamina * 10;
            if (currentHealth > maxHealth) currentHealth = maxHealth;
        }

        public void Refresh(TotalPrimaryStats aPrimaryStats)
        {
            AssertSimThread();
            MaxHealth = baseMaxHealth + aPrimaryStats.Stamina * 10;
        }

        internal void LevelUp(double aPerLevelHp, int aStamina)
        {
            AssertSimThread();
            baseMaxHealth += aPerLevelHp;
            UpdateStamina(aStamina);
            currentHealth = maxHealth;
        }

        
    }
}
