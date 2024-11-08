using Microsoft.Xna.Framework;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Inventory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD
{
    internal class LootBox : Box
    {
        Corpse lootedCorpse;
        Loot[] loot;

        Vector2 lootSize = Camera.GetRelativeSquare(0.05f);
        Vector2 spacing = Camera.GetRelativeSquare(0.025f);

        public LootBox(Vector2 aPos, Vector2 aSize) : base(new UITexture("GrayBackground", Color.NavajoWhite), aPos, aSize)
        {
            visible = false;
        }

        public Items.Item LootItem(int aIndex)
        {
            return lootedCorpse.Drop[aIndex];
        }

        public void ReduceItem(int aIndex, int aCount)
        {
            int newCount = lootedCorpse.Drop[aIndex].Count - aCount;
            Debug.Assert(newCount >= 0, "xdd");
            if (newCount == 0)
            {
                lootedCorpse.Drop[aIndex] = null;
                loot[aIndex].Hide();
                return;
            }
            lootedCorpse.Drop[aIndex].Count -= aCount;
        }

        public void Loot(Corpse aCorpse)
        {
            visible = true;
            lootedCorpse = aCorpse;
            CreateLoot();
        }

        void CreateLoot()
        {
            Items.Item[] loots = lootedCorpse.Drop;
            loot = new Loot[loots.Length];

            Vector2 pos = Vector2.Zero;
            pos.X = spacing.X;
            pos.Y = spacing.Y;
            for (int i = 0; i < loot.Length; i++)
            {
                if (loots[i] != null)
                {
                    loot[i] = new Loot(i, loots[i], loots[i].Gfx, pos, lootSize);

                }
                else
                {
                    loot[i] = new Loot(i, null, new GfxPath(GfxType.Debug, null), pos, Vector2.Zero);
                }
                pos.Y += lootSize.Y;
                pos.Y += spacing.Y;
            }
            children.AddRange(loot);
        }

        public override void Update(in UIElement aParent)
        {
            base.Update(aParent);

            ToFarFromCorpse();
        }

        void ToFarFromCorpse()
        {
            if (lootedCorpse == null)
            {
                return;
            }
            if ((lootedCorpse.Centre - ObjectManager.Player.FeetPos).Length() > lootedCorpse.LootLength)
            {
                StopLoot();
            }
        }

        public void StopLoot()
        {
            visible = false;
            loot = null;
            lootedCorpse = null;
            children.Clear();
        }
    }
}
