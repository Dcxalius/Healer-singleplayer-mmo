using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Items;
using Project_1.Textures;
using Project_1.UI.HUD;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Inventory
{
    internal class BagBox : Box
    {
        Item defaultBag;
        Item[] bags; 

        

        public BagBox(Vector2 aPos, Vector2 aSize) : base(new UITexture("WhiteBackground", Color.White), aPos, aSize)
        {
            defaultBag = new Item(new GfxPath(GfxType.Item, "DefaultBag"), InventoryBox.spacing, InventoryBox.itemSize);
            children.Add(defaultBag);
        }

        public void SetBags(Bag[] aBags)
        {
            bags = new Item[aBags.Length];
            for (int i = 0; i < aBags.Length; i++)
            {
                Vector2 pos = (i + 1) * (InventoryBox.itemSize + InventoryBox.spacing);
                Vector2 size = InventoryBox.itemSize;

                if (aBags[i] == null)
                {
                    bags[i] = new Item(new GfxPath(GfxType.Item, null), pos, size);
                }
                else
                {
                    bags[i] = new Item(aBags[i].Gfx, pos , size);
                }
            }

            children.AddRange(bags);
        }

        public override void Rescale()
        {
            base.Rescale();

        }
    }
}
