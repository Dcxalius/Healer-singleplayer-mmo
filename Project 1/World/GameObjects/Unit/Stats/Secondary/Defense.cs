using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Unit;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.World.GameObjects.Unit.Stats.Secondary
{
    internal class Defense //TODO: Summaries
    {
        static void AssertSimThread() => ThreadAffinity.AssertSimThread();
        public Armor Armor => armor;
        Armor armor;
        public double Hp5 => hp5;
        double hp5 = 0;
        public double SpiritHp5 => spiritHp5;
        double spiritHp5 = 0;
        public double HealthRegen => hp5 + spiritHp5;
        public double DodgeChance => dodgeChance;
        double dodgeChance = 0;
        public double ParryChance => parryChance;
        double parryChance = 0;
        public double BlockChance => blockChance;
        double blockChance = 0;
        public double BlockValue => blockValue;
        double blockValue = 0;
        public double CriticalChanceReduction => criticalChanceReduction;
        double criticalChanceReduction = 0;
        public double CriticalDamageReduction => criticalDamageReduction;
        double criticalDamageReduction = 0;
        public SpellResitance SpellResistance => spellResitance;
        SpellResitance spellResitance;

        public Defense(UnitData aUnitData)
        {
            armor = new Armor(aUnitData.Equipment.GetArmor);
            spellResitance = new SpellResitance();
            Refresh(aUnitData);
        }

        //TODO: Add Talent, Racial and Buffs to these values
        void RefreshArmor(UnitData aUnitData) => armor.Value = aUnitData.ApplyStatusModifiersInt("Armor", aUnitData.Equipment.GetArmor + aUnitData.BaseStats.TotalPrimaryStats.Agility.Armor); //TODO: This should be moved inside of armor.
        void RefreshDodge(UnitData aUnitData) => dodgeChance = Math.Clamp(aUnitData.ApplyStatusModifiers("DodgeChance", aUnitData.ClassData.BaseDodge + aUnitData.BaseStats.TotalPrimaryStats.Agility.GetDodgeChanceBonus(aUnitData.ClassData) + aUnitData.Equipment.GetSecondaryStat<double>("DodgeChance")), 0d, 1d);
        void RefreshParry(UnitData aUnitData) => parryChance = aUnitData.ClassData.CanParry ? Math.Clamp(aUnitData.ApplyStatusModifiers("ParryChance", 0.05d + aUnitData.Equipment.GetSecondaryStat<double>("ParryChance")), 0d, 1d) : 0d;
        void RefreshBlockChance(UnitData aUnitData) => blockChance = Math.Clamp(aUnitData.ApplyStatusModifiers("BlockChance", aUnitData.Equipment.HasShield ? 0.05d + aUnitData.Equipment.GetSecondaryStat<double>("BlockChance") : 0d), 0d, 1d);
        void RefreshBlockValue(UnitData aUnitData) => blockValue = aUnitData.ApplyStatusModifiers("BlockValue", aUnitData.Equipment.HasShield ? aUnitData.Equipment.GetSecondaryStat<double>("BlockValue") + aUnitData.BaseStats.TotalPrimaryStats.Strength.BlockValue : 0d);
        void RefreshHp5(UnitData aUnitData) => hp5 = aUnitData.ApplyStatusModifiers("Hp5", aUnitData.Equipment.GetSecondaryStat<double>("Hp5") + aUnitData.Equipment.GetSecondaryStat<int>("Hp5"));
        void RefreshSpiritHp5(UnitData aUnitData) => spiritHp5 = aUnitData.ApplyStatusModifiers("SpiritHp5", aUnitData.ClassData.BaseHp5 + aUnitData.BaseStats.TotalPrimaryStats.Spirit.GetHp5Bonus(aUnitData.ClassData)); //I dont think these values will ever be on eq; + aUnitData.Equipment.GetSecondaryStat<double>("SpiritHp5") + aUnitData.Equipment.GetSecondaryStat<int>("SpiritHp5"));
        public void Refresh(UnitData aUnitData)
        {
            AssertSimThread();
            RefreshArmor(aUnitData);
            RefreshDodge(aUnitData);
            RefreshParry(aUnitData);
            RefreshBlockChance(aUnitData);
            RefreshBlockValue(aUnitData);
            RefreshHp5(aUnitData);
            RefreshSpiritHp5(aUnitData);

            
            RefreshSpellResitance(aUnitData);
        }

        void RefreshSpellResitance(UnitData aUnitData)
        {
            AssertSimThread();
            Dictionary<SpellSchool, int> resitanceBySchool = new Dictionary<SpellSchool, int>
            {
                [SpellSchool.Arcane] = aUnitData.ApplyStatusModifiersInt("ArcaneResist", aUnitData.Equipment.GetSecondaryStat<int>("ArcaneResist")),
                [SpellSchool.Fire] = aUnitData.ApplyStatusModifiersInt("FireResist", aUnitData.Equipment.GetSecondaryStat<int>("FireResist")),
                [SpellSchool.Frost] = aUnitData.ApplyStatusModifiersInt("FrostResist", aUnitData.Equipment.GetSecondaryStat<int>("FrostResist")),
                [SpellSchool.Holy] = aUnitData.ApplyStatusModifiersInt("HolyResist", aUnitData.Equipment.GetSecondaryStat<int>("HolyResist")),
                [SpellSchool.Nature] = aUnitData.ApplyStatusModifiersInt("NatureResist", aUnitData.Equipment.GetSecondaryStat<int>("NatureResist")),
                [SpellSchool.Shadow] = aUnitData.ApplyStatusModifiersInt("ShadowResist", aUnitData.Equipment.GetSecondaryStat<int>("ShadowResist"))
            };

            spellResitance.SetValues(0, resitanceBySchool); //TODO: Implement getting All resitances and remove the hardcoded 0
        }
        public static double CalculateEHP(double playerHealth, double armorDamageReduction)
        {
            if (armorDamageReduction < 0 || armorDamageReduction >= 1)
                throw new ArgumentOutOfRangeException(nameof(armorDamageReduction), "Damage reduction must be between 0 and 1 (exclusive).");

            return playerHealth / (1 - armorDamageReduction);
        }

        public static double CalculateDamageReductionArmor(double armor, int attackerLevel) //TODO: This should be moved to an Armor class probably
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

            return 1 - damageReduction;
        }

    }
}
