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
        Item item;
        
        public int Weight { get => weight; }

        (int, int) countRange;
        int weight;

        public Loot(Item aItem, int aWeight, (int, int) aCountRange)
        {
            item = aItem;
            weight = aWeight;
            countRange = aCountRange;
        }
    }
}
