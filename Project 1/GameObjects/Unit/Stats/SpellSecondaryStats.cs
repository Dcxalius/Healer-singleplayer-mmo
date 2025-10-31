namespace Project_1.GameObjects.Unit.Stats
{
    internal class SpellSecondaryStats
    {
        public double CriticalChance { get; set; }
        public double CriticalDamage { get; set; }
        public double PercentSpellPenetration { get; set; }
        public int FlatSpellPenetration { get; set; }
        public double SpellVamp { get; set; }
        public double BonusHitChance { get; set; }
        public SpellDamage? SpellDamage { get; set; }

        public SpellSecondaryStats(
            double criticalChance = 0,
            double criticalDamage = 0,
            double percentSpellPenetration = 0,
            int flatSpellPenetration = 0,
            double spellVamp = 0,
            double bonusHitChance = 0,
            SpellDamage? spellDamage = null)
        {
            CriticalChance = criticalChance;
            CriticalDamage = criticalDamage;
            PercentSpellPenetration = percentSpellPenetration;
            FlatSpellPenetration = flatSpellPenetration;
            SpellVamp = spellVamp;
            BonusHitChance = bonusHitChance;
            SpellDamage = spellDamage;
        }
    }
}
