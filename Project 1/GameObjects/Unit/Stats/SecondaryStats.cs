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


        public Attack Spell
        {
            get
            {
                if (spell == null) return attack;
                return spell;
            }
        }
        Spell spell;
        public Defense Defense => defense;
        Defense defense;
        public Attack Attack => attack;
        Attack attack;
    }
}
