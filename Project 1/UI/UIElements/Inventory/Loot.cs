using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Input;
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
    internal class Loot : Item
    {
        Items.Item item;

        public Loot(int aSlotIndex, Items.Item aItem, GfxPath aPath, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(-2, aSlotIndex, true, aPath, aPos, aSize)
        {
            if (aItem == null) return;
            item = aItem;
            if (aItem.MaxStack == 1) return;
            itemCount.Value = aItem.Count.ToString();
        }

        protected override void RightClickedItem()
        {
            ObjectManager.Player.Inventory.LootItem(slotIndex);
        }

        public void Hide()
        {
            //gfx = null;
            itemCount.Value = null;
            Resize(RelativeScreenPosition.Zero);
            item = null;
        }

        public override void ReleaseOnMe(ReleaseEvent aRelease)
        {
            //base.ReleaseOnMe(aRelease);
        }
    }
}
