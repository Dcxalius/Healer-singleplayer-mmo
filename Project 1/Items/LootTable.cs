using Project_1.GameObjects;
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
        public LootData[] Loots { get => loots; }
        LootData[] loots;
        int totalWeights;
        int minDrops;
        int maxDrops;

        public LootTable(LootData[] aLoot, int aMinDrops, int aMaxDrops)
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

        public LootDrop GenerateDrop(WorldObject aObject)
        {
            List<Item> returnable = new List<Item>();

            int count = RandomManager.RollInt(minDrops, maxDrops + 1);
            int[] dropCount = new int[loots.Length];
            for (int i = 0; i < count; i++)
            {
                Item drop = FindItemToCreate(ref dropCount);
                returnable.Add(drop);
            }

            return new LootDrop(returnable.ToArray(), aObject);
        }

        Item FindItemToCreate(ref int[] aDropCount)
        {
            int weights = totalWeights;
            for (int i = 0; i < aDropCount.Length; i++)
            {
                if (loots[i].MaxDrops <= aDropCount[i])
                {
                    weights -= loots[i].Weight;
                }
            }

            if (weights == 0)
            {
                return null;
            }
            int roll = RandomManager.RollInt(1, weights + 1);
            for (int i = 0; i < loots.Length; i++)
            {
                if (loots[i].MaxDrops <= aDropCount[i])
                {
                    continue;
                }

                if (roll <= loots[i].Weight)
                {
                    aDropCount[i]++;
                    return ItemFactory.CreateItem(loots[i]);

                }
                roll -= loots[i].Weight;
            }

            Debug.Assert(false, "I dunnu how u got here tbh.");
            throw new NotImplementedException();
        }
    }
}
