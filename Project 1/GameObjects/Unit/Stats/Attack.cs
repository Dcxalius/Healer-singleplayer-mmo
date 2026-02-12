using Project_1.GameObjects.Unit.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using static Project_1.GameObjects.Spells.AoE.AreaOfEffectData;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class Attack
    {
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

            //TODO: Implement spell stats calculations bellow, Don't forget adding racials and talents.
            criticalChance = Math.Clamp(BASE_CRIT_CHANCE + aUnitData.BaseStats.TotalPrimaryStats.Agility * aUnitData.ClassData.AttackCritChanceScaler, 0f, 1f);
            criticalDamage = BASE_CRIT_DAMAGE + 0/* + unitData.AttackCriticalDamageMultiplier*/;
            percentPenetration = Math.Clamp(0 /*+ unitData.Equipment.GetPercentAttackPen*/, 0f, 1f);
            flatPenetration = 0 /*+ unitData.Equipment.GetFlatAttackPen*/;
            vampirism = Math.Clamp(0 /*+ unitData.Equipment.GetPercentAttackVamp*/, 0f, 1f);


            //Against level 60 targets, you need a total of 5% Hit Chance to never miss a target.
            //Dual Wielders need 24 % Hit Chance to never miss on every single auto-attack.
            //Against level 63(or Boss level) targets, you need a total of 9 % Hit Chance to never miss a target.
            //Dual Wielders need 25 % Hit Chance to never miss on every single auto-attack
            bonusHitChance = Math.Clamp(0/*+ unitData.Equipment.GetBonusHitAttack*/, 0f, 1f);

        }

    }
}
