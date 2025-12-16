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
        LootContext context;
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

        public Items.Item GetItem(int aIndex) => Messaging.LootState.Peek(aIndex);

        public void RefreshSlot(int slot)
        {
            if (loot == null) return;
            if (slot < 0 || slot >= loot.Length) return;
            loot[slot]?.UpdateItem(Messaging.LootState.Peek(slot));
        }

        public void Loot(LootContext lootContext, Items.Item[] snapshot)
        {
            if (snapshot == null || snapshot.Length == 0 || snapshot.All(item => item == null))
            {
                return;
            }

            ClearLoot();
            context = lootContext;
            Visible = true;
            CreateLoot(snapshot);
        }

        void CreateLoot(Items.Item[] snapshot)
        {
            loot = new Loot[snapshot.Length];
            List<int> indexToHide = new List<int>(); //TODO: Make this not hideous
            for (int i = 0; i < loot.Length; i++)
            {
                Items.Item snap = snapshot[i];
                if (snap != null)
                {
                    loot[i] = new Loot(i, snap, snap.GfxPath);
                }
                else
                {
                    loot[i] = new Loot(i, null, new GfxPath(GfxType.Debug, null));
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

        public override void Update()
        {
            base.Update();

            CheckIfShouldClose();
        }

        void CheckIfShouldClose()
        {
            if (CheckIfOutOfRange()) return;

            CheckIfLootedAll();
        }

        bool CheckIfOutOfRange()
        {
            if (context.AllowedDistance <= 0f) return false;
            if (context.Despawned) { StopLoot(); return true; }
            if (context.Position.DistanceTo(ObjectManager.Player.FeetPosition) > context.AllowedDistance)
            {
                StopLoot();
                return true;
            }
            return false;
        }

        void CheckIfLootedAll()
        {
            if (loot == null) return;
            if (loot.All(x => x == null))
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
            context = default;
            scrollableComponent.RemoveAllScrollableElements();
        }
    }
}
