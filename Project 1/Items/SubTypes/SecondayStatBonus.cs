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
            lhs.value = (T)(object)(lhs.value is null ? 0 : (int)(object)lhs.value + (int)(object)rhs);
            return lhs;
        }

        public static SecondayStatBonus<T> operator+ (SecondayStatBonus<T> lhs, SecondayStatBonus<T> rhs) 
        {

            Debug.Assert(lhs.SecondaryStat ==  rhs.secondaryStat);
            Debug.Assert(lhs.value.GetType() == rhs.value.GetType());
            
            switch (lhs.SecondaryStat)
            {
                case "SpellDamage":
                case "SpellFlatPenetration":
                    lhs.value = (T)(object)(lhs.value is null ? 0 : (int)(object)lhs.value + (int)(object)rhs.value);
                    break;
                case "SpellCritChance":
                case "SpellCritDamage":
                case "SpellPercentPenetration":
                case "SpellHaste":
                case "SpellVampirism":
                case "SpellBonusHitChance":
                    lhs.value = (T)(object)(lhs.value is null ? 0 : (double)(object)lhs.value + (double)(object)rhs.value);
                    break;
            }
            
            

            return lhs;
        }
    }
}
