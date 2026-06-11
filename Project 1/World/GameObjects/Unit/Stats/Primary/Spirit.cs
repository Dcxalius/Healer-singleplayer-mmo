using Project_1.GameObjects.Unit.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.World.GameObjects.Unit.Stats.Primary
{
    internal class Spirit : Stat
    {
        //Reminder: If Resting is implemented it should add a bonus to this value. Probably not here tho.
        public float GetHp5Bonus(ClassData aClass)
        {
            return Value * 0.2f;
        }

        public float GetMp5Bonus()
        {
            return Value * 0.125f;
        }

        public Spirit(int aValue) : base(aValue)
        {
        }

        public static bool operator ==(Spirit lhs, Spirit rhs) => lhs.Equals(rhs);
        public static bool operator !=(Spirit lhs, Spirit rhs) => !lhs.Equals(rhs);

        public static Spirit operator +(Spirit a, Spirit b) => a + b.Value;
        public static Spirit operator +(Spirit a, int b) => new Spirit(a.Value + b);

        public static Spirit operator -(Spirit a, Spirit b) => a - b.Value;
        public static Spirit operator -(Spirit a, int b) => new Spirit(a.Value - b);

        public static Spirit operator *(Spirit a, Spirit b) => a * b.Value;
        public static Spirit operator *(Spirit a, int b) => new Spirit(a.Value * b);

        public static Spirit operator /(Spirit a, Spirit b) => a / b.Value;
        public static Spirit operator /(Spirit a, int b) => new Spirit(a.Value / b);

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return this == null;
            if (obj.GetType() != typeof(Spirit)) return false;
            Spirit other = (Spirit)obj;

            return Value == other.Value;
        }
    }
}
