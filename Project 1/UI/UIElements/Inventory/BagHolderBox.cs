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
    internal class BagHolderBox : Box
    {
        Item defaultBag;
        Item[] bags; 

        

        public BagHolderBox(Vector2 aPos, Vector2 aSize) : base(new UITexture("WhiteBackground", Color.White), aPos, aSize)
        {
            defaultBag = new Item(-1, -1, false, new GfxPath(GfxType.Item, "DefaultBag"), InventoryBox.spacing, InventoryBox.itemSize);
            children.Add(defaultBag);
        }

        public void SetBags(Bag[] aBags)
        {
            bags = new Item[aBags.Length];
            for (int i = 0; i < aBags.Length; i++)
            {
                Vector2 pos = new Vector2((i + 1) * (InventoryBox.itemSize.X + InventoryBox.spacing.X) + InventoryBox.spacing.X, InventoryBox.spacing.Y);
                Vector2 size = InventoryBox.itemSize;

                if (aBags[i] == null)
                {
                    bags[i] = new Item(-1, -1 - i, true, new GfxPath(GfxType.Item, null), pos, size);
                }
                else
                {
                    bags[i] = new Item(-1, -1 - i, true, aBags[i].Gfx, pos , size);
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
