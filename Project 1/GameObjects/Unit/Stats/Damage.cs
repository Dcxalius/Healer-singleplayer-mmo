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
        [Flags]
        public enum Result
        {
            Miss = 0,
            Hit = 1,
            Dodge = 2,
            Parry = 4,
            Block = 8,
            Resist = 16,
            Crushing = 32,
            Crit = 64
        }


        public double DamageAmount => damageAmount; 
        double damageAmount;
        public DamageTypes DamageType => damageType;
        DamageTypes damageType;
        public Damage(double aDamageAmount, DamageTypes aDamageType)
        {
            damageAmount = aDamageAmount;
            damageType = aDamageType;
        }

        public void ApplyGlancingBlowDamage(MobData aMobData, UnitData aAttackingUnit, Unit.Attack aAttack)
        {
            int attackerWeaponSkill = Math.Min(aAttackingUnit.WeaponSkill.GetSkill(aAttack.WeaponType), aAttackingUnit.Level.CurrentLevel * 5);
            int mobDefense = aMobData.Level.CurrentLevel * 5;
            int ratingDifference = Math.Max(0, mobDefense - attackerWeaponSkill);

            double lowValue, highValue;

            if (aAttackingUnit.ClassData.IsCaster)
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

            if (aMobData.Level.CurrentLevel > aAttackingUnit.Level.CurrentLevel)
            {
                double glancingBlowChance = 0.2;

                if (new Random().NextDouble() < glancingBlowChance)
                {
                    damageAmount *= averageReduction;
                }
            }
        }


        public double CalculateDamageAfterReduction(ref Damage aDamage, UnitData aAttacker, UnitData aDefender)
        {
            switch (aDamage.DamageType)
            {
                case DamageTypes.Physical:
                    return aDamage.damageAmount -= Defense.CalculateDamageReductionArmor(aDefender.Equipment.GetArmor, aAttacker.Level.CurrentLevel);
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
                    return aDamage.DamageAmount;
                default:
                    throw new Exception("huh");
            }
            return aDamage.DamageAmount;
        }
        public void CrushingDamage(MobData aMobData, UnitData aUnitData)
        {
            int enemyWeaponSkill = aMobData.Level.CurrentLevel * 5;

            double crushChance = ((enemyWeaponSkill - aUnitData.DefenseSkill) * 2) - 15;

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
