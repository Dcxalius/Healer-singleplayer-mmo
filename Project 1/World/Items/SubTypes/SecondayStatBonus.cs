using Newtonsoft.Json;
using Project_1.GameObjects.Unit.Stats;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items.SubTypes
{
    internal class SecondayStatBonus<T> 
    {
        public string SecondaryStat => secondaryStat;
        string secondaryStat;
        public T Value => value;
        T value;
        [JsonConstructor]
        public SecondayStatBonus(string secondaryStat, T value) 
        {
            this.secondaryStat = secondaryStat;
            this.value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is not SecondayStatBonus<T> ssb) return false;
            return secondaryStat == ssb.secondaryStat;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
        public static SecondayStatBonus<T> operator+ (SecondayStatBonus<T> lhs, T rhs)
        {
            Debug.Assert(lhs.value.GetType() == rhs.GetType());
            lhs.value = AddValues(lhs.value, rhs);
            return lhs;
        }

        public static SecondayStatBonus<T> operator+ (SecondayStatBonus<T> lhs, SecondayStatBonus<T> rhs) 
        {

            Debug.Assert(lhs.SecondaryStat ==  rhs.secondaryStat);
            Debug.Assert(lhs.value.GetType() == rhs.value.GetType());
            
            lhs.value = AddValues(lhs.value, rhs.value);

            return lhs;
        }

        static T AddValues(T lhs, T rhs)
        {
            if (typeof(T) == typeof(int))
            {
                int result = (int)(object)lhs + (int)(object)rhs;
                return (T)(object)result;
            }
            if (typeof(T) == typeof(float))
            {
                float result = (float)(object)lhs + (float)(object)rhs;
                return (T)(object)result;
            }
            if (typeof(T) == typeof(double))
            {
                double result = (double)(object)lhs + (double)(object)rhs;
                return (T)(object)result;
            }

            throw new NotImplementedException($"Unsupported secondary stat type: {typeof(T)}");
        }
    }
}
