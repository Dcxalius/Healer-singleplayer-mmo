using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.Input;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
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

        RelativeScreenPosition lootSize = RelativeScreenPosition.GetSquareFromX(0.05f);
        RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.025f);

        bool tooMuchLootForWindow = false; //TODO: Break this out to a interface
        float scrollValue = 0;
        int totalLoot;
        float[] originalYPos;

        public LootBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("GrayBackground", Color.NavajoWhite), aPos, aSize)
        {
            ToggleVisibilty();
            capturesScroll = true;
        }

        public Items.Item GetItem(int aIndex)
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

            ClearLoot();
            ToggleVisibilty();
            lootedCorpse = aCorpse;
            CreateLoot();
        }

        void CreateLoot()
        {
            Items.Item[] loots = lootedCorpse.Drop;
            loot = new Loot[loots.Length];
            originalYPos = new float[loot.Length];

            RelativeScreenPosition pos = RelativeScreenPosition.Zero;
            pos.X = spacing.X;
            pos.Y = spacing.Y;
            for (int i = 0; i < loot.Length; i++)
            {
                if (loots[i] != null)
                {
                    loot[i] = new Loot(i, loots[i], loots[i].GfxPath, pos, lootSize);

                }
                else
                {
                    loot[i] = new Loot(i, null, new GfxPath(GfxType.Debug, null), pos, RelativeScreenPosition.Zero);
                }
                originalYPos[i] = pos.Y;
                pos.Y += lootSize.Y;
                pos.Y += spacing.Y;
            }

            if (RelativeSize.Y < pos.Y - lootSize.Y - spacing.Y)
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

            AddChildren(loot);
        }

        public override void Update(in UIElement aParent)
        {
            base.Update(aParent);

            CheckIfShouldClose();
        }


        void UpdateLootPosition()
        {
            //for (int i = 0; i < children.Count; i++)
            {
                //if (!loot.Contains(children[i])) continue;

                //children[i].Move(new RelativeScreenPosition(children[i].RelativePos.X, originalYPos[i] - scrollValue));
            }
        }

        protected override void ScrolledOnMe(ScrollEvent aScrollEvent)
        {
            base.ScrolledOnMe(aScrollEvent);

            if (!tooMuchLootForWindow) return;

            //float lastScrollValue = scrollValue;
            scrollValue += (aScrollEvent.DirectionAndSteps * 0.01f);

            CapScroll();
            UpdateLootPosition();


        }

        void CapScroll()
        {
            if (scrollValue <= 0)
            {
                scrollValue = 0;
                return;
            }

            if (scrollValue > originalYPos.Last() + lootSize.Y + spacing.Y - RelativeSize.Y)
            {
                scrollValue = originalYPos.Last() + lootSize.Y + spacing.Y - RelativeSize.Y;
            }
        }

        void CheckIfShouldClose()
        {
            if (lootedCorpse == null) return;
            if (lootedCorpse.Despawned)
            {
                StopLoot();
                return;
            }

            if (lootedCorpse.Centre.DistanceTo(ObjectManager.Player.FeetPosition) > lootedCorpse.LootLength)
            {
                StopLoot();
                return;
            }
            CheckIfLootedAll();
        }
        void CheckIfLootedAll()
        {
            if (lootedCorpse.IsEmpty)
            {
                StopLoot();
            }
        }

        public void StopLoot()
        {
            ToggleVisibilty();
            ClearLoot();
        }

        void ClearLoot()
        {
            //children.RemoveAll(child => loot.Contains(child));
            loot = null;
            lootedCorpse = null;
        }
    }
}
