using Project_1.GameObjects.Entities;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells
{
    internal struct SpellData
    {
        

        public string Name { get => name; }
        public double Cooldown { get => cooldown; }

        public bool Targetable(UnitData.RelationToPlayer aTarget) { return acceptableTargets.Contains(aTarget); }

        string name;
        double cooldown;
        double castTime;
        SpellEffect[] effects;
        UnitData.RelationToPlayer[] acceptableTargets;
        
        
    }
}
