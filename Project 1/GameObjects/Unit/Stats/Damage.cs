using Project_1.GameObjects.Doodads;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spawners;
using Project_1.GameObjects.Spells;
using Project_1.Managers;
using Project_1.UI.UIElements.Bars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.GameObjects.Spells.AoE.AreaOfEffectData;

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
            Glancing = 64,
            Crit = 128
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

        public void ApplyGlancingBlowDamage(UnitData aAttackingUnit, Unit.Attack aAttack, MobData aMobData)
        {
            int attackerWeaponSkill = Math.Min(aAttackingUnit.WeaponSkill.GetSkill(aAttack.WeaponType), aAttackingUnit.Level.CurrentLevel * 5);
            int mobDefense = aMobData.Level.CurrentLevel * 5;
            int ratingDifference = mobDefense - attackerWeaponSkill;
            int cappedRatingDifference = Math.Max(0, ratingDifference);

            double lowValue, highValue;

            if (aAttackingUnit.ClassData.IsCaster)
            {
                lowValue = Math.Max(0.01, Math.Min(0.6, 1.3 - 0.05 * ratingDifference - 0.7));
                highValue = Math.Max(0.2, Math.Min(0.99, 1.2 - 0.03 * ratingDifference - 0.3));
            }
            else
            {
                lowValue = Math.Max(0.01, Math.Min(0.91, 1.3 - 0.05 * ratingDifference));
                highValue = Math.Max(0.2, Math.Min(0.99, 1.2 - 0.03 * ratingDifference));
            }

            if (aMobData.Level.CurrentLevel > aAttackingUnit.Level.CurrentLevel)
            {
                double glancingBlowChance = 0.1 + 0.02 * cappedRatingDifference;

                if (RandomManager.RollDouble() < glancingBlowChance)
                {
                    damageAmount *= RandomManager.RollDouble(lowValue, highValue);
                }
            }
        }


        public void CalculateDamageAfterReduction(UnitData aAttacker, UnitData aDefender)
        {
            switch (DamageType)
            {
                case DamageTypes.Physical:
                    damageAmount *= Defense.CalculateDamageReductionArmor(aDefender.Equipment.GetArmor * aAttacker.SecondaryStats.Attack.PercentPenetration - aAttacker.SecondaryStats.Attack.FlatPenetration, aAttacker.Level.CurrentLevel);
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
                    break;
                default:
                    throw new Exception("huh");
            }
        }
        public void CrushingDamage(MobData aMobData, UnitData aUnitData)
        {
            int enemyWeaponSkill = aMobData.Level.CurrentLevel * 5;

            double crushChance = ((enemyWeaponSkill - aUnitData.DefenseSkill) * 2) - 15;

            if (crushChance < 0)
                crushChance = 0;
            else if (crushChance > 100)
                crushChance = 100;

            // AI did this and it seems wrong xdd
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
