using Microsoft.Xna.Framework;
using Project_1.Items;
using Project_1.Textures;
using Project_1.UI.HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Inventory
{
    internal class BagBox : Box
    {
        Item[] slots;

        public BagBox(int aSlotCount, int aColumnCount, Vector2 aPos, Vector2 aSize) : base(new UITexture("WhiteBackground", Color.Beige), aPos, aSize)
        {
            slots = new Item[aSlotCount];
            for (int i = 0; i < slots.Length; i++)
            {
                float x = ((i % aColumnCount) * ((InventoryBox.itemSize.X + InventoryBox.spacing.X)) + InventoryBox.spacing.X);
                float y = InventoryBox.spacing.Y + (InventoryBox.itemSize.Y + InventoryBox.spacing.Y) * (float)Math.Floor((double)i / aColumnCount);
                Vector2 pos = new Vector2(x, y);
                Vector2 size = InventoryBox.itemSize;

                if (slots[i] == null)
                {
                    slots[i] = new Item(new GfxPath(GfxType.Item, null), pos, size);
                }
                else
                {
                    //slots[i] = new Item(aBags[i].Gfx, pos, size);
                }
            }
            children.AddRange(slots);
        }
    }
}
