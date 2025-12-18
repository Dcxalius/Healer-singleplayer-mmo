using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spawners;
using Project_1.Items.SubTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class SecondaryStats
    {
        public SecondaryStats(UnitData aUnitData)
        {
            if (aUnitData.ClassData.IsCaster)
            {
                spell = new Spell(aUnitData);
            }
            attack = new Attack(aUnitData);
            defense = new Defense(aUnitData);
        }

        //public SecondayStatBonus<T> xdd<T>(string aSecondaryStat, DamageType damageType)
        //{
        //    if (damageType == DamageType.Physical);
        //        //return Attack.
        //    if (spell != null)
        //    {
        //        SpellSchool spellSchool = damageType switch
        //        {
        //            DamageType.Arcane => SpellSchool.Arcane,
        //            DamageType.Fire => SpellSchool.Fire,
        //            DamageType.Frost => SpellSchool.Frost,
        //            DamageType.Holy => SpellSchool.Holy,
        //            DamageType.Nature => SpellSchool.Nature,
        //            DamageType.Shadow => SpellSchool.Shadow,
        //            _ => SpellSchool.Base,
        //        };
        //        return spell.GetSecondaryStat<T>(spellSchool, aSecondaryStat);
        //    }
        //}

        public Attack Spell
        {
            get
            {
                //DamageType damageType = attack.DamageType;
                if (spell == null) return attack;
                return attack;
            }
        }
        Spell spell;
        public Defense Defense => defense;
        Defense defense;
        public Attack Attack => attack;
        Attack attack;
    }
}
