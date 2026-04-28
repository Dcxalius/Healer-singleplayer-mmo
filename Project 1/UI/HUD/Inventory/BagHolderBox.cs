using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;
using System;

namespace Project_1.UI.HUD.Inventory
{
    internal class BagHolderBox : Box
    {
        readonly Item defaultBag;
        Item[] bags = Array.Empty<Item>();

        public BagHolderBox(UI.UIElements.UIElement aParent, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, new UITexture("WhiteBackground", Color.White), aPos, aSize)
        {
            RelativeScreenPosition spacing = InventoryBox.AbsBagBoxSpacing.ToRelativeScreenPosition(Size);
            RelativeScreenPosition itemSize = InventoryBox.AbsItemSize.ToRelativeScreenPosition(Size);
            defaultBag = new Item(this, -1, 0, false, Color.White, new GfxPath(GfxType.Item, "DefaultBag"), spacing, itemSize);
        }

        public void SetBags(ItemUiSnapshot[] aBags, AbsoluteScreenPosition aItemSize, AbsoluteScreenPosition aSpacingSize)
        {
            KillAllChildren(defaultBag);
            RelativeScreenPosition relItemSize = aItemSize.ToRelativeScreenPosition(Size);
            RelativeScreenPosition relSpacing = aSpacingSize.ToRelativeScreenPosition(Size);
            defaultBag.Move(relSpacing);
            defaultBag.Resize(relItemSize);

            if (aBags == null || aBags.Length <= 1)
            {
                bags = Array.Empty<Item>();
                return;
            }

            bags = new Item[aBags.Length - 1];
            for (int i = 1; i < aBags.Length; i++)
            {
                RelativeScreenPosition pos = new RelativeScreenPosition(i * (relItemSize.X + relSpacing.X) + relSpacing.X, relSpacing.Y);
                bags[i - 1] = new Item(this, -1, i, true, aBags[i], pos, relItemSize);
            }
        }
    }
}
