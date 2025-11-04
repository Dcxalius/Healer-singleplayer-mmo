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
        public SecondaryStats(Entity aEntity)
        {
            if (aEntity.ClassData.IsCaster)
            {
                spell = new Spell();
            }
            attack = new Attack();
            defense = new Defense();
        }


        public Attack Spell
        {
            get
            {
                if (spell == null) return attack;
                return spell;
            }
        }

        Spell spell = new Spell();
        public Defense Defense => defense;
        Defense defense = new Defense();
        public Attack Attack => attack;
        Attack attack = new Attack();
    }
}
