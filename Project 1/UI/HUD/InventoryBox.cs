using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Bson;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Input;
using Project_1.Items;
using Project_1.Textures;
using Project_1.UI.HUD.Inventory;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD
{
    internal class InventoryBox : Box
    {

        //Inventory inventory;

        BagHolderBox bagHolderBox;
        BagBox[] bagBox;

        const float itemSizeX = 0.06f;
        const float spacingX = 0.01f;

        public static AbsoluteScreenPosition itemSize;
        public static AbsoluteScreenPosition size;
        public static RelativeScreenPosition xdd(AbsoluteScreenPosition aParentSize) => itemSize.ToRelativeScreenPosition(aParentSize);
        public static RelativeScreenPosition spacing;

        int columnCount;

        public InventoryBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("WhiteBackground",new Color(80, 80, 80, 80)), aPos, aSize) //this will scale down size to closest fit
        {
            itemSize = RelativeScreenPosition.GetSquareFromX(itemSizeX).ToAbsoluteScreenPos();
            spacing = RelativeScreenPosition.GetSquareFromX(spacingX);

            visibleKey = KeyBindManager.KeyListner.Inventory;
            Visible = false;
            Dragable = true;

            RelativeScreenPosition smile = itemSize.ToRelativeScreenPosition(Size);
            RelativeScreenPosition bhPos = new RelativeScreenPosition(spacing.X, 1f - (smile.Y + spacing.Y * 3));
            RelativeScreenPosition bhSize = new RelativeScreenPosition(smile.X * (Items.Inventory.bagSlots + 1) + spacing.X * (Items.Inventory.bagSlots + 2), smile.Y + spacing.Y * 2);

            bagHolderBox = new BagHolderBox(bhPos, bhSize);

            //columnCount = CalculateColumns(Inventory.defaultSlots, itemSize.X, spacing.X, aSize.X);
            columnCount = 8;
            bagBox = new BagBox[Items.Inventory.bagSlots + 1];

            //bagBox[0] = new BagBox(null, 0, 0, columnCount, spacing, CalculateBagBoxSize(Inventory.defaultSlots, itemSize.ToRelativeScreenPosition(), spacing, aSize.Y));
            bagBox[0] = new BagBox(null, 0, 0, columnCount, spacing, RelativeScreenPosition.Zero);
            for (int i = 1; i < bagBox.Length; i++)
            {
                bagBox[i] = new BagBox(null, i, 0, 1, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero);
            }
            AddChild(bagHolderBox);
            AddChildren(bagBox);

            alwaysOnScreen = true;
            hudMoveable = false;

            size = Size;
        }

        public override void Resize(RelativeScreenPosition aSize)
        {
            base.Resize(aSize);

            size = Size;
        }

        public void SetInventory(Items.Inventory aInventory)
        {
            bagHolderBox.SetBags(aInventory.GetBags());

            CreateBagBoxes(RelativeSize, aInventory);
            CalculateSize(RelativePos); //TODO: Make this accept a enum that dictates wheter it grows up and down
        }

        void CreateBagBoxes(RelativeScreenPosition aSize, Items.Inventory aInventory)
        {
            KillAllChildren();
            AddChild(bagHolderBox);
            RelativeScreenPosition bagPos = RelativeScreenPosition.Zero;
            bagPos.X = spacing.X;
            bagPos.Y = CalculateBagBoxSize(Items.Inventory.defaultSlots, itemSize.ToRelativeScreenPosition(), spacing, aSize.Y).Y;
            bagBox[0] = new BagBox(aInventory, 0, Items.Inventory.defaultSlots, columnCount, spacing, RelativeScreenPosition.Zero);
            bagPos.Y += spacing.Y;
            bagPos.Y += spacing.Y;

            for (int i = 1; i < bagBox.Length; i++)
            {
                if (aInventory.Bags[i] == null)
                {
                    bagBox[i] = new BagBox(aInventory, i, 0, 1, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero);
                    continue;
                }

                RelativeScreenPosition size = RelativeScreenPosition.Zero;
                //RelativeScreenPosition size = CalculateBagBoxSize(aInventory.Bags[i].SlotCount, itemSize.ToRelativeScreenPosition(), spacing, aSize.Y);

                bagBox[i] = new BagBox(aInventory, i, aInventory.Bags[i].SlotCount, columnCount, bagPos, size);
                bagPos.Y += size.Y + spacing.Y;
            }

            AddChildren(bagBox);

        }

        public void RefreshSlot(int aBag, int aSlot, Items.Inventory aInventory)
        {
            if (aBag >= 0)
            {
                bagBox[aBag].RefreshSlot(aSlot);
                return;
            }
            
            bagHolderBox.RefreshSlot(aSlot);
            if (aInventory.Bags[aSlot] != null)
            {
                bagBox[aSlot].RefreshBag(aInventory, aInventory.Bags[aSlot].SlotCount, columnCount);
                CalculateSize(RelativePos);
                return;
            }
            bagBox[aSlot].RefreshBag(aInventory, 0, 1);

            CalculateSize(RelativePos);

            return;
            
        }

        void CalculateSize(RelativeScreenPosition aPos) //TODO: Find better name
        {
            RelativeScreenPosition resize = RelativeScreenPosition.Zero;

            resize.X = bagBox[0].RelativeSize.X + spacing.X * 2;
            
            float bagY = spacing.Y;
            for (int i = 0; i < bagBox.Length; i++)
            {
                if (bagBox[i].RelativeSize.Y == 0)
                {
                    continue;
                }
                bagBox[i].Move(new RelativeScreenPosition(spacing.X, bagY));
                bagY += bagBox[i].RelativeSize.Y + spacing.Y;
            }

            resize.Y = bagHolderBox.RelativeSize.Y + spacing.Y + bagY;

            Resize(resize);
            bagHolderBox.Move(new RelativeScreenPosition(spacing.X, 1f - (itemSize.Y + spacing.Y * 3)));
            Move(new RelativeScreenPosition(RelativePos.X, aPos.Y));

        }

        static RelativeScreenPosition CalculateBagBoxSize(int aSlotCount, RelativeScreenPosition aItemSize, RelativeScreenPosition aSpacing, float aWidthOfInventory)
        {
            RelativeScreenPosition size = new RelativeScreenPosition();

            int columns = 8;
            size.X = columns * (aItemSize.X + aSpacing.X) + aSpacing.X;
            int rows = CalculateRows(aSlotCount, aItemSize.X, aSpacing.X, aWidthOfInventory);
            size.Y = rows * (aItemSize.Y + aSpacing.Y) + aSpacing.Y;

            return RelativeScreenPosition.Zero;
        }

        static int CalculateColumns(int aSlotCount, float aItemSizeX, float aSpacingX, float aWidthOfInventory) => (int)Math.Floor(aWidthOfInventory / (aItemSizeX + aSpacingX + aSpacingX / aSlotCount)); //TODO: This has to be wrong
        static int CalculateRows(int aSlotCount, float aItemSizeX, float aSpacingX, float aWidthOfInventory) => (int)Math.Ceiling(aSlotCount / 8d);


        public override void Rescale()
        {
            //TODO
            spacing = RelativeScreenPosition.GetSquareFromX(spacingX);
            itemSize = RelativeScreenPosition.GetSquareFromX(itemSizeX).ToAbsoluteScreenPos();
            base.Rescale();
        }
    }
}
