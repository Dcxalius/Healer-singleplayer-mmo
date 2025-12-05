using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class Armor : Stat
    {
        public float GetGetReductionPercentage(int aAttackerLevel)
        {
            if (aAttackerLevel < 60)
                return (float)((Value) / (Value + 400f + 85f * aAttackerLevel));

            return (float)(Value / (Value + 400 + 85 * (aAttackerLevel + 4.5 * (aAttackerLevel - 59))));
        }

        public Armor(int aValue) : base(aValue)
        {

        }


        public static bool operator ==(Armor lhs, Armor rhs) => lhs.Equals(rhs);
        public static bool operator !=(Armor lhs, Armor rhs) => !lhs.Equals(rhs);

        public static Armor operator +(Armor a, Armor b) => a + b.Value;
        public static Armor operator +(Armor a, int b) => new Armor(a.Value + b);

        public static Armor operator -(Armor a, Armor b) => a - b.Value;
        public static Armor operator -(Armor a, int b) => new Armor(a.Value - b);

        public static Armor operator *(Armor a, Armor b) => a * b.Value;
        public static Armor operator *(Armor a, int b) => new Armor(a.Value * b);

        public static Armor operator /(Armor a, Armor b) => a / b.Value;
        public static Armor operator /(Armor a, int b) => new Armor(a.Value / b);

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return this == null;
            if (obj.GetType() != typeof(Armor)) return false;
            Armor other = (Armor)obj;

            return Value == other.Value;
        }
    }
}
