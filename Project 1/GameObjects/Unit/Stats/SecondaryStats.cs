namespace Project_1.GameObjects.Unit.Stats
{
    internal class SecondaryStats
    {
        public SecondaryStats()
        {
            Attack = new AttackSecondaryStats();
            Spell = new SpellSecondaryStats();
            Defensive = new DefensiveSecondaryStats();
        }

        public SecondaryStats(
            AttackSecondaryStats? attack,
            SpellSecondaryStats? spell,
            DefensiveSecondaryStats? defensive)
        {
            Attack = attack ?? new AttackSecondaryStats();
            Spell = spell ?? new SpellSecondaryStats();
            Defensive = defensive ?? new DefensiveSecondaryStats();
        }

        public AttackSecondaryStats Attack { get; }
        public SpellSecondaryStats Spell { get; }
        public DefensiveSecondaryStats Defensive { get; }

        public bool CalculateDodge() => Defensive.CalculateDodge();
        public bool CalculateParry() => Defensive.CalculateParry();
        public bool CalculateBlock() => Defensive.CalculateBlock();
        public Damage CalculateBlockReduction(Damage aDamage) => Defensive.CalculateBlockReduction(aDamage);
        public double CalculateEHP(double playerHealth, double armorDamageReduction) => Defensive.CalculateEHP(playerHealth, armorDamageReduction);
        public double CalculateDamageReduction(double armor, int attackerLevel) => Defensive.CalculateDamageReduction(armor, attackerLevel);
    }
}
