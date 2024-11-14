using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public int ID { get => itemData.ID; }
        public int Count { get => count; set => count = value; }

        public int MaxStack { get => itemData.MaxStack; }

        public GfxPath Gfx { get => itemData.GfxPath; }

        public ItemData.ItemType ItemType { get => itemData.Type; }
        
        protected ItemData itemData;
        int count;

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

        //public void Draw(SpriteBatch aBatch, Rectangle aPos)
        //{
        //    itemData.Draw(aBatch, aPos);
        //}
    }
}
