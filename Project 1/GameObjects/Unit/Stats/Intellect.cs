using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class Intellect : Stat
    {
        public double ManaBonus => Value * 15;
        public double CastCritChance => 0.01f / 60 * Value; //TODO: Give melee crit from this?

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
