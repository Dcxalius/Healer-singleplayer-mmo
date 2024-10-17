using Project_1.GameObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells
{
    internal class InstantDamage : Instant
    {
        public int damage;

        public InstantDamage(string aName) : base(aName)
        {

        }

        public override bool Trigger(Entity aTarget)
        {
            //aTarget.TakeDamage()

            return base.Trigger(aTarget);
        }
    }
}
