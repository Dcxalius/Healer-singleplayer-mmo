﻿using Microsoft.VisualBasic.Devices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Input;
using Project_1.Items;
using Project_1.Managers;
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

        public (int, int) Index { get => (bagIndex, slotIndex); } //For bagslots -1 0 is default, unmovable bag, and then -1 1 for first movable bag and so on
        public int bagIndex;
        public int slotIndex;

        protected Text itemCount;

        public Items.Item GetActualItem 
        {
            get
            {
                if (bagIndex >= 0)
                {
                    return ObjectManager.Player.Inventory.GetItemInSlot(Index);
                }

                if (bagIndex == -1)
                {
                    return ObjectManager.Player.Inventory.GetBag(slotIndex);
                }

                if (bagIndex == -2)
                {
                    return HUDManager.GetLootItem(slotIndex);
                }

                return null;
            }
        }

        public Item(int aBagIndex, int aSlotIndex, bool aHoldable, GfxPath aPath, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aPath, aPos, aSize, Color.DarkGray)
        {
            bagIndex = aBagIndex;
            slotIndex = aSlotIndex;
            if (aPath.Name != null) isEmpty = false;
            itemCount = new Text("Gloryse");
            holdable = aHoldable;
        }

        protected override void ClickedOnMe(ClickEvent aClick)
        {
            base.ClickedOnMe(aClick);

            if (aClick.ButtonPressed != InputManager.ClickType.Left) return;

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
            if (aItem.MaxStack == 1) return;
            itemCount.Value = aItem.Count.ToString();
        }

        public void RemoveItem()
        {
            gfxOnButton = null;
            isEmpty = true;
            itemCount.Value = null;
        }

        public void HoldMe()
        {
            if (!holdable) return;
            Debug.Assert(!isHeld, "Tried to hold me twice.");

            isHeld = true;
            Color = Color.LightGray;
        }

        public void ReleaseMe()
        {
            if (!holdable) return;
            Debug.Assert(isHeld, "Tried to release me without holding me");

            isHeld = false;
            Color = Color.DarkGray;
        }

        

        public override void ReleaseOnMe(ReleaseEvent aRelease)
        {
            base.ReleaseOnMe(aRelease);

            if (!(aRelease.Parent.GetType().IsSubclassOf(GetType()) || aRelease.Parent.GetType() == GetType())) return;

            Item droppedOnMe = aRelease.Parent as Item;

            if (droppedOnMe.bagIndex == -1) // From bagrack in hand
            {
                if (bagIndex >= 0) //Onto Inventory
                {
                    if (droppedOnMe.slotIndex == bagIndex) return; //Bag is tried being placed in itself
                    Items.Item i = ObjectManager.Player.Inventory.GetItemInSlot(bagIndex, slotIndex);
                    if (i == null)
                    {
                        ObjectManager.Player.Inventory.UnequipBag(droppedOnMe.slotIndex, Index);
                        return;
                    }
                    //Swap bags if dropped on bag no?
                    return;
                }

                if (bagIndex == -1) //Onto bagrack
                {
                    ObjectManager.Player.Inventory.SwapPlacesOfBags(droppedOnMe.slotIndex, slotIndex);
                    return;
                }

                throw new NotImplementedException();
            }

            if (bagIndex == -1) // Onto bagrack
            {
                if (droppedOnMe.bagIndex == -2) return; //Drop from loot

                if (ObjectManager.Player.Inventory.GetItemInSlot(droppedOnMe.Index).ItemType != ItemData.ItemType.Container) return; //Dropped is not bag

                ObjectManager.Player.Inventory.SwapBags(droppedOnMe.Index, slotIndex);
                return;
            }

            if (droppedOnMe.bagIndex == -2) //From loot
            {
                ObjectManager.Player.Inventory.LootItem(droppedOnMe.slotIndex, Index);
                return;
            }

            if (bagIndex == -2) return; //Tried to place items in loot, this isnt tibia buddy

            ObjectManager.Player.Inventory.SwapItems(droppedOnMe.Index, Index);
        }

        public override void HoldReleaseOnMe()
        {
            if (isEmpty == false && !isHeld && holdable && heldEvents.ClickThatCreated == InputManager.ClickType.Right)
            {
                RightClickedItem();
            }

            if (isEmpty == false && holdable && heldEvents.ClickThatCreated == InputManager.ClickType.Left)
            {
                HUDManager.ReleaseItem();
            }

            base.HoldReleaseOnMe();
        }

        protected override void HoldReleaseAwayFromMe()
        {


            if (isEmpty == false && holdable && heldEvents.ClickThatCreated == InputManager.ClickType.Left)
            {
                InputManager.CreateReleaseEvent(this, heldEvents.ClickThatCreated);
                HUDManager.ReleaseItem();
            }
            base.HoldReleaseAwayFromMe();

        }

        protected virtual void RightClickedItem()
        {
            if (bagIndex >= 0)
            {
                switch (ObjectManager.Player.Inventory.GetItemInSlot(Index).ItemType)
                {
                    case ItemData.ItemType.NotSet:
                        throw new NotImplementedException();
                    case ItemData.ItemType.Container:
                        ObjectManager.Player.Inventory.EquipBag(Index);
                        break;
                    case ItemData.ItemType.Trash:
                        break;
                    case ItemData.ItemType.Consumable:
                        ObjectManager.Player.Inventory.ConsumeItem(Index);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            if (bagIndex == -1)
            {
                ObjectManager.Player.Inventory.UnequipBag(slotIndex);
            }
        }

        protected override void OnHover()
        {
            base.OnHover();

            if (!Visible) return;
            if (holdable)
            {
                HUDManager.SetDescriptorBox(this);
            }
        }

        protected override void OnDeHover()
        {
            base.OnDeHover();

            if (holdable)
            {
                HUDManager.SetDescriptorBox(null);
            }
        }

        public override void Draw(SpriteBatch aBatch)
        {

            base.Draw(aBatch);
            itemCount.RightAllignedDraw(aBatch, new AbsoluteScreenPosition((AbsolutePos.Location + AbsolutePos.Size)) - new AbsoluteScreenPosition(0, (int)itemCount.Offset.Y / 2));
            itemCount.LeftAllignedDraw(aBatch, RelativePos.ToAbsoluteScreenPos());
        }
    }
}
