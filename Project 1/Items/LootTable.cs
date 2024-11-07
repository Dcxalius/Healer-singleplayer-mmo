using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items
{
    internal class LootTable
    {
        Loot[] loots;
        int totalWeights;
        int minDrops;
        int maxDrops;

        public LootTable(Loot[] aLoot, int aMinDrops, int aMaxDrops)
        {
            totalWeights = 0;
            loots = aLoot;
            for (int i = 0; i < loots.Length; i++)
            {
                totalWeights += loots[i].Weight;
            }

            minDrops = aMinDrops;
            maxDrops = aMaxDrops;
        }
    }
}
