using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    public enum SpellSchool
    {
        Base,
        Arcane,
        Fire,
        Frost,
        Holy,
        Nature,
        Shadow,
        Healing
    }

    internal struct SpellStats 
    {
        const double BASE_CRIT_CHANCE = 0.05;
        const double BASE_CRIT_DAMAGE = 1.5;
        public static (string, Type)[] SecondaryStatsAsStrings => new (string, Type)[]
        {
            ("SpellDamage", typeof(int)),
            ("SpellCritChance", typeof(double)),
            ("SpellCritDamage", typeof(double)),
            ("SpellFlatPenetration", typeof(int)),
            ("SpellPercentPenetration", typeof(double)),
            ("SpellHaste", typeof(double)),
            ("SpellVampirism", typeof(double)),
            ("SpellBonusHitChance", typeof(double))
        };

        public T GetValue<T>(string aSecondaryStat)
        {
            Debug.Assert(typeof(T) == typeof(int) || typeof(T) == typeof(double), "Invalid type requested from SpellStats.");
            T returnable = aSecondaryStat switch
            {
                "SpellDamage" => (T)(object)spellDamage,
                "SpellCritChance" => (T)(object)critChance,
                "SpellCritDamage" => (T)(object)critDamage,
                "SpellFlatPenetration" => (T)(object)flatPenetration,
                "SpellPercentPenetration" => (T)(object)percentPenetration,
                "SpellHaste" => (T)(object)haste,
                "SpellVampirism" => (T)(object)vampirism,
                "SpellBonusHitChance" => (T)(object)bonusHitChance,
                _ => throw new Exception("Invalid secondary stat requested from SpellStats."),
            };
            Debug.Assert(aSecondaryStat switch
            {
                "SpellDamage" => typeof(T) == typeof(int),
                "SpellCritChance" => typeof(T) == typeof(double),
                "SpellCritDamage" => typeof(T) == typeof(double),
                "SpellFlatPenetration" => typeof(T) == typeof(int),
                "SpellPercentPenetration" => typeof(T) == typeof(double),
                "SpellHaste" => typeof(T) == typeof(double),
                "SpellVampirism" => typeof(T) == typeof(double),
                "SpellBonusHitChance" => typeof(T) == typeof(double),
                _ => false,
            }, "Type requested from SpellStats does not match the type of the secondary stat.");

            return returnable;
        }
        public SpellSchool SpellSchool => spellSchool;
        SpellSchool spellSchool;
        public int SpellDamageValue => spellDamage;
        int spellDamage;
        public double CritChance => critChance;
        double critChance;
        public double CritDamage => critDamage;
        double critDamage;
        public int FlatPenetration => flatPenetration;
        int flatPenetration;
        public double PercentPenetration => percentPenetration;
        double percentPenetration;
        public double Haste => haste;
        double haste;
        public double Vampirism => vampirism;
        double vampirism;
        public double BonusHitChance => bonusHitChance;
        double bonusHitChance;

        public void Refresh(UnitData aUnitData)
        {
            spellDamage = aUnitData.Equipment.GetSecondaryStat<int>(spellSchool.ToString() + "SpellDamage");
            critChance = Math.Clamp(BASE_CRIT_CHANCE + aUnitData.BaseStats.TotalPrimaryStats.Intellect * aUnitData.ClassData.SpellCritChanceScaler + aUnitData.Equipment.GetSecondaryStat<double>(spellSchool.ToString() + "SpellCritChance"), 0d, 1d);
            critDamage = BASE_CRIT_DAMAGE + aUnitData.Equipment.GetSecondaryStat<double>(spellSchool.ToString() + "SpellCritDamage");
            flatPenetration = aUnitData.Equipment.GetSecondaryStat<int>(spellSchool.ToString() + "SpellFlatPenetration");
            percentPenetration = Math.Clamp(aUnitData.Equipment.GetSecondaryStat<double>(spellSchool.ToString() + "SpellPercentPenetration"), 0d, 1d);
            haste = Math.Clamp(aUnitData.Equipment.GetSecondaryStat<double>(spellSchool.ToString() + "SpellHaste"), 0d, 1d);
            vampirism = Math.Clamp(aUnitData.Equipment.GetSecondaryStat<double>(spellSchool.ToString() + "SpellVampirism"), 0d, 1d);
            bonusHitChance = Math.Clamp(aUnitData.Equipment.GetSecondaryStat<double>(spellSchool.ToString() + "SpellBonusHitChance"), 0d, 1d);
        }

        public SpellStats(SpellSchool aSchool) 
        {
            spellSchool = aSchool;
            spellDamage = 0;
            critChance = BASE_CRIT_CHANCE;
            critDamage = BASE_CRIT_DAMAGE;
            flatPenetration = 0;
            percentPenetration = 0;
            haste = 0;
            vampirism = 0;
            bonusHitChance = 0;
        }
        public SpellStats(SpellSchool aSchool, UnitData unitData) : this()
        {
            spellSchool = aSchool;
            Refresh(unitData);
        }
    }
}
