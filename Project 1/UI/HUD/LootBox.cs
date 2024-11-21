using Microsoft.Xna.Framework;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.Input;
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

        bool tooMuchLootForWindow = false; //TODO: Break this out to a interface
        float scrollValue = 0;
        int totalLoot;
        float[] originalYPos;

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
            Debug.Assert(newCount >= 0, "Tried to reduce items by more then it had.");
            if (newCount == 0)
            {
                lootedCorpse.Drop[aIndex] = null;
                loot[aIndex].Hide();
                return;
            }
            lootedCorpse.Drop[aIndex].Count -= aCount; //TODO: Make this not remove directly from property
        }

        public void Loot(Corpse aCorpse)
        {
            if (aCorpse.IsEmpty)
            {
                return;
            }

            visible = true;
            lootedCorpse = aCorpse;
            CreateLoot();
        }

        void CreateLoot()
        {
            Items.Item[] loots = lootedCorpse.Drop;
            loot = new Loot[loots.Length];
            originalYPos = new float[loot.Length];

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
                originalYPos[i] = pos.Y;
                pos.Y += lootSize.Y;
                pos.Y += spacing.Y;
            }

            if (pos.Y > RelativeSize.Y - lootSize.Y - spacing.Y)
            {
                tooMuchLootForWindow = true;
                scrollValue = 0;
                totalLoot = loots.Length;
            }
            else
            {
                tooMuchLootForWindow = false;
                scrollValue = 0;
                totalLoot = 0;
            }

            children.AddRange(loot);
        }

        public override void Update(in UIElement aParent)
        {
            base.Update(aParent);


            UpdateScroll();
        }

        void UpdateScroll()
        {
            if (tooMuchLootForWindow == true)
            {
                float lastScrollValue = GetLastAndCurrentScroll();
                CapScroll();
                UpdateLootPosition(lastScrollValue);
            }
        }

        void UpdateLootPosition(float aLastScrollValue)
        {
            if (aLastScrollValue != scrollValue)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    if (loot.Contains(children[i]))
                    {
                        children[i].Move(new Vector2(children[i].RelativePos.X, originalYPos[i] - scrollValue));
                    }
                }
            }

        }

        float GetLastAndCurrentScroll()
        {
            float lastScrollValue = scrollValue;

            scrollValue += (InputManager.ScrolledSinceLastFrame / 120 / totalLoot * 0.3f); //TODO: Make this stop camera from zooming

            return lastScrollValue;
        }

        void CapScroll()
        {
            if (scrollValue < 0)
            {
                scrollValue = 0;
            }

            if (scrollValue > originalYPos.Last() + lootSize.Y + spacing.Y - RelativeSize.Y)
            {
                scrollValue = originalYPos.Last() + lootSize.Y + spacing.Y - RelativeSize.Y;
            }
        }

        void CheckIfShouldClose()
        {
            if (lootedCorpse == null)
            {
                return;
            }
            CheckIfLootedAll();
            ToFarFromCorpse();
        }
        void CheckIfLootedAll()
        {
            if (lootedCorpse.IsEmpty)
            {
                StopLoot();
            }
        }

        void ToFarFromCorpse()
        {
            if ((lootedCorpse.Centre - ObjectManager.Player.FeetPos).Length() > lootedCorpse.LootLength)
            {
                StopLoot();
            }
        }

        public void StopLoot()
        {
            visible = false;
            children.RemoveAll(child => loot.Contains(child));
            loot = null;
            lootedCorpse = null;
        }
    }
}
