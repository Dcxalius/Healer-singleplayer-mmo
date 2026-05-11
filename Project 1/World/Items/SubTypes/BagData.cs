using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items.SubTypes
{
    internal class BagData : ItemData
    {
        public int SlotCount { get => slotCount; }
        int slotCount;
        [JsonConstructor]
        public BagData(int id, string gfxName, string name, string description, int slotCount, int cost, Item.Quality quality, int itemLevel = 1, string[] tags = null) : base(id, gfxName, name, description, 1, ItemType.Bag, quality, cost, itemLevel, tags)
        {
            this.slotCount = slotCount;
            Assert();
        }

        void Assert()
        {

            Debug.Assert(slotCount > 0);
        }
    }
}
