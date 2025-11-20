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
        public Armor Armor => armor;
        Armor armor;
        public double HealthRegen => healthRegen;
        double healthRegen;
        public double DodgeChance => dodgeChance;
        double dodgeChance;//Base dodge + (Agility / Agility to Dodge ratio) + Talent bonuses + Race Bonuses + Item Bonuses
        public double ParryChance => parryChance;
        double parryChance; //5% base + parry rating + parry talents
        public double BlockChance => blockChance;
        double blockChance;
        public double BlockValue => blockValue;
        double blockValue;
        public SpellResitance SpellResitance => spellResitance;
        SpellResitance spellResitance;

        public Defense(UnitData aUnitData)
        {
            armor = new Armor(aUnitData.Equipment.GetArmor);


        }

        public void Refresh(UnitData aUnitData)
        {
            armor.Value = aUnitData.Equipment.GetArmor;

        }
        public void CalculateBlockReduction(ref Damage aDamage)
        {
            for (int i = 0; i < aDamage.DamageAmount.Length; i++)
            {
                if (aDamage.DamageType[i] == DamageType.Physical)
                {
                    double reducedAmount = aDamage.DamageAmount[i] - blockValue;
                    aDamage.DamageAmount[i] = reducedAmount < 0 ? 0 : reducedAmount;
                }
            }
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
