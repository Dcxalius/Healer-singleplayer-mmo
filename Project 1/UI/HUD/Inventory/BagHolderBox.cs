using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Items;
using Project_1.Items.SubTypes;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.HUD;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.Inventory
{
    internal class BagHolderBox : Box
    {
        Item defaultBag;
        Item[] bags;



        public BagHolderBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("WhiteBackground", Color.White), aPos, aSize)
        {
            defaultBag = new Item(-1, 0, false, Color.White, new GfxPath(GfxType.Item, "DefaultBag"), InventoryBox.BagBoxSpacing.ToAbsoluteScreenPos(InventoryBox.BagBoxSize.ToAbsoluteScreenPos(ParentSize)).ToRelativeScreenPosition(Size), InventoryBox.ItemSize.ToAbsoluteScreenPos(InventoryBox.BagBoxSize.ToAbsoluteScreenPos(ParentSize)).ToRelativeScreenPosition(Size));
            AddChild(defaultBag);
        }

        public void SetBags(ItemUiSnapshot[] aBags, AbsoluteScreenPosition aItemSize, AbsoluteScreenPosition aSpacingSize)
        {
            bags = new Item[aBags.Length - 1];
            for (int i = 1; i < aBags.Length; i++)
            {
                RelativeScreenPosition pos = new RelativeScreenPosition(i * (aItemSize.ToRelativeScreenPosition(Size).X + aSpacingSize.ToRelativeScreenPosition(Size).X) + aSpacingSize.ToRelativeScreenPosition(Size).X, aSpacingSize.ToRelativeScreenPosition(Size).Y);
                RelativeScreenPosition size = aItemSize.ToRelativeScreenPosition(Size);

                if (!aBags[i].HasValue)
                {
                    bags[i - 1] = new Item(-1, i, true, (Items.Item)null, pos, size);
                }
                else
                {
                    bags[i - 1] = new Item(-1, i, true, aBags[i], pos, size);
                }
            }

            AddChildren(bags);
        }

        public void RefreshSlot(int aSlot, Items.Inventory aInventory)
        {
            bags[aSlot - 1].AssignItem(aInventory.Bags[aSlot]);
        }

        public override void Rescale()
        {
            base.Rescale();

        }
    }
}
