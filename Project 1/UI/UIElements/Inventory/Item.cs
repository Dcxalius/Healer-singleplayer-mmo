using Microsoft.VisualBasic.Devices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.Input;
using Project_1.Items;
using Project_1.Items.SubTypes;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.HUD;
using Project_1.UI.UIElements.Buttons;
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

        public string ItemCount
        {
            get => itemCount.Value;
            set => itemCount.Value = value;
        }
        protected Text itemCount;


        public Items.Item GetActualItem 
        {
            get
            {
                if (bagIndex >= 0) return ObjectManager.Player.Inventory.GetItemInSlot(Index);
                
                if (bagIndex == -1) return ObjectManager.Player.Inventory.GetBag(slotIndex);
                
                if (bagIndex == -2) return HUDManager.GetLootItem(slotIndex);

                if (bagIndex == -3) return ObjectManager.Player.Equipment.EquipedInSlot((GameObjects.Unit.Equipment.Slot)slotIndex);

                if (bagIndex == -4) return HUDManager.GetGuildMemberInspectWindowTarget().Equipment.EquipedInSlot((GameObjects.Unit.Equipment.Slot)slotIndex);

                throw new NotImplementedException();
            }
        }

        public Item(int aBagIndex, int aSlotIndex, bool aHoldable, Color aBackgroundColor, GfxPath aPath, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aPath, aPos, aSize, aBackgroundColor) //TODO: Change this so a nulled path isn't required and figure out what to do with colors.
        {
            bagIndex = aBagIndex;
            slotIndex = aSlotIndex;
            if (aPath.Name != null) isEmpty = false;
            itemCount = new Text("Gloryse");
            holdable = aHoldable;
            usesPressedGfx = false;
        }

        protected override void ClickedOnMe(ClickEvent aClick)
        {
            base.ClickedOnMe(aClick);

            if (aClick.ButtonPressed != InputManager.ClickType.Left) return;

            if (isEmpty == false && holdable)
            {
                HUDManager.HoldItem(this, InputManager.GetMousePosAbsolute() - Location);
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
            Color = aItem.ItemQualityColor;
            if (aItem.MaxStack == 1) return;
            itemCount.Value = aItem.Count.ToString();
        }

        public void RemoveItem()
        {
            gfxOnButton = null;
            isEmpty = true;
            itemCount.Value = null;
            Color = Color.DarkGray;
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
            Pressed = false;
            Color = Color.DarkGray;
        }

        

        public override void ReleaseOnMe(ReleaseEvent aRelease)
        {
            base.ReleaseOnMe(aRelease);

            ItemDroppedOnMe(aRelease);
        }

        void ItemDroppedOnMe(ReleaseEvent aRelease)
        {
            if (!(aRelease.Creator.GetType().IsSubclassOf(GetType()) || aRelease.Creator.GetType() == GetType())) return;

            Item droppedOnMe = aRelease.Creator as Item;

            if (FromBagrack(droppedOnMe)) return;
            if (ToBagRack(droppedOnMe)) return;

            if (FromLoot(droppedOnMe)) return;
            if (ToLoot()) return;

            if (FromCharacterPane(droppedOnMe)) return;
            if (ToCharacterPane(droppedOnMe)) return;

            if (FromGuildMemberCharacterPane(droppedOnMe)) return;
            if (ToGuildMemberCharacterPane(droppedOnMe)) return;

            InventoryToInventory(droppedOnMe);
        }

        bool FromBagrack(Item aItemDroppedOnMe)
        {
            if (aItemDroppedOnMe.bagIndex != -1) return false;

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

        bool ToBagRack(Item aItemDroppedOnMe)
        {
            if (bagIndex != -1) return false;

            if (aItemDroppedOnMe.bagIndex == -2) return true; //Drop from loot

            if (ObjectManager.Player.Inventory.GetItemInSlot(aItemDroppedOnMe.Index).ItemType != ItemData.ItemType.Container) return true; //Dropped is not bag

            ObjectManager.Player.Inventory.SwapBags(aItemDroppedOnMe.Index, slotIndex);
            return true;
        }

        bool FromLoot(Item aItemDroppedOnMe)
        {
            if (aItemDroppedOnMe.bagIndex != -2) return false;

            ObjectManager.Player.Inventory.LootItem(aItemDroppedOnMe.slotIndex, Index);
            return true;
        }

        bool ToLoot()
        {
            if (bagIndex != -2) return false;
            return true; //Tried to place items in loot, this isnt tibia buddy
        }

        bool FromCharacterPane(Item aItemDroppedOnMe)
        {
            if (aItemDroppedOnMe.bagIndex != -3) return false;
            
            if (bagIndex == -3)
            {
                Equipment thisItem = GetActualItem as Equipment;
                Equipment droppedItem = aItemDroppedOnMe.GetActualItem as Equipment;

                if (!GameObjects.Unit.Equipment.FitsInSlot(droppedItem.type, (GameObjects.Unit.Equipment.Slot)slotIndex)) return true;
                if (thisItem == null)
                {
                    ObjectManager.Player.EquipInParticularSlot(droppedItem, (GameObjects.Unit.Equipment.Slot)slotIndex);
                    ObjectManager.Player.EquipInParticularSlot(null, (GameObjects.Unit.Equipment.Slot)aItemDroppedOnMe.slotIndex);
                    return true;
                }

                if (droppedItem.type != thisItem.type) return true;
                if (droppedItem.type >= Equipment.Type.MainHander) return true;
                if (thisItem.type >= Equipment.Type.MainHander) return true;
                ObjectManager.Player.EquipInParticularSlot(droppedItem, (GameObjects.Unit.Equipment.Slot)slotIndex);
                ObjectManager.Player.EquipInParticularSlot(thisItem, (GameObjects.Unit.Equipment.Slot)aItemDroppedOnMe.slotIndex);

                return true;
            }

            if (bagIndex >= 0)
            {
                Equipment thisItem = GetActualItem as Equipment;
                Equipment droppedItem = aItemDroppedOnMe.GetActualItem as Equipment;

                if (thisItem == null)
                {
                    ObjectManager.Player.Inventory.AddItem(droppedItem, Index);

                    ObjectManager.Player.EquipInParticularSlot(null, (GameObjects.Unit.Equipment.Slot)aItemDroppedOnMe.slotIndex);
                    return true;
                }

                if (!GameObjects.Unit.Equipment.FitsInSlot(thisItem.type, (GameObjects.Unit.Equipment.Slot)aItemDroppedOnMe.slotIndex)) return true;

                ObjectManager.Player.Inventory.AssignItem(droppedItem, Index);
                ObjectManager.Player.EquipInParticularSlot(thisItem, (GameObjects.Unit.Equipment.Slot)aItemDroppedOnMe.slotIndex);

                return true;
            }
            

            return true; //TODO: If dropped on gear for the same slot it should equip it if dropped on anything else it should dequip it

        }

        bool ToCharacterPane(Item aItemDroppedOnMe)
        {
            if (bagIndex != -3) return false;
            ObjectManager.Player.Inventory.SwapEquipment(aItemDroppedOnMe.Index, slotIndex, ObjectManager.Player);
            //TODO: Handle if trying to drag inbetween sheets.
            return true;
        }

        bool FromGuildMemberCharacterPane(Item aItemDroppedOnMe)
        {
            if (aItemDroppedOnMe.bagIndex != -4) return false;

            Entity openGuildPage = HUDManager.GetGuildMemberInspectWindowTarget();
            if (bagIndex == -4)
            {
                Equipment thisItem = GetActualItem as Equipment;
                Equipment droppedItem = aItemDroppedOnMe.GetActualItem as Equipment;


                if (!GameObjects.Unit.Equipment.FitsInSlot(droppedItem.type, (GameObjects.Unit.Equipment.Slot)slotIndex)) return true;
                if (thisItem == null)
                {
                    openGuildPage.EquipInParticularSlot(droppedItem, (GameObjects.Unit.Equipment.Slot)slotIndex);
                    openGuildPage.EquipInParticularSlot(null, (GameObjects.Unit.Equipment.Slot)aItemDroppedOnMe.slotIndex);
                    return true;
                }

                if (droppedItem.type != thisItem.type) return true;
                if (droppedItem.type >= Equipment.Type.MainHander) return true;
                if (thisItem.type >= Equipment.Type.MainHander) return true;
                openGuildPage.EquipInParticularSlot(droppedItem, (GameObjects.Unit.Equipment.Slot)slotIndex);
                openGuildPage.EquipInParticularSlot(thisItem, (GameObjects.Unit.Equipment.Slot)aItemDroppedOnMe.slotIndex);

                return true;
            }

            if (bagIndex >= 0)
            {
                Equipment thisItem = GetActualItem as Equipment;
                Equipment droppedItem = aItemDroppedOnMe.GetActualItem as Equipment;

                if (thisItem == null)
                {
                    ObjectManager.Player.Inventory.AddItem(droppedItem, Index);

                    openGuildPage.EquipInParticularSlot(null, (GameObjects.Unit.Equipment.Slot)aItemDroppedOnMe.slotIndex);
                    return true;
                }

                if (!GameObjects.Unit.Equipment.FitsInSlot(thisItem.type, (GameObjects.Unit.Equipment.Slot)aItemDroppedOnMe.slotIndex)) return true;

                ObjectManager.Player.Inventory.AssignItem(droppedItem, Index);
                openGuildPage.EquipInParticularSlot(thisItem, (GameObjects.Unit.Equipment.Slot)aItemDroppedOnMe.slotIndex);

                return true;
            }


            return true;
        }

        bool ToGuildMemberCharacterPane(Item aItemDroppedOnMe)
        {
            if (bagIndex != -4) return false;
            ObjectManager.Player.Inventory.SwapEquipment(aItemDroppedOnMe.Index, slotIndex, HUDManager.GetGuildMemberInspectWindowTarget());
            //TODO: Handle if trying to drag inbetween sheets.
            return true;
        }

        bool InventoryToInventory(Item aItemDroppedOnMe)
        {
            ObjectManager.Player.Inventory.SwapItems(aItemDroppedOnMe.Index, Index);
            return true;
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            if (isEmpty == false && !isHeld && holdable && heldEvents.ClickThatCreated == InputManager.ClickType.Right)
            {
                RightClickedItem();
            }

            if (isEmpty == false && holdable && heldEvents.ClickThatCreated == InputManager.ClickType.Left)
            {
                HUDManager.ReleaseItem();
            }

            base.ClickedOnAndReleasedOnMe();
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
                Friendly target;
                if (HUDManager.GetGuildMemberInspectWindowTarget() == null || HUDManager.PlayerCharacterPaneOpen) target = ObjectManager.Player;
                else target = HUDManager.GetGuildMemberInspectWindowTarget();
                switch (ObjectManager.Player.Inventory.GetItemInSlot(Index).ItemType)
                {
                    case ItemData.ItemType.NotSet:
                        throw new NotImplementedException();
                    case ItemData.ItemType.Container:
                        ObjectManager.Player.Inventory.EquipBag(Index);
                        return;
                    case ItemData.ItemType.Trash:
                        return;
                    case ItemData.ItemType.Consumable:
                        ObjectManager.Player.Inventory.ConsumeItem(Index, target);
                        return;
                    case ItemData.ItemType.Equipment:
                    case ItemData.ItemType.Weapon:
                        ObjectManager.Player.Inventory.Equip(Index, target);
                        return;
                    default:
                        throw new NotImplementedException();
                }
            }

            if (bagIndex == -1)
            {
                ObjectManager.Player.Inventory.UnequipBag(slotIndex);
                return;
            }

            if (bagIndex == -2)
            {
                ObjectManager.Player.Inventory.LootItem(slotIndex);
                return;
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

            HideDescriptorBox();
        }

        protected void HideDescriptorBox()
        {
            if (holdable)
            {
                HUDManager.SetDescriptorBox(null);
            }
        }

        public override void Rescale()
        {
            base.Rescale();
            itemCount.Rescale();
        }

        public override void Draw(SpriteBatch aBatch)
        {

            base.Draw(aBatch);
            itemCount.CentreRightDraw(aBatch, new AbsoluteScreenPosition((AbsolutePos.Location + AbsolutePos.Size)) - new AbsoluteScreenPosition(0, (int)itemCount.Offset.Y / 2));
            itemCount.CentreLeftDraw(aBatch, RelativePos.ToAbsoluteScreenPos());
        }
    }
}
