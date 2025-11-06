using Project_1.GameObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class Defense
    {
        Armor armor;
        double healthRegen;
        double dodgeChance;
        double parryChance;
        double blockChance;
        double blockValue;
        SpellResitance spellResitance;

        public Defense(UnitData aUnitData)
        {
//            All classes but Hunters and Rogues receive 1 % Dodge for every 20 points of Agility.
//Rogues receive 1 % Dodge for every 14.5 points of Agility.
//Hunters receive 1 % Dodge for every 26 points of Agility.

        }

        public void Refresh(UnitData aUnitData)
        {
            armor = new Armor(aUnitData.Equipment.GetArmor);
        }

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
        public static double CalculateEHP(double playerHealth, double armorDamageReduction)
        {
            if (armorDamageReduction < 0 || armorDamageReduction >= 1)
                throw new ArgumentOutOfRangeException(nameof(armorDamageReduction), "Damage reduction must be between 0 and 1 (exclusive).");

            return playerHealth / (1 - armorDamageReduction);
        }

        public static double CalculateDamageReductionArmor(double armor, int attackerLevel)
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

    }
}
