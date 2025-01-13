using Microsoft.VisualBasic.Devices;
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
        public int bagIndex; //BagIndex 0 and above is the inventory slots, -1 is for the slots for the bags themselves, -2 is for lootwindow, -3 is for equipped
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

        public Item(int aBagIndex, int aSlotIndex, bool aHoldable, GfxPath aPath, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aPath, aPos, aSize, Color.DarkGray) //TODO: Change this so a nulled path isn't required
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
            gfxOnButton = new UITexture(aItem.GfxPath, Color.White);
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

            if (!(aRelease.Creator.GetType().IsSubclassOf(GetType()) || aRelease.Creator.GetType() == GetType())) return;

            Item droppedOnMe = aRelease.Creator as Item;

            if (FromBagrack(droppedOnMe)) return;
            if (ToBagRack(droppedOnMe)) return;

            if (FromLoot(droppedOnMe)) return;
            if (ToLoot()) return;

            if (FromCharacterPane(droppedOnMe)) return;
            if (ToCharacterPane(droppedOnMe)) return;

            InventoryToInventory(droppedOnMe);
        }

        bool FromBagrack(Item aItemDroppedOnMe)
        {
            if (aItemDroppedOnMe.bagIndex == -1)
            {
                if (bagIndex >= 0) //Onto Inventory
                {
                    if (aItemDroppedOnMe.slotIndex == bagIndex) return true; //Bag is tried being placed in itself
                    Items.Item i = ObjectManager.Player.Inventory.GetItemInSlot(bagIndex, slotIndex);
                    if (i == null)
                    {
                        ObjectManager.Player.Inventory.UnequipBag(aItemDroppedOnMe.slotIndex, Index);
                        return true;
                    }
                    //Swap bags if dropped on bag no?
                    return true;
                }

                if (bagIndex == -1) //Onto bagrack
                {
                    ObjectManager.Player.Inventory.SwapPlacesOfBags(aItemDroppedOnMe.slotIndex, slotIndex);
                    return true;
                }

                throw new NotImplementedException();
            }

            return false;
        }

        bool ToBagRack(Item aItemDroppedOnMe)
        {
            if (bagIndex == -1) // Onto bagrack
            {
                if (aItemDroppedOnMe.bagIndex == -2) return true; //Drop from loot

                if (ObjectManager.Player.Inventory.GetItemInSlot(aItemDroppedOnMe.Index).ItemType != ItemData.ItemType.Container) return true; //Dropped is not bag

                ObjectManager.Player.Inventory.SwapBags(aItemDroppedOnMe.Index, slotIndex);
                return true;
            }
            return false;
        }

        bool FromLoot(Item aItemDroppedOnMe)
        {
            if (aItemDroppedOnMe.bagIndex == -2) //From loot
            {
                ObjectManager.Player.Inventory.LootItem(aItemDroppedOnMe.slotIndex, Index);
                return true;
            }
            return false;
        }

        bool ToLoot()
        {
            if (bagIndex == -2) return true; //Tried to place items in loot, this isnt tibia buddy
            return false;
        }
        
        bool FromCharacterPane(Item aItemDroppedOnMe)
        {
            if (aItemDroppedOnMe.bagIndex == -3)
            {
                return true; //TODO: If dropped on gear for the same slot it should equip it if dropped on anything else it should dequip it
                //Should also allow u to swap a onehander, trinket and ring to the other slot but disallow all other swaps
            }

            return false;
        }

        bool ToCharacterPane(Item aItemDroppedOnMe)
        {
            if (bagIndex == -3)
            {
                ObjectManager.Player.Inventory.SwapEquipment(aItemDroppedOnMe.Index, slotIndex);
                return true;
            }
            return false;
        }

        bool InventoryToInventory(Item aItemDroppedOnMe)
        {
            ObjectManager.Player.Inventory.SwapItems(aItemDroppedOnMe.Index, Index);
            return true;
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
                    case ItemData.ItemType.Equipment:
                    case ItemData.ItemType.Weapon:
                        ObjectManager.Player.Inventory.Equip(Index);
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
