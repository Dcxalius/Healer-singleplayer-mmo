using Project_1.GameObjects.Doodads;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spawners;
using Project_1.GameObjects.Spells;
using Project_1.Managers;
using Project_1.UI.UIElements.Bars;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.GameObjects.Spells.AoE.AreaOfEffectData;

namespace Project_1.GameObjects.Unit.Stats
{
    internal struct Damage
    {
        public bool ContainsDamage => value.Values.Sum() > 0;
        public double Sum => value.Values.Sum();
        public int Count => value.Count;
        public ImmutableList<DamageType> Types => value.Keys.ToImmutableList();
        public double this[DamageType aType] => value.ContainsKey(aType) ? value[aType] : 0;

        Dictionary<DamageType, double> value;

        public Damage(double[] aDamageAmount, DamageType[] aDamageType)
        {
            value = new Dictionary<DamageType, double>();
        }

        public Damage(Damage aDamageTaken)
        {
            value = new Dictionary<DamageType, double>(aDamageTaken.value);
        }

        public void ApplyCriticalStrike(Entity aAttacker, Entity aDefender)
        {
            double attackerCrit = aAttacker.SecondaryStats.Attack.CriticalDamage;
            double totalCrit = Math.Max(attackerCrit - aDefender.SecondaryStats.Defense.CriticalDamageReduction, 0);
            foreach (var (k, v) in value)
            {
                 value[k] *= totalCrit;
            }
        }

        public void ApplyGlancingBlowDamage(Entity aAttackingUnit, Unit.Attack aAttack, Entity aMobData)
        {
            int attackerWeaponSkill = Math.Min(aAttackingUnit.WeaponSkill.GetSkill(aAttack.WeaponType), aAttackingUnit.Level.CurrentLevel * 5);
            int mobDefense = aMobData.Level.CurrentLevel * 5;
            int ratingDifference = mobDefense - attackerWeaponSkill;
            int cappedRatingDifference = Math.Max(0, ratingDifference);

            double lowValue, highValue;

            if (aAttackingUnit.ClassData.IsCaster)
            {
                lowValue = Math.Max(0.01, Math.Min(0.6, 1.3 - 0.05 * ratingDifference - 0.7));
                highValue = Math.Max(0.2, Math.Min(0.99, 1.2 - 0.03 * ratingDifference - 0.3));
            }
            else
            {
                //307 skill level should be softcap glancing blows
                lowValue = Math.Max(0.01, Math.Min(0.91, 1.3 - 0.05 * ratingDifference));
                highValue = Math.Max(0.2, Math.Min(0.99, 1.2 - 0.03 * ratingDifference));
            }

            if (aMobData.Level.CurrentLevel > aAttackingUnit.Level.CurrentLevel)
            {
                foreach (var (k, v) in value)
                {
                    if (k == DamageType.Physical)
                    {
                        value[k] *= RandomManager.RollDouble(lowValue, highValue);
                        return;
                    }
                }
            }
        }

        public void ApplyBlocked(Entity aAttacker, Entity aDefender)
        {
            foreach (var (k, v) in value)
            {
                if (v <= 0) continue;
                if (aDefender.Equipment.CanShieldBlock(k)) value[k] = Math.Max(0, v - aDefender.SecondaryStats.Defense.BlockValue);
            }
        }

        public void ApplyDamageReduction(Entity aAttacker, Entity aDefender, IDamager aDamager)
        {
            foreach (var (k, v) in value)
            {
                switch (k)
                {
                    case DamageType.Physical:
                        value[k] *= Defense.CalculateDamageReductionArmor(aDefender.Equipment.GetArmor * aAttacker.SecondaryStats.Attack.PercentPenetration - aAttacker.SecondaryStats.Attack.FlatPenetration, aAttacker.Level.CurrentLevel);
                        break;
                    case DamageType.Arcane:
                        //How do we want to handle resistances for spells with multiple schools?
                        //How do we want to handle partial resists for binary spells? If wow like not at all
                        //How do we handle spelleffects, do slows and stuff only binary or partial as well?
                        value[k] *= SpellResitance.CalculateDamageReductionNonBinary(aDefender, aAttacker, SpellSchool.Arcane);
                        break;
                    case Stats.DamageType.Fire:
                        // Implement Fire damage reduction logic here
                        value[k] *= SpellResitance.CalculateDamageReductionNonBinary(aDefender, aAttacker, SpellSchool.Fire);
                        break;
                    case DamageType.Frost:
                        // Implement Frost damage reduction logic here
                        value[k] *= SpellResitance.CalculateDamageReductionNonBinary(aDefender, aAttacker, SpellSchool.Frost);
                        break;
                    case DamageType.Holy:
                        // Implement Holy damage reduction logic here
                        value[k] *= SpellResitance.CalculateDamageReductionNonBinary(aDefender, aAttacker, SpellSchool.Holy);
                        break;
                    case DamageType.Nature:
                        // Implement Nature damage reduction logic here
                        value[k] *= SpellResitance.CalculateDamageReductionNonBinary(aDefender, aAttacker, SpellSchool.Nature);
                        break;
                    case DamageType.Shadow:
                        // Implement Shadow damage reduction logic here
                        value[k] *= SpellResitance.CalculateDamageReductionNonBinary(aDefender, aAttacker, SpellSchool.Shadow);
                        break;
                    case DamageType.True:
                        break;
                    default:
                        throw new Exception("huh");
                }
            }
            
        }
        public void ApplyCrushingDamage(Entity aMobData, Entity aUnitData)
        {
            if (!value.Keys.Contains(DamageType.Physical))
                return;
            
            value[DamageType.Physical] *= 1.5;
        }
    }

   


    public enum DamageType
    {
        Physical,
        Arcane,
        Fire,
        Frost,
        Holy,
        Nature,
        Shadow,
        True
    }
}
