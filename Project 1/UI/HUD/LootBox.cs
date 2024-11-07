using Microsoft.Xna.Framework;
using Project_1.GameObjects.Entities;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD
{
    internal class LootBox : Box
    {
        Item[] loot;

        public LootBox(Vector2 aPos, Vector2 aSize) : base(new UITexture("GrayBackground", Color.NavajoWhite), aPos, aSize)
        {
            visible = false;
        }

        public void Loot(Corpse aCorpse)
        {
            visible = true;
        }

        public void StopLoot()
        {
            visible = false;
        }
    }
}
