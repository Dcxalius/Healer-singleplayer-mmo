using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items.SubTypes
{
    internal class Bag : Item //TODO: Change Name this should be bag or similar, container should be an item you can right click to open and loot like clams
    {
        [JsonIgnore]
        public int SlotCount { get => (itemData as BagData).SlotCount; }

        [JsonConstructor]
        Bag(int id) : this(ItemFactory.GetItemData<BagData>(id)) { }

        public Bag(BagData aData) : base(aData, 1)
        {
        }

        public Bag(LootData aLoot) : base(aLoot)
        {

        }
    }
}
