using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.GameObjects.Unit.Stats;

namespace Project_1.GameObjects.Unit
{
    internal class Health
    {
        public float MaxHealth 
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

        public float CurrentHealth
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

        float maxHealth;
        float currentHealth;

        float baseMaxHealth;

        float baseHealthPer5;
        float healthPer5;

        public Health(ClassData aClassData, BasePrimaryStats aPrimaryStats, int aLevel, float aCurrentHp) 
        {
            Debug.Assert(aCurrentHp > 0);

            
            baseMaxHealth = aClassData.BaseHealth + aClassData.PerLevelHp * (aLevel - 1);
            maxHealth = baseMaxHealth + aPrimaryStats.Stamina * 10;
            CurrentHealth = aCurrentHp;
            baseHealthPer5 = aClassData.HpPer5;
            healthPer5 = aClassData.HpPer5 + aPrimaryStats.Spirit;
        }

        public void HealthRegenTick() => CurrentHealth += healthPer5;

        public void UpdateStamina(int aStamina)
        {
            maxHealth = baseMaxHealth + aStamina * 10;
            if (currentHealth > maxHealth) currentHealth = maxHealth;
        }

        public void Refresh(TotalPrimaryStats aPrimaryStats)
        {
            MaxHealth = baseMaxHealth + aPrimaryStats.Stamina * 10;
            healthPer5 = baseHealthPer5 + aPrimaryStats.Spirit;
        }

        internal void LevelUp(float aPerLevelHp, int aStamina)
        {
            baseMaxHealth += aPerLevelHp;
            UpdateStamina(aStamina);
            currentHealth = maxHealth;
        }

        
    }
}
