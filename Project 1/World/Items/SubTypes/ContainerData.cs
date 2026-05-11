using Project_1.Items;
using Project_1.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.World.Items.SubTypes
{
    internal class ContainerData : ItemData
    {
        public (int id, int weight, int min, int max)[] Drops => drops;
        (int id, int weight, int min, int max)[] drops;

        public Item[] GenerateLoot(int aHash) //TODO: Change this entire function
        {
            //Issues with this. Certain Containers should always drop one item, for example clams should always drop meat
            //Other containers shouldn't always drop one particular item
            List<Item> loot = new List<Item>();
            int totalWeight = drops.Sum(d => d.weight);
            Random rand = new Random(aHash); //TODO: Don't do this xdd, RandomManager should properly be implemented instead
            int randomValue = rand.Next(0, totalWeight);
            int cumulativeWeight = 0;
            foreach (var drop in drops)
            {
                cumulativeWeight += drop.weight;
                if (randomValue < cumulativeWeight)
                {
                    int quantity = rand.Next(drop.min, drop.max + 1);
                    ItemData itemData = ItemFactory.GetItemData(drop.id);
                    if (itemData != null)
                    {
                        loot.Add(ItemFactory.CreateItem(itemData, quantity));
                    }
                    break;
                }
            }
            return loot.ToArray();
        }

        [JsonConstructor]
        public ContainerData(int id, string gfxName, string name, string description, Item.Quality quality, int cost, (int id, int weight, int min, int max)[] drops, int itemLevel = 1, string[] tags = null) : base(id, gfxName, name, description, 1, ItemType.Container, quality, cost, itemLevel, tags)
        {
            this.drops = drops;
        }
    }
}
