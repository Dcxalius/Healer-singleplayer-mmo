using Newtonsoft.Json;
using Project_1.Items;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.World.Items.SubTypes
{
    internal class Container : Item
    {
        public ContainerData ContainerData => (ContainerData)itemData;

        public int Hash => hash;
        int hash;

        bool isOpened;
        int[] lootedIndexes;

        public Item[] GetLoot()
        {
            isOpened = true;
            Item[] loot = ContainerData.GenerateLoot(hash);
            if (lootedIndexes != null)
            {
                for (int i = 0; i < loot.Length; i++)
                {
                    if (lootedIndexes.Contains(i))
                    {
                        loot[i] = null;
                    }
                }
            }
            return loot;
        }

        [JsonConstructor]
        Container(int id, int hash, bool isOpened, int[] lootedIndexes) : this(ItemFactory.GetItemData<ContainerData>(id), 1)
        {
            this.hash = hash;
            this.isOpened = isOpened;
            this.lootedIndexes = lootedIndexes;
        }

        public Container(LootData aLoot) : base(aLoot)
        {
            hash = RandomManager.RollInt();
        }

        public Container(ContainerData aData, int hash) : base(aData, 1)
        {
            this.hash = hash;
        }


        public Container(ContainerData aData) : base(aData, 1)
        {
            hash = RandomManager.RollInt();
        }
    }
}
