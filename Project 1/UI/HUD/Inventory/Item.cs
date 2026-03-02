using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Items;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.HUD.Windows;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Diagnostics;

namespace Project_1.UI.HUD.Inventory
{
    internal class Item : GFXButton
    {
        bool isHeld;
        bool isEmpty = true;
        readonly bool holdable;
        ItemUiSnapshot snapshot;

        public bool IsEmpty => isEmpty;
        public (int, int) Index => (bagIndex, slotIndex); //For bagslots -1 0 is default, unmovable bag, and then -1 1 for first movable bag and so on
        public int bagIndex; //BagIndex 0 and above is the inventory slots, -1 is for the slots for the bags themselves, -2 is for lootwindow, -3 is for equipped
        public int slotIndex;

        static int? GetInspectTargetRenderId()
        {
            if (!Window.IsWindowOpen(nameof(InspectWindow)))
            {
                return null;
            }

            return InspectWindow.CurrentTargetRenderId;
        }

        static bool IsCharacterWindowOpen() => Window.IsWindowOpen(nameof(CharacterWindow));
        static bool IsShopOpen() => Window.IsWindowOpen(nameof(ShopWindow));

        public string ItemCount
        {
            get => itemCount.Value;
            set => itemCount.Value = value;
        }

        protected Text itemCount;

        public Item(int aBagIndex, int aSlotIndex, bool aHoldable, ItemUiSnapshot itemSnapshot, RelativeScreenPosition aPos, RelativeScreenPosition aSize)
            : this(
                aBagIndex,
                aSlotIndex,
                aHoldable,
                itemSnapshot.HasValue ? itemSnapshot.QualityColor : Color.DarkGray,
                itemSnapshot.HasValue ? itemSnapshot.GfxPath : new GfxPath(GfxType.Item, null),
                aPos,
                aSize)
        {
            AssignItem(itemSnapshot);
        }

        public Item(int aBagIndex, int aSlotIndex, bool aHoldable, Color aBackgroundColor, GfxPath aPath, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aPath, aPos, aSize, aBackgroundColor) //TODO: Change this so a nulled path isn't required and figure out what to do with colors.
        {
            bagIndex = aBagIndex;
            slotIndex = aSlotIndex;
            if (aPath.Name != null) isEmpty = false;
            itemCount = new Text("Gloryse");
            holdable = aHoldable;
            usesPressedGfx = false;
            snapshot = ItemUiSnapshot.Empty;
        }

        public void AssignItem(in ItemUiSnapshot aSnapshot)
        {
            if (!aSnapshot.HasValue)
            {
                RemoveItem();
                return;
            }

            snapshot = aSnapshot;
            imageOnButton.SetImage(aSnapshot.GfxPath);
            isEmpty = false;
            Color = aSnapshot.QualityColor;
            if (aSnapshot.MaxStack <= 1)
            {
                itemCount.Value = null;
                return;
            }

            itemCount.Value = aSnapshot.Count.ToString();
        }

        public void RemoveItem()
        {
            imageOnButton.ClearImage();
            isEmpty = true;
            itemCount.Value = null;
            Color = Color.DarkGray;
            snapshot = ItemUiSnapshot.Empty;
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
            if (!snapshot.HasValue)
            {
                Color = Color.DarkGray;
                return;
            }

            Color = snapshot.QualityColor;
        }

        public override void ReleaseOnMe(ReleaseEvent aRelease)
        {
            base.ReleaseOnMe(aRelease);
            ItemDroppedOnMe(aRelease);
        }

        void ItemDroppedOnMe(ReleaseEvent aRelease)
        {
            if (aRelease.Creator == null) return;
            if (!(aRelease.Creator.GetType().IsSubclassOf(GetType()) || aRelease.Creator.GetType() == GetType())) return;

            Item droppedOnMe = aRelease.Creator as Item;
            if (droppedOnMe == null) return;

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
                if (!snapshot.HasValue)
                {
                    Mailboxes.PublishSimCommand(new InventoryUnequipBagRequested(aItemDroppedOnMe.slotIndex, Index));
                    return true;
                }

                //Swap bags if dropped on bag no?
                return true;
            }

            if (bagIndex == -1) //Onto bagrack
            {
                Mailboxes.PublishSimCommand(new InventorySwapBagSlotsRequested(aItemDroppedOnMe.slotIndex, slotIndex));
                return true;
            }

            throw new NotImplementedException();
        }

