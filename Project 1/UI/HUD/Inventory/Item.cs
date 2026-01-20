using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.GuildMembers;
using Project_1.Input;
using Project_1.Items;
using Project_1.Items.SubTypes;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.HUD.Windows;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.Inventory
{
    internal class Item : GFXButton
    {
        bool isHeld = false;
        bool isEmpty = true;
        bool holdable;
        public bool IsEmpty { get => isEmpty; }
        Items.Item snapshot;

        public (int, int) Index { get => (bagIndex, slotIndex); } //For bagslots -1 0 is default, unmovable bag, and then -1 1 for first movable bag and so on
        public int bagIndex; //BagIndex 0 and above is the inventory slots, -1 is for the slots for the bags themselves, -2 is for lootwindow, -3 is for equipped
        public int slotIndex;

        static GuildMember GetInspectTarget()
        {
            if (!Window.IsWindowOpen(nameof(InspectWindow)))
            {
                return null;
            }
            return InspectWindow.CurrentTarget;
        }

        static bool IsCharacterWindowOpen() => Window.IsWindowOpen(nameof(CharacterWindow));
        static bool IsShopOpen() => Window.IsWindowOpen(nameof(ShopWindow));

        public string ItemCount
        {
            get => itemCount.Value;
            set => itemCount.Value = value;
        }
        protected Text itemCount;


        public Items.Item GetActualItem => snapshot;

        public Item(int aBagIndex, int aSlotIndex, bool aHoldable, Items.Item aItem, RelativeScreenPosition aPos, RelativeScreenPosition aSize)
            : this(aBagIndex, aSlotIndex, aHoldable, aItem?.ItemQualityColor ?? Color.DarkGray, aItem?.GfxPath ?? new GfxPath(GfxType.Item, null), aPos, aSize)
        {
            AssignItem(aItem);
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

        

        public void AssignItem(Items.Item aItem)
        {
            if (aItem == null)
            {
                RemoveItem();
                return;
            }

            snapshot = ItemFactory.CreateItem(aItem.ID, aItem.Count);
            imageOnButton.SetImage(aItem.GfxPath);
            isEmpty = false;
            Color = aItem.ItemQualityColor;
            if (aItem.MaxStack == 1) return;
            itemCount.Value = aItem.Count.ToString();
        }

        public void RemoveItem()
        {
            imageOnButton.ClearImage();
            isEmpty = true;
            itemCount.Value = null;
            Color = Color.DarkGray;
            snapshot = null;
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
            Items.Item item = GetActualItem;
            if(item == null)
            {
                Color = Color.DarkGray;
                return;
            }
            Color = GetActualItem.ItemQualityColor;
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
                Items.Item i = GetActualItem;
                if (i == null)
                {
                    Mailboxes.Main.Publish(new InventoryUnequipBagRequested(aItemDroppedOnMe.slotIndex, Index));
                    return true;
                }
                //Swap bags if dropped on bag no?
                return true;
            }

            if (bagIndex == -1) //Onto bagrack
            {
                Mailboxes.Main.Publish(new InventorySwapBagSlotsRequested(aItemDroppedOnMe.slotIndex, slotIndex));
                return true;
            }

            throw new NotImplementedException();
        }

        bool ToBagRack(Item aItemDroppedOnMe)
        {
            if (bagIndex != -1) return false;

            if (aItemDroppedOnMe.bagIndex == -2) return true; //Drop from loot

            Items.Item droppedItem = aItemDroppedOnMe.GetActualItem;
            if (droppedItem == null || droppedItem.ItemType != ItemData.ItemType.Container) return true; //Dropped is not bag

            Mailboxes.Main.Publish(new InventorySwapBagsRequested(aItemDroppedOnMe.Index, slotIndex));
            return true;
        }

        bool FromLoot(Item aItemDroppedOnMe)
        {
            if (aItemDroppedOnMe.bagIndex != -2) return false;

            Mailboxes.Main.Publish(new LootItemRequested(aItemDroppedOnMe.slotIndex, Index));
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
                    Mailboxes.Main.Publish(new EquipmentSwapRequested(aItemDroppedOnMe.slotIndex, slotIndex, null));
                    return true;
                }

                if (droppedItem.type != thisItem.type) return true;
                if (droppedItem.type >= Equipment.Type.MainHander) return true;
                if (thisItem.type >= Equipment.Type.MainHander) return true;
                Mailboxes.Main.Publish(new EquipmentSwapRequested(aItemDroppedOnMe.slotIndex, slotIndex, null));

                return true;
            }

            if (bagIndex >= 0)
            {
                Equipment thisItem = GetActualItem as Equipment;
                Equipment droppedItem = aItemDroppedOnMe.GetActualItem as Equipment;

                if (thisItem == null)
                {
                    Mailboxes.Main.Publish(new EquipmentMoveToInventoryRequested(aItemDroppedOnMe.slotIndex, Index, null));
                    return true;
                }

                if (!GameObjects.Unit.Equipment.FitsInSlot(thisItem.type, (GameObjects.Unit.Equipment.Slot)aItemDroppedOnMe.slotIndex)) return true;

                Mailboxes.Main.Publish(new EquipmentMoveToInventoryRequested(aItemDroppedOnMe.slotIndex, Index, null));

                return true;
            }


            return true; //TODO: If dropped on gear for the same slot it should equip it if dropped on anything else it should dequip it

        }

        bool ToCharacterPane(Item aItemDroppedOnMe)
        {
            if (bagIndex != -3) return false;
            Mailboxes.Main.Publish(new InventorySwapEquipmentRequested(aItemDroppedOnMe.Index, slotIndex, null));
            //TODO: Handle if trying to drag inbetween sheets.
            return true;
        }

        bool FromGuildMemberCharacterPane(Item aItemDroppedOnMe)
        {
            if (aItemDroppedOnMe.bagIndex != -4) return false;

            GuildMember inspectTarget = GetInspectTarget();
            if (inspectTarget == null) return true;
            Friendly openGuildPage = inspectTarget;
            if (bagIndex == -4)
            {
                Equipment thisItem = GetActualItem as Equipment;
                Equipment droppedItem = aItemDroppedOnMe.GetActualItem as Equipment;


                if (!GameObjects.Unit.Equipment.FitsInSlot(droppedItem.type, (GameObjects.Unit.Equipment.Slot)slotIndex)) return true;
                if (thisItem == null)
                {
                    Mailboxes.Main.Publish(new EquipmentSwapRequested(aItemDroppedOnMe.slotIndex, slotIndex, openGuildPage));
                    return true;
                }

                if (droppedItem.type != thisItem.type) return true;
                if (droppedItem.type >= Equipment.Type.MainHander) return true;
                if (thisItem.type >= Equipment.Type.MainHander) return true;
                Mailboxes.Main.Publish(new EquipmentSwapRequested(aItemDroppedOnMe.slotIndex, slotIndex, openGuildPage));

                return true;
            }

            if (bagIndex >= 0)
            {
                Equipment thisItem = GetActualItem as Equipment;
                Equipment droppedItem = aItemDroppedOnMe.GetActualItem as Equipment;

                if (thisItem == null)
                {
                    Mailboxes.Main.Publish(new EquipmentMoveToInventoryRequested(aItemDroppedOnMe.slotIndex, Index, openGuildPage));
                    return true;
                }

                if (!GameObjects.Unit.Equipment.FitsInSlot(thisItem.type, (GameObjects.Unit.Equipment.Slot)aItemDroppedOnMe.slotIndex)) return true;

                Mailboxes.Main.Publish(new EquipmentMoveToInventoryRequested(aItemDroppedOnMe.slotIndex, Index, openGuildPage));

                return true;
            }


            return true;
        }

        bool ToGuildMemberCharacterPane(Item aItemDroppedOnMe)
        {
            if (bagIndex != -4) return false;
            GuildMember inspectTarget = GetInspectTarget();
            if (inspectTarget != null)
            {
                Mailboxes.Main.Publish(new InventorySwapEquipmentRequested(aItemDroppedOnMe.Index, slotIndex, inspectTarget));
            }
            //TODO: Handle if trying to drag inbetween sheets.
            return true;
        }

        bool InventoryToInventory(Item aItemDroppedOnMe)
        {
            Mailboxes.Main.Publish(new InventorySwapItemsRequested(aItemDroppedOnMe.Index, Index));
            return true;
        }

        protected override void ClickedOnMe(ClickEvent aClick)
        {
            base.ClickedOnMe(aClick);

            if (aClick.ButtonPressed != InputManager.ClickType.Left) return;

            if (isEmpty == false && holdable)
            {
                Mailboxes.Ui.Publish(new DescriptorBoxClear());
                Mailboxes.Ui.Publish(new HeldItemStart(this, UiMouseStateCache.Absolute - Location));
            }
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            if (isEmpty == false && !isHeld && holdable && heldEvents.ClickThatCreated == InputManager.ClickType.Right)
            {
                RightClickedItem();
            }

            if (isEmpty == false && holdable && heldEvents.ClickThatCreated == InputManager.ClickType.Left)
            {
                Mailboxes.Ui.Publish(new DescriptorBoxClear());
                Mailboxes.Ui.Publish(new HeldItemEnd());
            }

            base.ClickedOnAndReleasedOnMe();
        }

        protected override void HoldReleaseAwayFromMe()
        {


            if (isEmpty == false && holdable && heldEvents.ClickThatCreated == InputManager.ClickType.Left)
            {
                UiInputBridge.PublishRelease(this, heldEvents.ClickThatCreated);
                Mailboxes.Ui.Publish(new DescriptorBoxClear());
                Mailboxes.Ui.Publish(new HeldItemEnd());
            }
            base.HoldReleaseAwayFromMe();

        }

        protected virtual void RightClickedItem()
        {
            if (bagIndex >= 0)
            {
                Friendly target;
                bool shopOpen = IsShopOpen();
                if (shopOpen)
                {
                    //TODO: Add refund system instead of direct deletion.
                }

                GuildMember inspectTarget = GetInspectTarget();
                if (inspectTarget == null || IsCharacterWindowOpen()) target = null;
                else target = inspectTarget;
                Items.Item actual = GetActualItem;
                if (actual == null) return;
                switch (actual.ItemType)
                {
                    case ItemData.ItemType.NotSet:
                        throw new NotImplementedException();
                    case ItemData.ItemType.Container:
                        Mailboxes.Main.Publish(new InventoryEquipBagRequested(Index));
                        return;
                    case ItemData.ItemType.Trash:
                        return;
                    case ItemData.ItemType.Consumable:
                        Mailboxes.Main.Publish(new InventoryConsumeRequested(Index, target));
                        return;
                    case ItemData.ItemType.Equipment:
                    case ItemData.ItemType.Weapon:
                        Mailboxes.Main.Publish(new InventoryEquipRequested(Index, target));
                        return;
                    default:
                        throw new NotImplementedException();
                }
            }

            if (bagIndex == -1)
            {
                Mailboxes.Main.Publish(new InventoryUnequipBagRequested(slotIndex, null));
                return;
            }

            if (bagIndex == -2)
            {
                Mailboxes.Main.Publish(new LootItemRequested(slotIndex, null));
                return;
            }
        }

        protected override void OnHover()
        {
            base.OnHover();

            if (!Visible) return;
            if (!holdable) return;
            Mailboxes.Ui.Publish(new DescriptorBoxSet(GetActualItem));
        }

        protected override void OnDeHover()
        {
            base.OnDeHover();

            HideDescriptorBox();
        }

        protected void HideDescriptorBox()
        {
            if (!holdable) return;
            Mailboxes.Ui.Publish(new DescriptorBoxClear());
        }

        public override void Rescale()
        {
            base.Rescale();
            itemCount.Rescale();
        }

        public override void Draw(SpriteBatch aBatch)
        {

            base.Draw(aBatch);
            itemCount.CentreRightDraw(aBatch, new AbsoluteScreenPosition(AbsolutePos.Location + AbsolutePos.Size) - new AbsoluteScreenPosition(0, (int)itemCount.Offset.Y / 2));
        }
    }
}
