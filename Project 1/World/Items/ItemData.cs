using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items
{
    internal class ItemData : IComparable<ItemData>
    {
        public enum ItemType //TODO: This should probably be removed
        {
            NotSet = 0,
            Container = 1,
            Trash = 2,
            Consumable = 3,
            Equipment = 4,
            Weapon = 5,
            Reagent = 6,
            Bag = 7
        }


        public int ID => id;
        int id;
        public string Name => name;
        string name;
        public string Description => description;
        string description;

        public int MaxStack => maxStack;
        int maxStack;

        public GfxPath GfxPath => gfx;
        GfxPath gfx;

        public Item.Quality Quality => quality;
        Item.Quality quality;

        public ItemType Type => itemType;
        ItemType itemType;

        public int RequiredLevel => Math.Min(itemLevel - 5, 60);

        public int ItemLevel => itemLevel;
        int itemLevel;

        public int Cost => cost;
        int cost;

        public string[] Tags => tags;
        string[] tags;

        public bool HasTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag) || tags == null) return false;
            return Array.IndexOf(tags, tag) >= 0;
        }

        [JsonConstructor]
        public ItemData(int id, string gfxName, string name, string description, int maxStack, ItemType itemType, Item.Quality quality, int cost, int itemLevel = 1, string[] tags = null)
        {
            this.id = id;
            gfx = new GfxPath(GfxType.Item, gfxName);
            this.name = name;
            this.description = description;
            this.maxStack = maxStack;
            this.itemType = itemType;
            this.quality = quality;
            this.cost = cost;
            this.itemLevel = itemLevel;
            this.tags = tags ?? Array.Empty<string>();
            Assert();
        }

        void Assert()
        {
            Debug.Assert(name != null && description != null && maxStack > 0 && itemType != ItemType.NotSet, "Itemdata not properly set.");
        }

        public int CompareTo(ItemData other)
        {
            if (other == null) return -1;
            if (other.id > id) return -1;
            if (other.id < id) return 1;
            return 0;
        }
    }
}
