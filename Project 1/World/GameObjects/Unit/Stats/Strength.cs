using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.GameObjects.Unit.Classes;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class Strength : Stat
    {
        public int GetMeleeAttackPower(ClassData aClass)
        {
            if (aClass.MeleeAttackBonus != ClassData.MeleeAttackPowerBonus.Strength) return Value;
            return Value * 2;
        }

        public double BlockValue => Value / 20;

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
