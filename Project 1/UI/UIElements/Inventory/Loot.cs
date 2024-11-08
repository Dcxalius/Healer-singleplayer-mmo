using Microsoft.Xna.Framework;
using Project_1.GameObjects;
using Project_1.Input;
using Project_1.Items;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Inventory
{
    internal class Loot : Item
    {
        public Loot(int aSlotIndex, Items.Item aItem, GfxPath aPath, Vector2 aPos, Vector2 aSize) : base(-2, aSlotIndex, true, aPath, aPos, aSize)
        {
            if (aItem == null) return;
            if (aItem.MaxStack == 1) return;
            itemCount.Value = aItem.Count.ToString();
        }



        public void Hide()
        {
            //gfx = null;
            itemCount.Value = null;
            Resize(Vector2.Zero);
        }

        public override void ReleaseOnMe(ReleaseEvent aRelease)
        {
            //base.ReleaseOnMe(aRelease);
        }
    }
}
