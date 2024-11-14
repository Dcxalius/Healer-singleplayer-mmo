using Newtonsoft.Json;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items
{
    internal class LootData
    {
        [JsonIgnore]
        public ItemData ItemData { get => ItemFactory.GetItemData(itemName); }
        public string ItemName { get => itemName; }
        string itemName;

        public int Weight { get => weight; }
        int weight;


        [JsonIgnore]
        public int MinCount { get => countRange.Item1; }
        [JsonIgnore]
        public int MaxCount { get => countRange.Item2; }
        public (int, int) CountRange { get => countRange; }
        (int, int) countRange;

        public int MaxDrops { get => maxDrops; }
        int maxDrops;

        public LootData(string itemName, int weight, (int, int) countRange, int maxDrops)
        {
            this.itemName = itemName;
            Debug.Assert(ItemFactory.DoesDataExist(itemName), "No item by given name detected.");
            this.weight = weight;
            Debug.Assert(weight > 0, "Invalid Weight.");
            this.countRange = countRange;
            Debug.Assert(countRange.Item1 > 0 && countRange.Item1 <= countRange.Item2, "Invalid count range for loot detected.");
            this.maxDrops = maxDrops;
            Debug.Assert(maxDrops > 0, "Invalid MaxDrop.");
        }
    }
}
