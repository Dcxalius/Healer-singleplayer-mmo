using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class Stamina : Stat
    {
        public double HealthBonus => Value * 10;

        public Stamina(int aValue) : base(aValue)
        {
        }

        public static bool operator ==(Stamina lhs, Stamina rhs) => lhs.Equals(rhs);
        public static bool operator !=(Stamina lhs, Stamina rhs) => !lhs.Equals(rhs);

        public static Stamina operator +(Stamina a, Stamina b) => a + b.Value;
        public static Stamina operator +(Stamina a, int b) => new Stamina(a.Value + b);

        public static Stamina operator -(Stamina a, Stamina b) => a - b.Value;
        public static Stamina operator -(Stamina a, int b) => new Stamina(a.Value - b);

        public static Stamina operator *(Stamina a, Stamina b) => a * b.Value;
        public static Stamina operator *(Stamina a, int b) => new Stamina(a.Value * b);

        public static Stamina operator /(Stamina a, Stamina b) => a / b.Value;
        public static Stamina operator /(Stamina a, int b) => new Stamina(a.Value / b);

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return this == null;
            if (obj.GetType() != typeof(Stamina)) return false;
            Stamina other = (Stamina)obj;

            return Value == other.Value;
        }
    }
}
