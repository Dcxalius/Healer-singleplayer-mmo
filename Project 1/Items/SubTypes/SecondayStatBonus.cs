using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
    }
}
