using Project_1.GameObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells
{
    internal class Instant : SpellEffect
    {


        public Instant(string aName) : base(aName)
        {

        }

        public virtual bool Trigger(Entity aTarget)
        {
            return false;
        }
    }
}
