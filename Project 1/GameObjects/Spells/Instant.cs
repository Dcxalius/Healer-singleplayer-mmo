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
        Type type 
        {
            get
            {
                if (value > 0)
                {
                    return Type.Heal;
                }
                else
                {
                    return Type.Attack;
                }
            }
        }

        [JsonConstructor]
        public Instant(string name, int value) : base(name)
        {
            Debug.Assert(value != 0, "Tried to make effect with no effect.");
            this.value = value;
            
        }

        public override bool Trigger(Entity aCaster, Entity aTarget)
        {
            if (type == Type.Attack)
            {
                aTarget.TakeDamage(aCaster, value);
                return true;
            }

            if (type == Type.Heal)
            {
                aTarget.TakeHealing(aCaster, value);
                return true;

            }
            return false;
        }
    }
}
