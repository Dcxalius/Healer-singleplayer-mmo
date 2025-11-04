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
        public Spell()
        {
            spellDamage = new SpellDamage();
        }

        SpellDamage spellDamage;

        public double SpellCriticalChance => criticalChance;
        public double SpellCriticalDamage => CriticalDamage;
        public double PercentSpellPenetration => percentPenetration;
        public int FlatSpellPenetration => flatPenetration;
        public double SpellVamp => vamp;
        public double BonusSpellHitChance => bonusHitChance;
        public override void Refresh(UnitData unitData)
        {
            //TODO: Implement spell stats calculations bellow, Don't forget adding racials and talents.
            spellCriticalChance = Math.Max(0, Math.Min(100,/* unitData.Equipment.GetSpellCrit */ unitData.BaseStats.TotalPrimaryStats.Intellect * unitData.ClassData.SpellCritChanceScaler));
            spellCriticalDamage = 2 * (1/* + unitData.CriticalDamageMultiplier*/);
            percentSpellPenetration = 0 /*+ unitData.Equipment.GetPercentSpellPen*/;
            flatSpellPenetration = 0 /*+ unitData.Equipment.GetFlatSpellPen*/;
            spellVamp = 0 /*+ unitData.Equipment.GetPercentSpellVamp*/; ;


            //Against same level targets, you need a total of 3% Spell Hit Chance to not miss a Spell.
            //Against +3 level targets, you need a total of 16 % Spell Hit Chance to not miss a Spell.
            bonusSpellHitChance = 0/*+ unitData.Equipment.GetBonusSpell*/ ;

        }
    }
}
