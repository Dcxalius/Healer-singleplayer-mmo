using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class Agility : Stat
    {
        public int Armor => Value * 2;


        public int GetMeleeAttackPower(ClassData aClass)
        {
            if (aClass.MeleeAttackBonus != ClassData.MeleeAttackPowerBonus.Agility) return 0; 
            return Value;
        }
        public double GetMeleeCritChance(ClassData aClass) => Value * aClass.MeleeCritScaling;
        public double GetDodge(ClassData aClass) => Value * aClass.DodgeScaling;

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
