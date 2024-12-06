using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Items;
using Project_1.Textures;
using Project_1.UI.HUD;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Inventory
{
    internal class BagHolderBox : Box
    {
        Item defaultBag;
        Item[] bags; 

        

        public BagHolderBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("WhiteBackground", Color.White), aPos, aSize)
        {
            defaultBag = new Item(-1, 0, false, new GfxPath(GfxType.Item, "DefaultBag"), InventoryBox.spacing, InventoryBox.itemSize);
            children.Add(defaultBag);
        }

        public void SetBags(Container[] aBags)
        {
            bags = new Item[aBags.Length - 1];
            for (int i = 1; i < aBags.Length; i++)
            {
                RelativeScreenPosition pos = new RelativeScreenPosition((i) * (InventoryBox.itemSize.X + InventoryBox.spacing.X) + InventoryBox.spacing.X, InventoryBox.spacing.Y);
                RelativeScreenPosition size = InventoryBox.itemSize;

                if (aBags[i] == null)
                {
                    bags[i - 1] = new Item(-1, i, true, new GfxPath(GfxType.Item, null), pos, size);
                }
                else
                {
                    bags[i - 1] = new Item(-1, i, true, aBags[i].Gfx, pos , size);
                }
            }

            children.AddRange(bags);
        }

        public void RefreshSlot(int aSlot)
        {

            bags[aSlot - 1].AssignItem(ObjectManager.Player.Inventory.bags[aSlot]);
        }

        public override void Rescale()
        {
            base.Rescale();

        }
    }
}