        bool ToBagRack(Item aItemDroppedOnMe)
        {
            if (bagIndex != -1) return false;
            if (aItemDroppedOnMe.bagIndex == -2) return true; //Drop from loot
            if (!aItemDroppedOnMe.snapshot.HasValue || aItemDroppedOnMe.snapshot.ItemType != ItemData.ItemType.Container) return true; //Dropped is not bag

            Mailboxes.PublishSimCommand(new InventorySwapBagsRequested(aItemDroppedOnMe.Index, slotIndex));
            return true;
        }

        bool FromLoot(Item aItemDroppedOnMe)
        {
            if (aItemDroppedOnMe.bagIndex != -2) return false;

            Mailboxes.PublishSimCommand(new LootItemRequested(aItemDroppedOnMe.slotIndex, Index));
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
            if (!aItemDroppedOnMe.snapshot.IsEquipmentLike) return true;

            if (bagIndex == -3)
            {
                if (!aItemDroppedOnMe.snapshot.FitsInSlot((GameObjects.Unit.Equipment.Slot)slotIndex)) return true;
                if (!snapshot.HasValue)
                {
                    Mailboxes.PublishSimCommand(new EquipmentSwapRequested(aItemDroppedOnMe.slotIndex, slotIndex, null));
                    return true;
                }

                if (!snapshot.IsEquipmentLike) return true;
                if (aItemDroppedOnMe.snapshot.EquipmentType != snapshot.EquipmentType) return true;
                if (aItemDroppedOnMe.snapshot.IsMainHandRestrictedType) return true;
                if (snapshot.IsMainHandRestrictedType) return true;
                Mailboxes.PublishSimCommand(new EquipmentSwapRequested(aItemDroppedOnMe.slotIndex, slotIndex, null));
                return true;
            }

            if (bagIndex >= 0)
            {
                if (!snapshot.HasValue)
                {
                    Mailboxes.PublishSimCommand(new EquipmentMoveToInventoryRequested(aItemDroppedOnMe.slotIndex, Index, null));
                    return true;
                }

                if (!snapshot.IsEquipmentLike) return true;
                if (!snapshot.FitsInSlot((GameObjects.Unit.Equipment.Slot)aItemDroppedOnMe.slotIndex)) return true;
                Mailboxes.PublishSimCommand(new EquipmentMoveToInventoryRequested(aItemDroppedOnMe.slotIndex, Index, null));
                return true;
            }

            return true; //TODO: If dropped on gear for the same slot it should equip it if dropped on anything else it should dequip it
        }

        bool ToCharacterPane(Item aItemDroppedOnMe)
        {
            if (bagIndex != -3) return false;
            Mailboxes.PublishSimCommand(new InventorySwapEquipmentRequested(aItemDroppedOnMe.Index, slotIndex, null));
            //TODO: Handle if trying to drag inbetween sheets.
            return true;
        }

        bool FromGuildMemberCharacterPane(Item aItemDroppedOnMe)
        {
            if (aItemDroppedOnMe.bagIndex != -4) return false;
            if (!aItemDroppedOnMe.snapshot.IsEquipmentLike) return true;

            int? inspectTargetRenderId = GetInspectTargetRenderId();
            if (!inspectTargetRenderId.HasValue) return true;
            if (bagIndex == -4)
            {
                if (!aItemDroppedOnMe.snapshot.FitsInSlot((GameObjects.Unit.Equipment.Slot)slotIndex)) return true;
                if (!snapshot.HasValue)
                {
                    Mailboxes.PublishSimCommand(new EquipmentSwapRequested(aItemDroppedOnMe.slotIndex, slotIndex, inspectTargetRenderId.Value));
                    return true;
                }

                if (!snapshot.IsEquipmentLike) return true;
                if (aItemDroppedOnMe.snapshot.EquipmentType != snapshot.EquipmentType) return true;
                if (aItemDroppedOnMe.snapshot.IsMainHandRestrictedType) return true;
                if (snapshot.IsMainHandRestrictedType) return true;
                Mailboxes.PublishSimCommand(new EquipmentSwapRequested(aItemDroppedOnMe.slotIndex, slotIndex, inspectTargetRenderId.Value));
                return true;
            }

            if (bagIndex >= 0)
            {
                if (!snapshot.HasValue)
                {
                    Mailboxes.PublishSimCommand(new EquipmentMoveToInventoryRequested(aItemDroppedOnMe.slotIndex, Index, inspectTargetRenderId.Value));
                    return true;
                }

                if (!snapshot.IsEquipmentLike) return true;
                if (!snapshot.FitsInSlot((GameObjects.Unit.Equipment.Slot)aItemDroppedOnMe.slotIndex)) return true;
                Mailboxes.PublishSimCommand(new EquipmentMoveToInventoryRequested(aItemDroppedOnMe.slotIndex, Index, inspectTargetRenderId.Value));
                return true;
            }

            return true;
        }

