using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class Spirit : Stat
    {
        public double Hp5Bonus => throw new NotImplementedException();
        public double Mp5Bonus => throw new NotImplementedException();

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
