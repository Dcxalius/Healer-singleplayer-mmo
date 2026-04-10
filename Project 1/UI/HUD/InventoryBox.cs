using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Items;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.HUD.Inventory;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using System;

namespace Project_1.UI.HUD
{
    internal class InventoryBox : Box
    {
        BagHolderBox bagHolderBox;
        BagContentBox[] bagContentBoxes;
        Label gold;
        Image goldImage;

        const float itemSizeX = 0.022f;
        const float spacingX = 0.0025f;

        public static RelativeScreenPosition ItemSize => itemSizeInBagSpace;
        static RelativeScreenPosition itemSizeInBagSpace;

        public static AbsoluteScreenPosition AbsItemSize => absItemSize;
        static AbsoluteScreenPosition absItemSize;

        public static RelativeScreenPosition BagBoxSize => bagBoxSize;
        static RelativeScreenPosition bagBoxSize;

        public static RelativeScreenPosition BagBoxSpacing => bagBoxSpacing;
        static RelativeScreenPosition bagBoxSpacing;

        public static AbsoluteScreenPosition AbsBagBoxSpacing => absBagBoxSpacing;
        static AbsoluteScreenPosition absBagBoxSpacing;

        static RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(spacingX);
        int columnCount;
        InventoryUiSnapshot latestSnapshot;
        bool hasSnapshot;

        public InventoryBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize, int aColumnCount)
            : base(null, new UITexture("WhiteBackground", new Color(80, 80, 80, 80)), aPos, new RelativeScreenPosition(0.3f, 0.4f))
        {
            InitUIElement(aColumnCount);
            InitStatics();
            InitChildren();
        }

        void InitUIElement(int aColumnCount)
        {
            columnCount = aColumnCount;
            visibleKey = KeyBindManager.KeyListner.Inventory;
            Visible = false;
            Dragable = true;
            alwaysOnScreen = true;
            hudMoveable = false;
        }

        void InitStatics()
        {
            // Keep inventory icon metrics window-relative; this avoids feedback loops when the inventory box resizes itself.
            itemSizeInBagSpace = RelativeScreenPosition.GetSquareFromX(itemSizeX);
            absItemSize = itemSizeInBagSpace.ToAbsoluteScreenPos();
            bagBoxSpacing = RelativeScreenPosition.GetSquareFromX(spacingX);
            absBagBoxSpacing = bagBoxSpacing.ToAbsoluteScreenPos();
            bagBoxSize = RelativeScreenPosition.One;
        }

        void InitChildren()
        {
            RelativeScreenPosition itemSizeInInventoryScope = AbsItemSize.ToRelativeScreenPosition(Size);
            RelativeScreenPosition bagBoxSpacingInInventoryScope = AbsBagBoxSpacing.ToRelativeScreenPosition(Size);
            RelativeScreenPosition bhPos = new RelativeScreenPosition(bagBoxSpacingInInventoryScope.X, 1f - (itemSizeInInventoryScope.Y + bagBoxSpacingInInventoryScope.Y * 3));
            RelativeScreenPosition bhSize = new RelativeScreenPosition(itemSizeInInventoryScope.X * Items.Inventory.bagSlots + bagBoxSpacingInInventoryScope.X * (Items.Inventory.bagSlots + 1), itemSizeInInventoryScope.Y + bagBoxSpacingInInventoryScope.Y * 2);

            bagHolderBox = new BagHolderBox(this, bhPos, bhSize);

            bagContentBoxes = new BagContentBox[Items.Inventory.bagSlots];
            for (int i = 0; i < bagContentBoxes.Length; i++)
            {
                bagContentBoxes[i] = new BagContentBox(this, i);
            }

            RelativeScreenPosition imgSize = RelativeScreenPosition.GetSquareFromY(bagHolderBox.RelativeSize.Y / 2, Size);
            goldImage = new Image(this, new UITexture("Gold", Color.White), RelativeScreenPosition.One - imgSize - bagBoxSpacingInInventoryScope.OnlyX, imgSize);
            RelativeScreenPosition goldSize = new RelativeScreenPosition(1 - imgSize.X - bagBoxSpacingInInventoryScope.X * 2, bhSize.Y);
            gold = new Label(this, RelativeScreenPosition.One.OnlyY - goldSize.OnlyY - bagBoxSpacingInInventoryScope.OnlyY, goldSize, Label.TextAllignment.CentreRight, aText: "0");
        }

