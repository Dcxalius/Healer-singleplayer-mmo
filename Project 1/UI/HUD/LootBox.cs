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
    internal class LootBox : Box //TODO: Make this inherit from scrollable
    {
        Corpse lootedCorpse;
        Loot[] loot;
        ScrollableBox scrollableComponent;

        RelativeScreenPosition lootSize = RelativeScreenPosition.GetSquareFromX(0.05f);
        RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.025f);

        int totalLoot;
        float[] originalYPos;

        public LootBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("GrayBackground", Color.NavajoWhite), aPos, aSize)
        {
            ToggleVisibilty();
            RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.025f);
            scrollableComponent = new ScrollableBox(new UITexture("WhiteBackground", Color.White), Color.Yellow, spacing, aSize - spacing - spacing);
            //capturesScroll = true;
            AddChild(scrollableComponent);
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

            scrollableComponent.AddScrollableElements(loot);
        }

        public override void Update()
        {
            base.Update();

            CheckIfShouldClose();
        }

        void CheckIfShouldClose()
        {
            if (lootedCorpse == null) return;
            
            if (CheckIfCorpseDespawned()) return;

            if (CheckIfOutOfRange()) return;

            CheckIfLootedAll();
        }

        bool CheckIfCorpseDespawned()
        {
            if (lootedCorpse.Despawned)
            {
                StopLoot();
                return true;
            }
            return false;
        }

        bool CheckIfOutOfRange()
        {
            if (lootedCorpse.Centre.DistanceTo(ObjectManager.Player.FeetPosition) > lootedCorpse.LootLength)
            {
                StopLoot();
                return true;
            }
            return false;
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
            scrollableComponent.RemoveAllScrollableElements();
        }
    }
}
