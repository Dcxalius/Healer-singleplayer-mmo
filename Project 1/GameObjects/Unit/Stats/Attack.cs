using Project_1.GameObjects.Unit.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.GameObjects.Spells.AoE.AreaOfEffectData;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class Attack
    {

        protected double criticalChance;
        protected double criticalDamage;
        protected double percentPenetration;
        protected int flatPenetration;
        protected double lifeSteal;
        protected double bonusHitChance;

        public virtual void Refresh(UnitData unitData)
        {

            //TODO: Implement spell stats calculations bellow, Don't forget adding racials and talents.
            criticalChance = Math.Max(0, Math.Min(100,/* unitData.Equipment.GetAttackCrit */ unitData.BaseStats.TotalPrimaryStats.Agility     * unitData.ClassData.AttackCritChanceScaler));
            criticalDamage = 2 * (1/* + unitData.AttackCriticalDamageMultiplier*/);
            percentPenetration = 0 /*+ unitData.Equipment.GetPercentAttackPen*/;
            flatPenetration = 0 /*+ unitData.Equipment.GetFlatAttackPen*/;
            lifeSteal = 0 /*+ unitData.Equipment.GetPercentAttackVamp*/; ;


            //Against level 60 targets, you need a total of 5% Hit Chance to never miss a target.
            //Dual Wielders need 24 % Hit Chance to never miss on every single auto-attack.
            //Against level 63(or Boss level) targets, you need a total of 9 % Hit Chance to never miss a target.
            //Dual Wielders need 25 % Hit Chance to never miss on every single auto-attack
            bonusHitChance = 0/*+ unitData.Equipment.GetBonusHitAttack*/ ;

        }

    }
}