        public void SetInventory(InventoryUiSnapshot snapshot)
        {
            latestSnapshot = snapshot;
            hasSnapshot = true;
            RebuildFromSnapshot(snapshot);
            ApplyDynamicSize(snapshot);
            LayoutElements();
        }

        void RebuildFromSnapshot(InventoryUiSnapshot snapshot)
        {
            bagHolderBox.SetBags(
                snapshot.BagItems,
                AbsItemSize,
                AbsBagBoxSpacing);

            ItemUiSnapshot[] defaultBagItems = null;
            if (snapshot.ItemsByBag != null && snapshot.ItemsByBag.Length > 0)
            {
                defaultBagItems = snapshot.ItemsByBag[0];
            }
            bagContentBoxes[0].RefreshBag(defaultBagItems, Items.Inventory.defaultSlots, columnCount);

            for (int i = 1; i < bagContentBoxes.Length; i++)
            {
                if (snapshot.ItemsByBag == null || snapshot.ItemsByBag.Length <= i || snapshot.ItemsByBag[i] == null)
                {
                    bagContentBoxes[i].Empty();
                    continue;
                }

                bagContentBoxes[i].RefreshBag(snapshot.ItemsByBag[i], snapshot.ItemsByBag[i].Length, columnCount);
            }
        }

        public void RefreshGold(int aGoldAmount) => gold.Text = aGoldAmount.ToString();

        public void RefreshSlot(int aBag, int aSlot, InventoryUiSnapshot snapshot)
        {
            latestSnapshot = snapshot;
            hasSnapshot = true;

            if (aBag < 0)
            {
                SetInventory(snapshot);
                return;
            }

            if (aBag >= bagContentBoxes.Length)
            {
                SetInventory(snapshot);
                return;
            }

            if (snapshot.ItemsByBag == null || snapshot.ItemsByBag.Length <= aBag || snapshot.ItemsByBag[aBag] == null)
            {
                bagContentBoxes[aBag].Empty();
                ApplyDynamicSize(snapshot);
                LayoutElements();
                return;
            }

            ItemUiSnapshot[] bagSnapshots = snapshot.ItemsByBag[aBag];
            if (bagContentBoxes[aBag].SlotCount != bagSnapshots.Length)
            {
                SetInventory(snapshot);
                return;
            }

            bagContentBoxes[aBag].RefreshSlot(aSlot, (aSlot >= 0 && aSlot < bagSnapshots.Length) ? bagSnapshots[aSlot] : ItemUiSnapshot.Empty);
            ApplyDynamicSize(snapshot);
            LayoutElements();
        }

        void ApplyDynamicSize(InventoryUiSnapshot snapshot)
        {
            CalculateDynamicSize();
            
            RebuildFromSnapshot(snapshot);
        }

