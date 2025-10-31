using Project_1.Items.SubTypes;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class AttackSecondaryStats
    {
        public double CriticalChance { get; set; }
        public double CriticalDamage { get; set; }
        public double PercentArmorPenetration { get; set; }
        public int FlatArmorPenetration { get; set; }
        public double LifeSteal { get; set; }
        public double BonusHitChance { get; set; }
        public WeaponSkill? WeaponSkill { get; set; }

        public AttackSecondaryStats(
            double criticalChance = 0,
            double criticalDamage = 0,
            double percentArmorPenetration = 0,
            int flatArmorPenetration = 0,
            double lifeSteal = 0,
            double bonusHitChance = 0,
            WeaponSkill? weaponSkill = null)
        {
            CriticalChance = criticalChance;
            CriticalDamage = criticalDamage;
            PercentArmorPenetration = percentArmorPenetration;
            FlatArmorPenetration = flatArmorPenetration;
            LifeSteal = lifeSteal;
            BonusHitChance = bonusHitChance;
            WeaponSkill = weaponSkill;
        }
    }
}
