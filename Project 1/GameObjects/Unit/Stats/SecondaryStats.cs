using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spawners;
using Project_1.Items.SubTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class SecondaryStats
    {
       

        public bool CalculateDodge() => new Random().NextDouble() < dodgeChance;
        public bool CalculateParry() => new Random().NextDouble() < parryChance;
        public bool CalculateBlock() => new Random().NextDouble() < blockChance;
        public Damage CalculateBlockReduction(Damage aDamage)
        {
            if (CalculateBlock())
            {
                double reducedAmount = aDamage.DamageAmount - blockValue;
                return new Damage(reducedAmount < 0 ? 0 : reducedAmount, aDamage.DamageType);
            }
            else return aDamage;
        }
        public double CalculateEHP(double playerHealth, double armorDamageReduction)
        {
            if (armorDamageReduction < 0 || armorDamageReduction >= 1)
                throw new ArgumentOutOfRangeException(nameof(armorDamageReduction), "Damage reduction must be between 0 and 1 (exclusive).");

            return playerHealth / (1 - armorDamageReduction);
        }

        public double CalculateDamageReduction(double armor, int attackerLevel)
        {
            double damageReduction;

            if (attackerLevel < 60)
            {
                damageReduction = armor / (armor + 400 + 85 * attackerLevel);
            }
            else
            {
                damageReduction = armor / (armor + 400 + 85 * (attackerLevel + 4.5 * (attackerLevel - 59)));
            }

            return damageReduction;
        }

        double attackCriticalChance;
        double attackCriticalDamage;
        double perArmorPenetration;
        int flatArmorPenetration;
        double lifeSteal;
        double bonusAttackHitChance;


        double spellCriticalChance;
        double spellCriticalDamage;
        double perSpellPenetration;
        int flatSpellPenetration;
        double spellVamp;
        double bonusSpellHitChance;

        double healthRegen;
        double dodgeChance;
        double parryChance;
        double blockChance;
        double blockValue;


        Armor armor;
        SpellResitance spellResitance;
        SpellDamage spellDamage;
        WeaponSkill weaponSkill;
        int defense;

    }
}