        void CalculateDynamicSize()
        {
            RelativeScreenPosition resize = RelativeScreenPosition.Zero;

            RelativeScreenPosition outerSpacingInScreenSpace = absBagBoxSpacing.ToRelativeScreenPosition(); //I'll be honest, I dont know why this isn't in the context of the inv box
            float s = (absItemSize.ToRelativeScreenPosition() * columnCount + absBagBoxSpacing.ToRelativeScreenPosition() * (columnCount + 3)).X;
            resize.X = s;

            float bagY = outerSpacingInScreenSpace.Y;
            (AbsoluteScreenPosition, AbsoluteScreenPosition)[] oldPosAndSize = new (AbsoluteScreenPosition, AbsoluteScreenPosition)[Items.Inventory.bagSlots];
            for (int i = 0; i < bagContentBoxes.Length; i++)
            {
                if (bagContentBoxes[i].RelativeSize.Y == 0)
                {
                    continue;
                }
                oldPosAndSize[i] = (bagContentBoxes[i].RelativePos.ToAbsoluteScreenPos(Size), bagContentBoxes[i].Size);
                bagY += bagContentBoxes[i].Size.ToRelativeScreenPosition().Y + outerSpacingInScreenSpace.Y;
            }

            resize.Y = bagHolderBox.Size.ToRelativeScreenPosition().Y + outerSpacingInScreenSpace.Y + bagY;

            AbsoluteScreenPosition bagHolderAbsSize = bagHolderBox.Size;
            Resize(resize);

            bagHolderBox.Move(new RelativeScreenPosition(AbsBagBoxSpacing.ToRelativeScreenPosition(Size).X, 1f - (bagHolderAbsSize.ToRelativeScreenPosition(Size).Y + AbsBagBoxSpacing.ToRelativeScreenPosition(Size).Y)));
            bagHolderBox.Resize(bagHolderAbsSize.ToRelativeScreenPosition(Size));

            for (int i = 0; i < bagContentBoxes.Length; i++)
            {
                bagContentBoxes[i].Move(AbsBagBoxSpacing.ToRelativeScreenPosition(Size).OnlyX + oldPosAndSize[i].Item1.ToRelativeScreenPosition(Size).OnlyY);
                bagContentBoxes[i].Resize(oldPosAndSize[i].Item2.ToRelativeScreenPosition(Size));

            }

            goldImage.Resize(RelativeScreenPosition.GetSquareFromY(bagHolderBox.RelativeSize.Y / 2, Size));
            //goldImage.Move(RelativeScreenPosition.One - goldImage.RelativeSize - outerSpacingInScreenSpace.OnlyX);
            goldImage.Move(RelativeScreenPosition.One - bagHolderBox.RelativeSize.OnlyY / 2 - goldImage.RelativeSize.OnlyY / 2 - goldImage.RelativeSize.OnlyX - outerSpacingInScreenSpace.OnlyX);
            //


            gold.Resize(new RelativeScreenPosition(1 - goldImage.RelativeSize.X - outerSpacingInScreenSpace.X * 2, bagHolderBox.RelativeSize.Y));
            gold.Move(new RelativeScreenPosition(RelativeScreenPosition.Zero + RelativeScreenPosition.One.OnlyY - gold.RelativeSize.OnlyY - outerSpacingInScreenSpace.OnlyY));
        }

        void LayoutElements()
        {
            RelativeScreenPosition spacingInScope = AbsBagBoxSpacing.ToRelativeScreenPosition(Size);
            float nextY = spacingInScope.Y;
            for (int i = 0; i < bagContentBoxes.Length; i++)
            {
                if (bagContentBoxes[i].RelativeSize.Y <= 0f)
                {
                    continue;
                }

                bagContentBoxes[i].Move(new RelativeScreenPosition(spacingInScope.X, nextY));
                nextY += bagContentBoxes[i].RelativeSize.Y + spacingInScope.Y;
            }

            bagHolderBox.Move(new RelativeScreenPosition(spacingInScope.X, 1f - bagHolderBox.RelativeSize.Y - spacingInScope.Y));
            LayoutGold(spacingInScope);
        }

        void LayoutGold(RelativeScreenPosition spacingInScope)
        {
            goldImage.Resize(RelativeScreenPosition.GetSquareFromY(bagHolderBox.RelativeSize.Y / 2, Size));
            goldImage.Move(RelativeScreenPosition.One - bagHolderBox.RelativeSize.OnlyY / 2 - goldImage.RelativeSize.OnlyY / 2 - goldImage.RelativeSize.OnlyX - spacingInScope.OnlyX);
            gold.Resize(new RelativeScreenPosition(1 - goldImage.RelativeSize.X - spacingInScope.X * 2, bagHolderBox.RelativeSize.Y));
            gold.Move(RelativeScreenPosition.One.OnlyY - gold.RelativeSize.OnlyY - spacingInScope.OnlyY);
        }

        public override void Rescale()
        {
            spacing = RelativeScreenPosition.GetSquareFromX(spacingX);
            base.Rescale();
            InitStatics();
            if (hasSnapshot)
            {
                SetInventory(latestSnapshot);
            }
            else
            {
                CalculateDynamicSize();
                LayoutElements();
            }
        }
    }
}
