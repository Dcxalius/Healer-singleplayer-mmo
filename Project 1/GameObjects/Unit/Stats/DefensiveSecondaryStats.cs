using System;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class DefensiveSecondaryStats
    {
        private static readonly Random RandomGenerator = new();

        public double HealthRegen { get; set; }
        public double DodgeChance { get; set; }
        public double ParryChance { get; set; }
        public double BlockChance { get; set; }
        public double BlockValue { get; set; }
        public Armor? Armor { get; set; }
        public SpellResitance? SpellResitance { get; set; }
        public int Defense { get; set; }

        public DefensiveSecondaryStats(
            double healthRegen = 0,
            double dodgeChance = 0,
            double parryChance = 0,
            double blockChance = 0,
            double blockValue = 0,
            Armor? armor = null,
            SpellResitance? spellResitance = null,
            int defense = 0)
        {
            HealthRegen = healthRegen;
            DodgeChance = dodgeChance;
            ParryChance = parryChance;
            BlockChance = blockChance;
            BlockValue = blockValue;
            Armor = armor;
            SpellResitance = spellResitance;
            Defense = defense;
        }

        public bool CalculateDodge() => RandomGenerator.NextDouble() < DodgeChance;
        public bool CalculateParry() => RandomGenerator.NextDouble() < ParryChance;
        public bool CalculateBlock() => RandomGenerator.NextDouble() < BlockChance;

        public Damage CalculateBlockReduction(Damage aDamage)
        {
            if (!CalculateBlock()) return aDamage;

            double reducedAmount = aDamage.DamageAmount - BlockValue;
            return new Damage(reducedAmount < 0 ? 0 : reducedAmount, aDamage.DamageType);
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
    }
}
