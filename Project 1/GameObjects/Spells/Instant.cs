using Newtonsoft.Json;
using Project_1.GameObjects.Entities;
using Project_1.Managers;
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

        int RandomValue
        {
            get
            {
                if (minValue == maxValue) return minValue;
                return RandomManager.RollInt(minValue, maxValue);
            }
        }

        int minValue;
        int maxValue;
        Type type 
        {
            get
            {
                if (minValue > 0)
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
        public Instant(string name, int minValue, int maxValue) : base(name)
        {
            Debug.Assert(maxValue != 0, "Tried to make effect with no effect.");
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public override bool Trigger(Entity aCaster, Entity aTarget)
        {
            if (type == Type.Attack)
            {
                aTarget.TakeDamage(aCaster, RandomValue);
                return true;
            }

            if (type == Type.Heal)
            {
                aTarget.TakeHealing(aCaster, RandomValue);
                return true;

            }
            return false;
        }
    }
}
