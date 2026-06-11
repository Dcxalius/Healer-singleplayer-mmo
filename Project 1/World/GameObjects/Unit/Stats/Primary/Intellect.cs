using Project_1.GameObjects.Unit.Classes;
using Project_1.World.GameObjects.Unit.Stats.Primary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class Intellect : Stat
    {
        //TODO: Weaponchance skillup increase
        /// <summary>
        /// Returns the Mana granted by the current Intellect
        /// </summary>
        public double ManaBonus => Value * 15;
        /// <summary>
        /// Returns the Spell crit chance based on a scalar in class and the current Intellect value.
        /// </summary>
        /// <param name="aClass"></param>
        /// <returns></returns>
        public double GetCastCritChance(ClassData aClass)
        {
            return aClass.SpellCritChanceScaler * Value; //Design: Give melee crit from int as well?
        }

        public Intellect(int aValue) : base(aValue)
        {
        }

        public static bool operator ==(Intellect lhs, Intellect rhs) => lhs.Equals(rhs);
        public static bool operator !=(Intellect lhs, Intellect rhs) => !lhs.Equals(rhs);

        public static Intellect operator +(Intellect a, Intellect b) => a + b.Value;
        public static Intellect operator +(Intellect a, int b) => new Intellect(a.Value + b);

        public static Intellect operator -(Intellect a, Intellect b) => a - b.Value;
        public static Intellect operator -(Intellect a, int b) => new Intellect(a.Value - b);

        public static Intellect operator *(Intellect a, Intellect b) => a * b.Value;
        public static Intellect operator *(Intellect a, int b) => new Intellect(a.Value * b);

        public static Intellect operator /(Intellect a, Intellect b) => a / b.Value;
        public static Intellect operator /(Intellect a, int b) => new Intellect(a.Value / b);

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return this == null;
            if (obj.GetType() != typeof(Intellect)) return false;
            Intellect other = (Intellect)obj;

            return Value == other.Value;
        }
    }
}
