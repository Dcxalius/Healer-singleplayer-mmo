using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spawners;
using Project_1.Items.SubTypes;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class SecondaryStats
    {
        static void AssertSimThread() => ThreadAffinity.AssertSimThread();
        public SecondaryStats(UnitData aUnitData)
        {
            attack = new Attack(aUnitData);
            spell = aUnitData.ClassData.IsCaster
                ? new Spell(aUnitData)
                : new Spell(attack);
            defense = new Defense(aUnitData);
        }

        public void Refresh(UnitData aUnitData)
        {
            AssertSimThread();
            attack.Refresh(aUnitData);
            spell.Refresh(aUnitData);
            defense.Refresh(aUnitData);
        }

        public Spell Spell => spell;
        Spell spell;
        public Defense Defense => defense;
        Defense defense;
        public Attack Attack => attack;
        Attack attack;
    }
}
