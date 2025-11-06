using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.GameObjects.Spells.AoE.AreaOfEffectData;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class Spell : Attack
    {
        public Spell(UnitData unitData)
        {
            spellDamage = new SpellDamage();
            Refresh(unitData);
        }

        SpellDamage spellDamage;

        public override void Refresh(UnitData unitData)
        {
            //TODO: Implement spell stats calculations bellow, Don't forget adding racials and talents.
            criticalChance = Math.Max(0, Math.Min(100,/* unitData.Equipment.GetSpellCrit */ unitData.BaseStats.TotalPrimaryStats.Intellect * unitData.ClassData.SpellCritChanceScaler));
            criticalDamage = 2 * (1/* + unitData.CriticalDamageMultiplier*/);
            percentPenetration = 0 /*+ unitData.Equipment.GetPercentSpellPen*/;
            flatPenetration = 0 /*+ unitData.Equipment.GetFlatSpellPen*/;
            vampirism = 0 /*+ unitData.Equipment.GetPercentSpellVamp*/; ;


            //Against same level targets, you need a total of 3% Spell Hit Chance to not miss a Spell.
            //Against +3 level targets, you need a total of 16 % Spell Hit Chance to not miss a Spell.
            bonusHitChance = 0/*+ unitData.Equipment.GetBonusSpell*/ ;
            spellDamage.Refresh(unitData);
        }
    }
}
