using Project_1.GameObjects.Doodads;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spawners;
using Project_1.GameObjects.Spells;
using Project_1.Managers;
using Project_1.UI.UIElements.Bars;
using Project_1.World.GameObjects.Unit.Stats.Secondary;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    internal struct Damage
    {
        //Note: This looks like it is getting close to finalization
        public bool ContainsDamage => value.Values.Sum() > 0;
        public double Sum => value.Values.Sum();
        public int Count => value.Count;
        public ImmutableList<DamageType> Types => value.Keys.ToImmutableList();
        public double this[DamageType aType] => value.ContainsKey(aType) ? value[aType] : 0;

        Dictionary<DamageType, double> value;

        public Damage(double aDamageAmount, DamageType aDamageType)
        {
            value = new Dictionary<DamageType, double>
            {
                { aDamageType, aDamageAmount }
            };
        }

        public Damage(double[] aDamageAmount, DamageType[] aDamageType)
        {
            value = new Dictionary<DamageType, double>();
            for (int i = 0; i < aDamageAmount.Length; i++)
            {
                if (!value.TryAdd(aDamageType[i], aDamageAmount[i]))
                    throw new Exception("Tried to add same damage type twice in Damage constructor.");
            }
        }

        static public Damage Zero => new Damage(new double[0], new DamageType[0]); //This might require DamageType to be new DamageType[1] { DamageType.True } or something

        public Damage(Damage aDamageTaken)
        {
            value = new Dictionary<DamageType, double>(aDamageTaken.value);
        }

        public void ApplyCriticalStrike(Entity aAttacker, Entity aDefender)
        {
            ThreadAffinity.AssertSimThread();
            double attackerCrit = aAttacker.SecondaryStats.Attack.CriticalDamage;
            double totalCrit = Math.Max(attackerCrit - aDefender.SecondaryStats.Defense.CriticalDamageReduction, 0);
            foreach (var (k, v) in value)
            {
                 value[k] *= totalCrit;
            }
        }

        public void ApplyCriticalStrike(double aCriticalMultiplier)
        {
            ThreadAffinity.AssertSimThread();
            foreach (var (k, v) in value)
            {
                value[k] *= aCriticalMultiplier;
            }
        }

        public void ApplyGlancingBlowDamage(Entity aAttackingUnit, Unit.Attack aAttack, Entity aMobData)
        {
            ThreadAffinity.AssertSimThread();
            int attackerWeaponSkill = Math.Min(aAttackingUnit.WeaponSkill.GetSkill(aAttack.WeaponType), aAttackingUnit.Level.CurrentLevel * 5);
            int mobDefense = aMobData.Level.CurrentLevel * 5;
            int ratingDifference = mobDefense - attackerWeaponSkill;
            int cappedRatingDifference = Math.Max(0, ratingDifference);

            double lowValue, highValue;

            //TODO: Seperate the below code to seperate methods
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

        public void ApplyBlocked(Entity aAttacker, Entity aDefender) //Q: Should we track blocked damage separately? Allowing that data to also be displayed.
        {
            ThreadAffinity.AssertSimThread();
            foreach (var (k, v) in value)
            {
                if (v <= 0) continue;
                if (aDefender.Equipment.CanShieldBlock(k)) value[k] = Math.Max(0, v - aDefender.SecondaryStats.Defense.BlockValue);
            }
        }

        public void ApplyDamageReduction(Entity aAttacker, Entity aDefender, IDamager aDamager)
        {
            ThreadAffinity.AssertSimThread();
            SpellResitance defenderSpellResitance = aDefender.SecondaryStats.Defense.SpellResistance;
           
            foreach (var (k, v) in value)
            {
                switch (k)
                {
                    //TODO: This should probably be reworked, using a seperate DamageType => SpellSchool pattern and then just checking if the damage type is physical, and if not doing the spell calc using the spell school.
                    case DamageType.Physical:
                        value[k] *= Defense.CalculateDamageReductionArmor(aDefender.Equipment.GetArmor * aAttacker.SecondaryStats.Attack.PercentPenetration - aAttacker.SecondaryStats.Attack.FlatPenetration, aAttacker.Level.CurrentLevel);
                        break;
                    case DamageType.Arcane:
                        //Q: How do we want to handle resistances for spells with multiple schools? A: For now the damage is calculated seperately. Need to check how we calculate resitance prob though.
                        //Q: How do we want to handle partial resists for binary spells? A: If wow-like, not at all
                        //Q: How do we handle spelleffects, are slows and stuff only binary or partial as well? Just because wow doesn't allow for partially resisting a frost nova doesn't mean we have to.
                        

                        if (!aDamager.BinarySpell)
                            value[k] *= defenderSpellResitance.CalculateDamageReductionNonBinary(aDefender, aAttacker, SpellSchool.Arcane);
                        break;
                    case Stats.DamageType.Fire:
                        if (!aDamager.BinarySpell)
                            value[k] *= defenderSpellResitance.CalculateDamageReductionNonBinary(aDefender, aAttacker, SpellSchool.Fire);
                        break;
                    case DamageType.Frost:
                        if (!aDamager.BinarySpell)
                            value[k] *= defenderSpellResitance.CalculateDamageReductionNonBinary(aDefender, aAttacker, SpellSchool.Frost);
                        break;
                    case DamageType.Holy:
                        if (!aDamager.BinarySpell)
                            value[k] *= defenderSpellResitance.CalculateDamageReductionNonBinary(aDefender, aAttacker, SpellSchool.Holy);
                        break;
                    case DamageType.Nature:
                        if (!aDamager.BinarySpell)
                            value[k] *= defenderSpellResitance.CalculateDamageReductionNonBinary(aDefender, aAttacker, SpellSchool.Nature);
                        break;
                    case DamageType.Shadow:
                        if (!aDamager.BinarySpell)
                            value[k] *= defenderSpellResitance.CalculateDamageReductionNonBinary(aDefender, aAttacker, SpellSchool.Shadow);
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
            ThreadAffinity.AssertSimThread();
            foreach (var (k, v) in value)
            {
                value[k] *= 1.5;
            }
        }
    }

    public enum DamageType //TODO: Move this into the Damage class
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
