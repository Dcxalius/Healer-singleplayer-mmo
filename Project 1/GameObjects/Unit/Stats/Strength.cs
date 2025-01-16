using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class Strength : Stat
    {
        public double AttackPower(ClassData aClass) => throw new NotImplementedException();
        public double BlockValue => throw new NotImplementedException();

        public Strength(int aValue) : base(aValue)
        {

        }


        public static bool operator ==(Strength lhs, Strength rhs) => lhs.Equals(rhs);
        public static bool operator !=(Strength lhs, Strength rhs) => !lhs.Equals(rhs);

        public static Strength operator +(Strength a, Strength b) => a + b.Value;
        public static Strength operator +(Strength a, int b) => new Strength(a.Value + b);

        public static Strength operator -(Strength a, Strength b) => a - b.Value;
        public static Strength operator -(Strength a, int b) => new Strength(a.Value - b);

        public static Strength operator *(Strength a, Strength b) => a * b.Value;
        public static Strength operator *(Strength a, int b) => new Strength(a.Value * b);

        public static Strength operator /(Strength a, Strength b) => a / b.Value;
        public static Strength operator /(Strength a, int b) => new Strength(a.Value / b);

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return this == null;
            if (obj.GetType() != typeof(Strength)) return false;
            Strength other = (Strength)obj;

            return Value == other.Value;
        }
    }
}
