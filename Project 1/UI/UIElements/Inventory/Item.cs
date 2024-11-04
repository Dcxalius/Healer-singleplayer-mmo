using Microsoft.VisualBasic.Devices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects;
using Project_1.Input;
using Project_1.Items;
using Project_1.Textures;
using Project_1.UI.HUD;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Inventory
{
    internal class Item : GFXButton
    {
        bool isHeld = false;
        bool isEmpty = true;
        bool holdable;
        public bool IsEmpty { get => isEmpty; }

        (int, int) Index { get => (bagIndex, slotIndex); } //For bagslots -1 -1 is default, unmovable bag, and then -1 -2 for first movable bag and so on
        int bagIndex;
        int slotIndex;

        public Item(int aBagIndex, int aSlotIndex, bool aHoldable, GfxPath aPath, Vector2 aPos, Vector2 aSize) : base(aPath, aPos, aSize, Color.DarkGray)
        {
            bagIndex = aBagIndex;
            slotIndex = aSlotIndex;
            if (aPath.Name != null) isEmpty = false;
            holdable = aHoldable;
        }

        protected override void ClickedOnMe(ClickEvent aClick)
        {
            base.ClickedOnMe(aClick);

            if (aClick.ButtonPressed != ClickEvent.ClickType.Left) return;

            if (isEmpty == false && holdable)
            {
                HUDManager.HoldItem(this, (InputManager.GetMousePosAbsolute() - AbsolutePos.Location).ToVector2());
            }
        }

        public void AssignItem(Items.Item aItem)
        {
            if (aItem == null)
            {
                RemoveItem();
                return;
            }
            gfxOnButton = new UITexture(aItem.Gfx, Color.White);
            isEmpty = false;
        }

        public void RemoveItem()
        {
            gfxOnButton = null;
            isEmpty = true;
        }

        public void HoldMe()
        {
            if (!holdable) return;
            Debug.Assert(!isHeld, "Tried to hold me twice.");


            isHeld = true;
            Color = Color.DarkGray;
        }

        public void ReleaseMe()
        {
            if (!holdable) return;
            Debug.Assert(isHeld, "Tried to release me without holding me");
            isHeld = false;
            Color = Color.Gray;
        }

        public override void ReleaseOnMe(ReleaseEvent aRelease)
        {
            base.ReleaseOnMe(aRelease);

            if (aRelease.Parent.GetType() != GetType()) return;
            if ((aRelease.Parent as Item).Index.Item1 == -1 && Index.Item1 != -1)
            {
                Items.Item i = ObjectManager.Player.Inventory.GetItemInSlot(bagIndex, slotIndex);
                if (i == null)
                {
                    ObjectManager.Player.Inventory.RemoveBag();
                }
                
                
            }
            ObjectManager.Player.Inventory.SwapItems(Index, (aRelease.Parent as Item).Index);
        }

        public override void HoldReleaseOnMe()
        {
            base.HoldReleaseOnMe();

            if (isEmpty == false && holdable)
            {
                HUDManager.ReleaseItem();
            }
        }

        protected override void HoldReleaseAwayFromMe()
        {
            base.HoldReleaseAwayFromMe();

            if (isEmpty == false && holdable)
            {
                InputManager.CreateReleaseEvent(this, ReleaseEvent.ReleaseType.Left);
                HUDManager.ReleaseItem();
            }
        }

        public override void Draw(SpriteBatch aBatch)
        {

            base.Draw(aBatch);
        }
    }
}
