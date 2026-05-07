using Project_1.GameObjects.Unit.Classes;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class Attack
    {
        static void AssertSimThread() => ThreadAffinity.AssertSimThread();
        const float BASE_CRIT_CHANCE = 0.05f;
        const float BASE_CRIT_DAMAGE = 2f;
        public Attack(UnitData aUnitData)
        {
            Refresh(aUnitData);
        }

        protected Attack() { }

        protected float criticalChance;
        protected float criticalDamage;
        protected float percentPenetration;
        protected int flatPenetration;
        protected float vampirism;
        protected float bonusHitChance;

        public float CriticalChance => criticalChance;
        public float CriticalDamage => criticalDamage;
        public float PercentPenetration => percentPenetration;
        public int FlatPenetration => flatPenetration;
        public float Vampirism => vampirism;
        public float BonusHitChance => bonusHitChance;

        public virtual void Refresh(UnitData aUnitData)
        {
            AssertSimThread();

            //TODO: Implement spell stats calculations bellow, Don't forget adding racials and talents.
            criticalChance = (float)Math.Clamp(aUnitData.ApplyStatusModifiers("CritChance", BASE_CRIT_CHANCE + aUnitData.BaseStats.TotalPrimaryStats.Agility * aUnitData.ClassData.AttackCritChanceScaler), 0d, 1d);
            criticalDamage = (float)aUnitData.ApplyStatusModifiers("CritDamage", BASE_CRIT_DAMAGE + 0/* + unitData.AttackCriticalDamageMultiplier*/);
            percentPenetration = (float)Math.Clamp(aUnitData.ApplyStatusModifiers("PercentPenetration", 0 /*+ unitData.Equipment.GetPercentAttackPen*/), 0d, 1d);
            flatPenetration = aUnitData.ApplyStatusModifiersInt("FlatPenetration", 0 /*+ unitData.Equipment.GetFlatAttackPen*/);
            vampirism = (float)Math.Clamp(aUnitData.ApplyStatusModifiers("Vampirism", 0 /*+ unitData.Equipment.GetPercentAttackVamp*/), 0d, 1d);


            //Against level 60 targets, you need a total of 5% Hit Chance to never miss a target.
            //Dual Wielders need 24 % Hit Chance to never miss on every single auto-attack.
            //Against level 63(or Boss level) targets, you need a total of 9 % Hit Chance to never miss a target.
            //Dual Wielders need 25 % Hit Chance to never miss on every single auto-attack
            bonusHitChance = (float)Math.Clamp(aUnitData.ApplyStatusModifiers("BonusHitChance", 0/*+ unitData.Equipment.GetBonusHitAttack*/), 0d, 1d);

        }

    }
}
