using Newtonsoft.Json;
using Project_1.GameObjects.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells
{
    internal class Instant : SpellEffect
    {
        enum Type
        {
            Heal,
            Attack
        }

        int value;
        Type type;

        [JsonConstructor]
        public Instant(string name, int value) : base(name)
        {
            Debug.Assert(value != 0, "Tried to make effect with no effect.");
            this.value = value;
            if (value > 0)
            {
                type = Type.Heal;
            }
            else
            {
                type = Type.Attack;
            }
        }

        public override bool Trigger(Entity aCaster, Entity aTarget)
        {
            if (type == Type.Attack)
            {
                aTarget.TakeDamage(aCaster, value);

            }

            return false;
        }
    }
}
