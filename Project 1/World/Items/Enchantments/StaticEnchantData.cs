using Project_1.Items.SubTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.World.Items.Enchantments
{
    internal class StaticEnchantData : EnchantmentData
    {
        //Passive effect
        public StaticEnchantData(int id, string name, string description = null, int[] primaryStats = null, SecondayStatBonus<int>[] secondaryStatsInt = null, SecondayStatBonus<float>[] secondaryStatsFloat = null) : base(id, name, description, primaryStats, secondaryStatsInt, secondaryStatsFloat)
        {
        }
    }
}
