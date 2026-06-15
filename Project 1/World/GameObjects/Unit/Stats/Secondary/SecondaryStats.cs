using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spawners;
using Project_1.GameObjects.Unit;
using Project_1.Items.SubTypes;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.World.GameObjects.Unit.Stats.Secondary
{
    internal class SecondaryStats
    {
        static void AssertSimThread() => ThreadAffinity.AssertSimThread();
        public SecondaryStats(UnitData aUnitData)
        {
            attack = new Attack(aUnitData);
            spell = aUnitData.ClassData.IsCaster ? new Spell(aUnitData) : new Spell(attack); //Q: Unsure if this is sustainable. A cleaner way is probably using the spell for all classes, and then having a tag in spells that says if it uses attack values and then in the calculations check that and pull attack instead of spell, rather than doing this stuff which can get weird
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
