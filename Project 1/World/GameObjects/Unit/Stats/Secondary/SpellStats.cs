using Newtonsoft.Json;
using Project_1.GameObjects.Unit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.World.GameObjects.Unit.Stats.Secondary
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
            bool isBaseSchool = spellSchool == SpellSchool.Base;
            string statPrefix = spellSchool.ToString();
            spellDamage = aUnitData.Equipment.GetSecondaryStat<int>(statPrefix + "SpellDamage")
                + aUnitData.GetTalentSecondaryStat<int>(statPrefix + "SpellDamage");
            double baseCritChance = isBaseSchool
                ? BASE_CRIT_CHANCE + aUnitData.BaseStats.TotalPrimaryStats.Intellect * aUnitData.ClassData.SpellCritChanceScaler
                : 0d;
            double baseCritDamage = isBaseSchool ? BASE_CRIT_DAMAGE : 0d;
            critChance = Math.Clamp(baseCritChance + aUnitData.Equipment.GetSecondaryStat<double>(statPrefix + "SpellCritChance") + aUnitData.GetTalentSecondaryStat<double>(statPrefix + "SpellCritChance"), 0d, 1d);
            critDamage = baseCritDamage + aUnitData.Equipment.GetSecondaryStat<double>(statPrefix + "SpellCritDamage") + aUnitData.GetTalentSecondaryStat<double>(statPrefix + "SpellCritDamage");
            flatPenetration = aUnitData.Equipment.GetSecondaryStat<int>(statPrefix + "SpellFlatPenetration")
                + aUnitData.GetTalentSecondaryStat<int>(statPrefix + "SpellFlatPenetration");
            percentPenetration = Math.Clamp(aUnitData.Equipment.GetSecondaryStat<double>(statPrefix + "SpellPercentPenetration") + aUnitData.GetTalentSecondaryStat<double>(statPrefix + "SpellPercentPenetration"), 0d, 1d);
            haste = Math.Clamp(aUnitData.Equipment.GetSecondaryStat<double>(statPrefix + "SpellHaste") + aUnitData.GetTalentSecondaryStat<double>(statPrefix + "SpellHaste"), 0d, 1d);
            vampirism = Math.Clamp(aUnitData.Equipment.GetSecondaryStat<double>(statPrefix + "SpellVampirism") + aUnitData.GetTalentSecondaryStat<double>(statPrefix + "SpellVampirism"), 0d, 1d);
            bonusHitChance = Math.Clamp(aUnitData.Equipment.GetSecondaryStat<double>(statPrefix + "SpellBonusHitChance") + aUnitData.GetTalentSecondaryStat<double>(statPrefix + "SpellBonusHitChance"), 0d, 1d);
        }

        public SpellStats(SpellSchool aSchool) 
        {
            spellSchool = aSchool;
            spellDamage = 0;
            critChance = aSchool == SpellSchool.Base ? BASE_CRIT_CHANCE : 0d;
            critDamage = aSchool == SpellSchool.Base ? BASE_CRIT_DAMAGE : 0d;
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

        internal static SpellStats CreateForValidation(
            SpellSchool aSchool,
            int aSpellDamage = 0,
            double aCritChance = BASE_CRIT_CHANCE,
            double aCritDamage = BASE_CRIT_DAMAGE,
            int aFlatPenetration = 0,
            double aPercentPenetration = 0,
            double aHaste = 0,
            double aVampirism = 0,
            double aBonusHitChance = 0)
        {
            SpellStats stats = new SpellStats(aSchool)
            {
                spellDamage = aSpellDamage,
                critChance = aCritChance,
                critDamage = aCritDamage,
                flatPenetration = aFlatPenetration,
                percentPenetration = aPercentPenetration,
                haste = aHaste,
                vampirism = aVampirism,
                bonusHitChance = aBonusHitChance
            };

            return stats;
        }
    }
}
