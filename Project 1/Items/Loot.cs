using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items
{
    internal class Loot
    {
        public ItemData ItemData { get => itemData; }
        ItemData itemData;
        

        public int Weight { get => weight; }

        public int MinCount { get => countRange.Item1; }
        public int MaxCount { get => countRange.Item2; }
        (int, int) countRange;
        int weight;

        public Loot(ItemData aItemData, int aWeight, (int, int) aCountRange)
        {
            itemData = aItemData;
            weight = aWeight;
            countRange = aCountRange;
        }
    }
}
