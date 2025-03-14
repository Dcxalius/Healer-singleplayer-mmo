using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items.SubTypes
{
    internal class ContainerData : ItemData
    {
        public int SlotCount { get => slotCount; }
        int slotCount;
        [JsonConstructor]
        public ContainerData(int id, string gfxName, string name, string description, int slotCount, Item.Quality quality) : base(id, gfxName, name, description, 1, ItemType.Container, quality)
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
