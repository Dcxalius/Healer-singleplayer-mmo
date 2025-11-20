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


        public double[] DamageAmount => damageAmount; 
        double[] damageAmount;
        public DamageType[] DamageType => damageType;
        DamageType[] damageType;
        public Damage(double[] aDamageAmount, DamageType[] aDamageType)
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
                for (int i = 0; i < damageAmount.Length; i++)
                {
                    if (damageType[i] == Stats.DamageType.Physical)
                    {
                        damageAmount[i] *= RandomManager.RollDouble(lowValue, highValue);
                        return;
                    }
                }
            }
        }


        public void CalculateDamageAfterReduction(UnitData aAttacker, UnitData aDefender, IDamager aDamager)
        {
            void SRCDAR(ref double aDamage, SpellSchool aSpellSchool)
            {
                aDamage *= SpellResitance.CalculateDamageReductionNonBinary(aDefender, aAttacker, aSpellSchool);
            }
            for (int i = 0; i < damageAmount.Length; i++)
            {
                switch (damageType[i])
                {
                    case Stats.DamageType.Physical:
                        damageAmount[i] *= Defense.CalculateDamageReductionArmor(aDefender.Equipment.GetArmor * aAttacker.SecondaryStats.Attack.PercentPenetration - aAttacker.SecondaryStats.Attack.FlatPenetration, aAttacker.Level.CurrentLevel);
                        break;
                    case Stats.DamageType.Arcane:
                        //How do we want to handle resistances for spells with multiple schools?
                        //How do we want to handle partial resists for binary spells? If wow like not at all
                        //How do we handle spelleffects, do slows and stuff only binary or partial as well?
                        SRCDAR(ref damageAmount[i], SpellSchool.Arcane);
                        break;
                    case Stats.DamageType.Fire:
                        // Implement Fire damage reduction logic here
                        SRCDAR(ref damageAmount[i], SpellSchool.Fire);
                        break;
                    case Stats.DamageType.Frost:
                        // Implement Frost damage reduction logic here
                        SRCDAR(ref damageAmount[i], SpellSchool.Frost);
                        break;
                    case Stats.DamageType.Holy:
                        // Implement Holy damage reduction logic here
                        SRCDAR(ref damageAmount[i], SpellSchool.Holy);
                        break;
                    case Stats.DamageType.Nature:
                        // Implement Nature damage reduction logic here
                        SRCDAR(ref damageAmount[i], SpellSchool.Nature);
                        break;
                    case Stats.DamageType.Shadow:
                        // Implement Shadow damage reduction logic here
                        SRCDAR(ref damageAmount[i], SpellSchool.Shadow);
                        break;
                    case Stats.DamageType.True:
                        break;
                    default:
                        throw new Exception("huh");
                }
            }
            
        }
        public void CrushingDamage(MobData aMobData, UnitData aUnitData)
        {
            if (!damageType.Contains(Stats.DamageType.Physical))
                return;
            int enemyWeaponSkill = aMobData.Level.CurrentLevel * 5;

            double crushChance = ((enemyWeaponSkill - aUnitData.DefenseSkill) * 2) - 15;

            if (crushChance < 0)
                crushChance = 0;
            else if (crushChance > 100)
                crushChance = 100;

            // AI did this and it seems wrong xdd
            for (int i = 0; i < damageType.Length; i++)
            {
                if (damageType[i] == Stats.DamageType.Physical)
                {
                    damageAmount[i] *= (1 + (crushChance / 100));
                    return;
                }
            }
        }
    }

   


    public enum DamageType
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
