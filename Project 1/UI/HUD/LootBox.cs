using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.Input;
using Project_1.Items;
using Project_1.Textures;
using Project_1.UI.HUD.Inventory;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
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
        LootDrop lootedDrop;
        Loot[] loot;
        ScrollableBox scrollableComponent;

        RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.025f);


        public LootBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("GrayBackground", Color.NavajoWhite), aPos, aSize)
        {
            ToggleVisibilty();
            RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.025f, Size);
            scrollableComponent = new ScrollableBox(3f, new UITexture("WhiteBackground", Color.White), Color.Yellow, spacing, RelativeScreenPosition.One - spacing - spacing);
            //capturesScroll = true;
            AddChild(scrollableComponent);
            Dragable = true;
            hudMoveable = false;
        }

        public Items.Item GetItem(int aIndex)
        {
            return lootedDrop.Drop[aIndex];
        }

        public void ReduceItem(int aIndex, int aCount)
        {
            int newCount = lootedDrop.Drop[aIndex].Count - aCount;
            Debug.Assert(newCount >= 0, "Tried to reduce items by more then it had.");
            if (newCount == 0)
            {
                lootedDrop.Drop[aIndex] = null;
                loot[aIndex].Hide();
                return;
            }
            lootedDrop.Drop[aIndex].Count -= aCount; //TODO: Make this not remove directly from property
        }

        public void Loot(LootDrop aDrop)
        {
            if (aDrop.IsEmpty)
            {
                return;
            }

            ClearLoot();
            lootedDrop = aDrop;
            Visible = true;
            CreateLoot();
        }

        void CreateLoot()
        {
            Items.Item[] loots = lootedDrop.Drop;
            loot = new Loot[loots.Length];

            for (int i = 0; i < loot.Length; i++)
            {
                if (loots[i] != null)
                {
                    loot[i] = new Loot(i, loots[i], loots[i].GfxPath);

                }
                else
                {
                    loot[i] = new Loot(i, null, new GfxPath(GfxType.Debug, null));
                }
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
            if (lootedDrop == null) return;
            
            if (CheckIfCorpseDespawned()) return;

            if (CheckIfOutOfRange()) return;

            CheckIfLootedAll();
        }

        bool CheckIfCorpseDespawned()
        {
            if (lootedDrop.Despawned)
            {
                StopLoot();
                return true;
            }
            return false;
        }

        bool CheckIfOutOfRange()
        {
            if (!lootedDrop.InDistance)
            {
                StopLoot();
                return true;
            }
            return false;
        }

        void CheckIfLootedAll()
        {
            if (lootedDrop.IsEmpty)
            {
                StopLoot();
            }
        }

        public void StopLoot()
        {
            Visible = false;
            ClearLoot();
        }

        void ClearLoot()
        {
            //children.RemoveAll(child => loot.Contains(child));
            loot = null;
            lootedDrop = null;
            scrollableComponent.RemoveAllScrollableElements();
        }
    }
}
