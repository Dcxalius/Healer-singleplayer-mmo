using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spawners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    internal struct Damage
    {
        public double DamageAmount => damageAmount; 
        double damageAmount;
        public DamageTypes DamageType => damageType;
        DamageTypes damageType;
        public Damage(double aDamageAmount, DamageTypes aDamageType)
        {
            damageAmount = aDamageAmount;
            damageType = aDamageType;
        }

        public void ApplyGlancingBlowDamage(MobData aMobData, Entity aAttacker)
        {
            int attackerWeaponSkill = Math.Min(weaponSkill.GetSkill(), aAttacker.CurrentLevel * 5);
            int mobDefense = aMobData.Level.CurrentLevel * 5;
            int ratingDifference = Math.Max(0, mobDefense - attackerWeaponSkill);

            double lowValue, highValue;

            if (aAttacker.ClassData.IsCaster)
            {
                lowValue = Math.Max(0.01, 1.3 - 0.05 * ratingDifference - 0.7);
                highValue = Math.Min(0.99, 1.2 - 0.03 * ratingDifference - 0.3);
            }
            else
            {
                lowValue = Math.Max(0.01, Math.Min(0.91, 1.3 - 0.05 * ratingDifference));
                highValue = Math.Min(0.99, 1.2 - 0.03 * ratingDifference);
            }

            lowValue = Math.Max(lowValue, 0.6);
            highValue = Math.Max(highValue, 0.2);

            double averageReduction = (lowValue + highValue) / 2;

            if (aMobData.Level.CurrentLevel > aAttacker.CurrentLevel)
            {
                double glancingBlowChance = 0.2;

                if (new Random().NextDouble() < glancingBlowChance)
                {
                    damageAmount *= averageReduction;
                }
            }
        }


        public double CalculateDamageAfterReduction(ref Damage aDamage, Entity aAttacker)
        {
            switch (aDamage.DamageType)
            {
                case DamageTypes.Physical:
                    aDamage.damageAmount -= CalculateDamageReduction(armor, aAttacker.CurrentLevel);
                    break;
                case DamageTypes.Arcane:
                    // Implement Arcane damage reduction logic here
                    break;
                case DamageTypes.Fire:
                    // Implement Fire damage reduction logic here
                    break;
                case DamageTypes.Frost:
                    // Implement Frost damage reduction logic here
                    break;
                case DamageTypes.Holy:
                    // Implement Holy damage reduction logic here
                    break;
                case DamageTypes.Nature:
                    // Implement Nature damage reduction logic here
                    break;
                case DamageTypes.Shadow:
                    // Implement Shadow damage reduction logic here
                    break;
                case DamageTypes.True:
                    // Implement True damage reduction logic here
                    break;
                default:
                    break;
            }
            return aDamage.DamageAmount;
        }
        public void ModifyDamage(MobData aMobData, SecondaryStats aStats)
        {
            int enemyWeaponSkill = aMobData.Level.CurrentLevel * 5;

            double crushChance = ((enemyWeaponSkill - defense) * 2) - 15;

            if (crushChance < 0)
                crushChance = 0;
            else if (crushChance > 100)
                crushChance = 100;

            // Modify the damage based on the calculated crush chance
            damageAmount *= (1 + (crushChance / 100));
        }
    }

   


    public enum DamageTypes
    {
        Physical,
        Arcane,
        Fire,
        Frost,
        Holy,
        Nature,
        Shadow,
        True
    }
}
