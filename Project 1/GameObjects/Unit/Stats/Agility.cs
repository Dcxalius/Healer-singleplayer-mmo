using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class Agility : Stat
    {
        public double Armor => throw new NotImplementedException();
        public double CritChance => throw new NotImplementedException();
        public double AttackPower(ClassData aClass ) => throw new NotImplementedException();

        public double Dodge => throw new NotImplementedException();

        public Agility(int aValue) : base(aValue)
        {
        }


        public static bool operator ==(Agility lhs, Agility rhs) => lhs.Equals(rhs);
        public static bool operator !=(Agility lhs, Agility rhs) => !lhs.Equals(rhs);

        public static Agility operator +(Agility a, Agility b) => a + b.Value;
        public static Agility operator +(Agility a, int b) => new Agility(a.Value + b);

        public static Agility operator -(Agility a, Agility b) => a - b.Value;
        public static Agility operator -(Agility a, int b) => new Agility(a.Value - b);

        public static Agility operator *(Agility a, Agility b) => a * b.Value;
        public static Agility operator *(Agility a, int b) => new Agility(a.Value * b);

        public static Agility operator /(Agility a, Agility b) => a / b.Value;
        public static Agility operator /(Agility a, int b) => new Agility(a.Value / b);

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return this == null;
            if (obj.GetType() != typeof(Agility)) return false;
            Agility other = (Agility)obj;

            return Value == other.Value;
        }
    }
}
