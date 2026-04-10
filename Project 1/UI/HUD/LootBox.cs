using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Messaging.Events;
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
        LootContext context;
        Loot[] loot; //TODO: Should be removed
        ScrollableBox<Loot> scrollableComponent;

        RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.025f);


        public LootBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(null, new UITexture("GrayBackground", Color.NavajoWhite), aPos, aSize)
        {
            ToggleVisibilty();
            RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.025f, Size);
            scrollableComponent = new ScrollableBox<Loot>(this, 3f, new UITexture("WhiteBackground", Color.White), Color.Yellow, spacing, RelativeScreenPosition.One - spacing - spacing);
            //capturesScroll = true;
            Dragable = true;
            hudMoveable = false;
        }

        public void RefreshSlot(int slot, ItemUiSnapshot snapshot)
        {
            if (loot == null) return;
            if (slot < 0 || slot >= loot.Length) return;
            loot[slot]?.UpdateItem(snapshot);
        }

        public void Loot(LootContext lootContext, ItemUiSnapshot[] snapshot)
        {
            if (snapshot == null || snapshot.Length == 0 || snapshot.All(item => !item.HasValue))
            {
                return;
            }

            ClearLoot();
            context = lootContext;
            Visible = true;
            CreateLoot(snapshot);
        }

        void CreateLoot(ItemUiSnapshot[] snapshot)
        {
            loot = new Loot[snapshot.Length];
            List<int> indexToHide = new List<int>(); //TODO: Make this not hideous
            for (int i = 0; i < loot.Length; i++)
            {
                ItemUiSnapshot snap = snapshot[i];
                if (snap.HasValue)
                {
                    loot[i] = new Loot(scrollableComponent, i, snap);
                }
                else
                {
                    loot[i] = new Loot(scrollableComponent, i, ItemUiSnapshot.Empty);
                    indexToHide.Add(i);
                }
            }

            scrollableComponent.AddScrollableElements(loot);
            for (int i = 0; i < indexToHide.Count; i++)
            {
                loot[indexToHide[i]].Visible = false;
            }
            scrollableComponent.SetScrollValue(0f);
        }

        public void CloseIfContext(int id)
        {
            if (context.Id == id)
            {
                StopLoot();
            }
        }

        public void StopLoot()
        {
            Visible = false;
            ClearLoot();
            Managers.HUDManager.InvalidateUi();
        }

        void ClearLoot()
        {
            //children.RemoveAll(child => loot.Contains(child));
            loot = null;
            context = default;
            scrollableComponent.RemoveAllScrollableElements();
        }
    }
}
