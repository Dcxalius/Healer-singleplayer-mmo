using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items.SubTypes
{
    internal class Container : Item //TODO: Change Name this should be bag or similar, container should be an item you can right click to open and loot like clams
    {
        [JsonIgnore]
        public int SlotCount { get => (itemData as ContainerData).SlotCount; }

        public Container(ContainerData aData) : base(aData, 1)
        {
        }

        public Container(LootData aLoot) : base(aLoot)
        {

        }
    }
}
