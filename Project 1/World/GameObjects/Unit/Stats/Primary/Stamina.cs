using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.World.GameObjects.Unit.Stats.Primary
{
    internal class Stamina : Stat
    {
        /// <summary>
        /// Returns (current stamina - 20) * 10 + 20. If stamina is less than 20 it returns stamina.
        /// </summary>
        public double HealthBonus => Value > 20 ? (Value - 20 ) * 10 + 20 : Value; //TODO: Implement racial bonuses.
        //Gnome mage, nelf druid/priest has 19 stam, so check is necessary for that particular case :P

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