        bool ToGuildMemberCharacterPane(Item aItemDroppedOnMe)
        {
            if (bagIndex != -4) return false;
            int? inspectTargetRenderId = GetInspectTargetRenderId();
            if (inspectTargetRenderId.HasValue)
            {
                Mailboxes.PublishSimCommand(new InventorySwapEquipmentRequested(aItemDroppedOnMe.Index, slotIndex, inspectTargetRenderId.Value));
            }
            //TODO: Handle if trying to drag inbetween sheets.
            return true;
        }

        bool InventoryToInventory(Item aItemDroppedOnMe)
        {
            Mailboxes.PublishSimCommand(new InventorySwapItemsRequested(aItemDroppedOnMe.Index, Index));
            return true;
        }

        protected override void ClickedOnMe(ClickEvent aClick)
        {
            base.ClickedOnMe(aClick);
            if (aClick.ButtonPressed != InputManager.ClickType.Left) return;

            if (!isEmpty && holdable)
            {
                Mailboxes.PublishUiEvent(new DescriptorBoxClear());
                Mailboxes.PublishUiEvent(new HeldItemStart(UiElementId, UiMouseStateCache.Absolute - Location));
            }
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            if (!isEmpty && !isHeld && holdable && heldEvents.ClickThatCreated == InputManager.ClickType.Right)
            {
                RightClickedItem();
            }

            if (!isEmpty && holdable && heldEvents.ClickThatCreated == InputManager.ClickType.Left)
            {
                Mailboxes.PublishUiEvent(new DescriptorBoxClear());
                Mailboxes.PublishUiEvent(new HeldItemEnd());
            }

            base.ClickedOnAndReleasedOnMe();
        }

        protected override void HoldReleaseAwayFromMe()
        {
            if (!isEmpty && holdable && heldEvents.ClickThatCreated == InputManager.ClickType.Left)
            {
                UiInputBridge.PublishRelease(this, heldEvents.ClickThatCreated);
                Mailboxes.PublishUiEvent(new DescriptorBoxClear());
                Mailboxes.PublishUiEvent(new HeldItemEnd());
            }
            base.HoldReleaseAwayFromMe();
        }

        protected virtual void RightClickedItem()
        {
            if (bagIndex >= 0)
            {
                bool shopOpen = IsShopOpen();
                if (shopOpen)
                {
                    //TODO: Add refund system instead of direct deletion.
                }

                int? inspectTargetRenderId = GetInspectTargetRenderId();
                int? targetRenderId = !inspectTargetRenderId.HasValue || IsCharacterWindowOpen()
                    ? null
                    : inspectTargetRenderId.Value;

                if (!snapshot.HasValue) return;
                switch (snapshot.ItemType)
                {
                    case ItemData.ItemType.NotSet:
                        return;
                    case ItemData.ItemType.Container:
                        Mailboxes.PublishSimCommand(new InventoryEquipBagRequested(Index));
                        return;
                    case ItemData.ItemType.Trash:
                        return;
                    case ItemData.ItemType.Consumable:
                        Mailboxes.PublishSimCommand(new InventoryConsumeRequested(Index, targetRenderId));
                        return;
                    case ItemData.ItemType.Equipment:
                    case ItemData.ItemType.Weapon:
                        Mailboxes.PublishSimCommand(new InventoryEquipRequested(Index, targetRenderId));
                        return;
                    default:
                        throw new NotImplementedException();
                }
            }

            if (bagIndex == -1)
            {
                Mailboxes.PublishSimCommand(new InventoryUnequipBagRequested(slotIndex, null));
                return;
            }

            if (bagIndex == -2)
            {
                Mailboxes.PublishSimCommand(new LootItemRequested(slotIndex, null));
                return;
            }
        }

        protected override void OnHover()
        {
            base.OnHover();
            if (!Visible || !holdable || !snapshot.HasValue || !snapshot.HasDescriptor) return;
            Mailboxes.PublishUiEvent(new DescriptorBoxSet(snapshot.Descriptor));
        }

        protected override void OnDeHover()
        {
            base.OnDeHover();
            HideDescriptorBox();
        }

        protected void HideDescriptorBox()
        {
            if (!holdable) return;
            Mailboxes.PublishUiEvent(new DescriptorBoxClear());
        }

        public override void Rescale()
        {
            base.Rescale();
            itemCount.Rescale();
        }

        public override void Draw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            base.Draw(aBatch);
            itemCount.CentreRightDraw(aBatch, new AbsoluteScreenPosition(AbsolutePos.Location + AbsolutePos.Size) - new AbsoluteScreenPosition(0, (int)itemCount.Offset.Y / 2));
        }
    }
}
