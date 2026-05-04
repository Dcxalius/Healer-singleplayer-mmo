using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spells.Buff;
using Project_1.World.GameObjects.Unit.Stats.Secondary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.World.GameObjects.Spells.SpellEffects
{
    internal abstract class LastingEffect : SpellEffect
    {

        public double Duration => duration;
        double duration;

        public LastingEffect(double aDuration, string aName, bool aIsBinary, HashSet<SpellSchool> aSpellSchools) : base(aName, aIsBinary, aSpellSchools)
        {
            duration = aDuration * 1000;
        }


    }
}
