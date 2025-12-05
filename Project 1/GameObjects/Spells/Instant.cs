using Newtonsoft.Json;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Unit.Stats;
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
                if (minValue == maxValue) return Math.Abs(minValue);
                if (type == Type.Heal)
                {
                    return RandomManager.RollInt(minValue, maxValue);
                }
                if (type == Type.Attack)
                {
                    return RandomManager.RollInt(Math.Abs(minValue), Math.Abs(maxValue));
                }
                throw new NotImplementedException();
            }
        }

        DamageType damageType;
        int minValue;
        int maxValue;
        Type type => minValue > 0 ? Type.Heal : Type.Attack;

        [JsonConstructor]
        public Instant(string name, int minValue, int maxValue, DamageType damageType) : base(name)
        {
            Debug.Assert(maxValue != 0, "Tried to make effect with no effect.");
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.damageType = damageType;
        }

        public override bool Trigger(Entity aCaster, Entity aTarget)
        {
            if (type == Type.Attack)
            {
                aTarget.RecieveSpellAttack(aCaster, Name, new Damage(RandomValue, damageType));
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
