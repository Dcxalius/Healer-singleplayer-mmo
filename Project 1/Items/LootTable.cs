using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items
{
    internal class LootTable
    {
        public int Count { get => loots.Length; }
        public Loot[] Loots { get => loots; }
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

        public Item[] GenerateDrop()
        {
            List<Item> returnable = new List<Item>();

            int count = RandomManager.RollInt(minDrops, maxDrops);
            for (int i = 0; i < count; i++)
            {
                returnable.Add(FindItemToCreate());
            }

            return returnable.ToArray();
        }

        Item FindItemToCreate()
        {
            int roll = RandomManager.RollInt(1, totalWeights);
            for (int j = 0; j < loots.Length; j++)
            {
                if (roll <= loots[j].Weight)
                {
                    return new Item(loots[j]);

                }
                roll -= loots[j].Weight;
            }

            Debug.Assert(false, "I dunnu how u got here tbh.");
            return null;
        }
    }
}
