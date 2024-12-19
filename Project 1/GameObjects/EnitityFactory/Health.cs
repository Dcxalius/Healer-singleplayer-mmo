using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.EnitityFactory
{
    internal class Health
    {
        public float MaxHealth { get => maxHealth; }

        public float CurrentHealth
        {
            get => currentHealth;
            set
            {
                if (value >= maxHealth)
                {
                    currentHealth = maxHealth;
                    return;
                }
                currentHealth = value;
            }
        }

        float maxHealth;
        float currentHealth;

        float baseMaxHealth;
        
        float healthPer5;


        public Health(float aMaxValue, float aHealthPer5, BasePrimaryStats aPrimaryStats) 
        {
            Debug.Assert(aMaxValue > 0);
            Debug.Assert(aHealthPer5 > 0);
            baseMaxHealth = aMaxValue;
            maxHealth = aMaxValue + aPrimaryStats.Stamina * 10;
            currentHealth = aMaxValue + aPrimaryStats.Stamina * 10;
            healthPer5 = aHealthPer5;
        }

        public void HealthRegenTick() => CurrentHealth += healthPer5;

        public void UpdateStamina(float aStamina)
        {
            maxHealth = baseMaxHealth + aStamina * 10;
            if (currentHealth > maxHealth) currentHealth = maxHealth;
        }
    }
}
