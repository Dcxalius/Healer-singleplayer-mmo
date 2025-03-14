using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project_1.Items
{
    internal class Item
    {
        public enum Quality
        {
            Trash,
            Common,
            Uncommon,
            Rare,
            Epic,
            Legendary
        }

        public int ID { get => itemData.ID; }
        public int Count { get => count; set => count = value; }
        [JsonIgnore]
        public int MaxStack { get => itemData.MaxStack; }
        [JsonIgnore]
        public GfxPath GfxPath { get => itemData.GfxPath; }

        [JsonIgnore]
        public ItemData.ItemType ItemType { get => itemData.Type; }
        
        [JsonIgnore]
        public string Name { get => itemData.Name; }
        [JsonIgnore]
        public string Description { get => itemData.Description; }
        [JsonIgnore]
        public Color ItemQualityColor
        {
            get
            {
                switch (ItemQuality)
                {
                    case Quality.Trash:
                        return Color.LightGray;
                    case Quality.Common:
                        return Color.White;
                    case Quality.Uncommon:
                        return Color.Green;
                    case Quality.Rare:
                        return Color.Blue;
                    case Quality.Epic:
                        return Color.Purple;
                    case Quality.Legendary:
                        return Color.Orange;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        [JsonIgnore]
        public Quality ItemQuality => itemData.Quality;



        protected ItemData itemData;
        int count;

        [JsonConstructor]
        public Item(int id, int count)
        {
            itemData = ItemFactory.GetItemData(id);
            this.count = count;
        }

        public Item(ItemData aData, int aCount)
        {
            Debug.Assert(aCount > 0, "Tried to add 0 counts of an item");
            itemData = aData;
            count = aCount;
        }

        public Item(LootData aLoot)
        {
            itemData = aLoot.ItemData;
            count = RandomManager.RollInt(aLoot.MinCount, aLoot.MaxCount);
        }

        public int AddToStack(int aCount)
        {
            if (itemData.MaxStack == count)
            {
                return aCount;
            }
            
            if (aCount + count <= itemData.MaxStack)
            {
                count += aCount;
                return 0;
            }

            int nr = (itemData.MaxStack - count);
            count = itemData.MaxStack;
            return nr;
        }

        public bool RemoveFromStack(int aCount)
        {
            if (count >= aCount)
            {
                count -= aCount;
                return true;
            }

            return false;
            
        }
    }
}
