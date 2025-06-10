using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Bson;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Input;
using Project_1.Items;
using Project_1.Managers;
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

        public static RelativeScreenPosition ItemSize => itemSize;
        static RelativeScreenPosition itemSize;

        public static RelativeScreenPosition BagBoxSize => bagBoxSize;
        static RelativeScreenPosition bagBoxSize;

        public static RelativeScreenPosition BagBoxSpacing => bagBoxSpacing;
        static RelativeScreenPosition bagBoxSpacing;


        public static RelativeScreenPosition outerSpacing = RelativeScreenPosition.GetSquareFromX(spacingX);

        int columnCount;

        public InventoryBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize, int aColumnCount) : base(new UITexture("WhiteBackground",new Color(80, 80, 80, 80)), aPos, new RelativeScreenPosition(0.3f, 0.4f))
        {
            columnCount = aColumnCount;

            bagBoxSize = new RelativeScreenPosition(1 - outerSpacing.X * 2, 0.3f);
            itemSize  = RelativeScreenPosition.GetSquareFromX((1 - (aColumnCount + 1) * outerSpacing.X) / (float)aColumnCount, bagBoxSize.ToAbsoluteScreenPos(Size));
            bagBoxSpacing = RelativeScreenPosition.GetSquareFromX(spacingX, bagBoxSize.ToAbsoluteScreenPos(Size));
            DebugManager.Print(GetType(), "bbS: " + bagBoxSpacing.ToAbsoluteScreenPos(bagBoxSize.ToAbsoluteScreenPos(Size)) + " | iS: " + itemSize.ToAbsoluteScreenPos(bagBoxSize.ToAbsoluteScreenPos(Size)));
            visibleKey = KeyBindManager.KeyListner.Inventory;
            Visible = false;
            Dragable = true;

            RelativeScreenPosition itemSizeInInventoryScope = itemSize.ToAbsoluteScreenPos(bagBoxSize.ToAbsoluteScreenPos(Size)).ToRelativeScreenPosition(Size);
            RelativeScreenPosition bagBoxSpacingInInventoryScope = bagBoxSpacing.ToAbsoluteScreenPos(bagBoxSize.ToAbsoluteScreenPos(Size)).ToRelativeScreenPosition(Size);
            RelativeScreenPosition bhPos = new RelativeScreenPosition(bagBoxSpacingInInventoryScope.X, 1f - (itemSizeInInventoryScope.Y + bagBoxSpacingInInventoryScope.Y * 3));
            RelativeScreenPosition bhSize = new RelativeScreenPosition(itemSizeInInventoryScope.X * (Items.Inventory.bagSlots) + bagBoxSpacingInInventoryScope.X * (Items.Inventory.bagSlots + 1), itemSizeInInventoryScope.Y + bagBoxSpacingInInventoryScope.Y * 2);

            bagHolderBox = new BagHolderBox(bhPos, bhSize);

            //columnCount = CalculateColumns(Inventory.defaultSlots, itemSize.X, spacing.X, aSize.X);
            bagBox = new BagBox[Items.Inventory.bagSlots];

            //bagBox[0] = new BagBox(null, 0, 0, columnCount, spacing, CalculateBagBoxSize(Inventory.defaultSlots, itemSize.ToRelativeScreenPosition(), spacing, aSize.Y));
            bagBox[0] = new BagBox(0, outerSpacing, RelativeScreenPosition.Zero);
            for (int i = 1; i < bagBox.Length; i++)
            {
                bagBox[i] = new BagBox(i, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero);
            }
            AddChild(bagHolderBox);
            AddChildren(bagBox);

            alwaysOnScreen = true;
            hudMoveable = false;
        }

        public override void Resize(RelativeScreenPosition aSize)
        {
            base.Resize(aSize);
        }

        public void SetInventory(Items.Inventory aInventory)
        {
            bagHolderBox.SetBags(aInventory.GetBags(), itemSize.ToAbsoluteScreenPos(bagBoxSize.ToAbsoluteScreenPos(Size)), bagBoxSpacing.ToAbsoluteScreenPos(bagBoxSize.ToAbsoluteScreenPos(Size)));

            CreateBagBoxes(RelativeSize, aInventory);
            CalculateSize(RelativePos); //TODO: Make this accept a enum that dictates wheter it grows up and down
        }

        void CreateBagBoxes(RelativeScreenPosition aSize, Items.Inventory aInventory)
        {
            RelativeScreenPosition bagPos = RelativeScreenPosition.Zero;
            RelativeScreenPosition size = new RelativeScreenPosition(1 - outerSpacing.X * 2, 0.3f);

            bagPos.X = outerSpacing.X;
            bagBox[0].Move(outerSpacing);
            bagBox[0].Resize(size);
            bagBox[0].RefreshBag(aInventory, Items.Inventory.defaultSlots, columnCount);
            //bagBox[0] = new BagBox(aInventory, 0, Items.Inventory.defaultSlots, columnCount, spacing, new RelativeScreenPosition(1 - spacing.X * 2, 0.3f));
            bagPos.Y += outerSpacing.Y;
            bagPos.Y += bagBox[0].RelativeSize.Y;
            bagPos.Y += outerSpacing.Y;

            for (int i = 1; i < bagBox.Length; i++)
            {
                if (aInventory.Bags[i] == null)
                {
                    bagBox[i].Empty();
                    continue;
                }

                //RelativeScreenPosition size = CalculateBagBoxSize(aInventory.Bags[i].SlotCount, itemSize.ToRelativeScreenPosition(), spacing, aSize.Y);

                bagBox[i].Move(bagPos);
                bagBox[i].Resize(size);
                bagBox[i].RefreshBag(aInventory, aInventory.GetBag(i).SlotCount, columnCount);

                //bagBox[i] = new BagBox(aInventory, i, aInventory.Bags[i].SlotCount, columnCount, bagPos, size);
                bagPos.Y += size.Y + outerSpacing.Y;
            }
        }

        public void RefreshSlot(int aBag, int aSlot, Items.Inventory aInventory)
        {
            if (aBag >= 0)
            {
                bagBox[aBag].RefreshSlot(aSlot);
                return;
            }

            bagHolderBox.RefreshSlot(aSlot);
            if (aInventory.Bags[aSlot] == null)
            {
                bagBox[aSlot].Empty();
                CalculateSize(RelativePos);
                return;
            }

            RelativeScreenPosition pos = outerSpacing + bagBox[0].RelativeSize.OnlyY + outerSpacing.OnlyY;
            for (int i = 1; i < aSlot; i++)
            {
                if (aInventory.Bags[i] == null) continue;

                pos.Y += bagBox[i].RelativeSize.Y + outerSpacing.Y;
            }
            bagBox[aSlot].Move(pos);
            bagBox[aSlot].Resize(bagBoxSize);
            bagBox[aSlot].RefreshBag(aInventory, aInventory.Bags[aSlot].SlotCount, columnCount);
            CalculateSize(RelativePos);
            return;

        }

        void CalculateSize(RelativeScreenPosition aPos) //TODO: Find better name
        {
            RelativeScreenPosition resize = RelativeScreenPosition.Zero;
            //All of this is wrong xdd
            //You cannot use RelativeSize 
            
            resize.X = bagBox[0].RelativeSize.ToAbsoluteScreenPos(Size).ToRelativeScreenPosition().X + outerSpacing.X * 2;

            float bagY = outerSpacing.Y;
            (AbsoluteScreenPosition, AbsoluteScreenPosition)[] oldPosAndSize = new (AbsoluteScreenPosition, AbsoluteScreenPosition)[Items.Inventory.bagSlots]; 
            for (int i = 0; i < bagBox.Length; i++)
            {
                if (bagBox[i].RelativeSize.Y == 0)
                {
                    continue;
                }
                oldPosAndSize[i] = (bagBox[i].RelativePos.ToAbsoluteScreenPos(Size), bagBox[i].RelativeSize.ToAbsoluteScreenPos(Size));
                DebugManager.Print(GetType(), bagBox[i].RelativePos + ", " + bagBox[i].RelativeSize);
                bagY += bagBox[i].RelativeSize.ToAbsoluteScreenPos(Size).ToRelativeScreenPosition().Y + outerSpacing.Y;
            }

            resize.Y = bagHolderBox.RelativeSize.ToAbsoluteScreenPos(Size).ToRelativeScreenPosition().Y + outerSpacing.Y + bagY;

            Resize(resize);

            RelativeScreenPosition itemSizeInInventoryScope = itemSize.ToAbsoluteScreenPos(bagBoxSize.ToAbsoluteScreenPos(Size)).ToRelativeScreenPosition(Size);
            RelativeScreenPosition bagBoxSpacingInInventoryScope = bagBoxSpacing.ToAbsoluteScreenPos(bagBoxSize.ToAbsoluteScreenPos(Size)).ToRelativeScreenPosition(Size);
            bagHolderBox.Move(new RelativeScreenPosition(bagBoxSpacingInInventoryScope.X, 1f - (itemSizeInInventoryScope.Y + bagBoxSpacingInInventoryScope.Y * 3)));
            Move(new RelativeScreenPosition(RelativePos.X, aPos.Y));

            for (int i = 0; i < bagBox.Length; i++)
            {
                bagBox[i].Move(oldPosAndSize[i].Item1.ToRelativeScreenPosition(Size)); //TODO: Fix pls.
                bagBox[i].Resize(oldPosAndSize[i].Item2.ToRelativeScreenPosition(Size));
                DebugManager.Print(GetType(), bagBox[i].RelativePos + ", " + bagBox[i].RelativeSize);

            }
        }

        static RelativeScreenPosition CalculateBagBoxSize(int aSlotCount, RelativeScreenPosition aItemSize, RelativeScreenPosition aSpacing, float aWidthOfInventory)
        {
            //RelativeScreenPosition size = new RelativeScreenPosition();

            //int columns = 8;
            //size.X = columns * (aItemSize.X + aSpacing.X) + aSpacing.X;
            //int rows = CalculateRows(aSlotCount, aItemSize.X, aSpacing.X, aWidthOfInventory);
            //size.Y = rows * (aItemSize.Y + aSpacing.Y) + aSpacing.Y;

            return RelativeScreenPosition.Zero;
        }

        static int CalculateRows(int aSlotCount, float aItemSizeX, float aSpacingX, float aWidthOfInventory) => (int)Math.Ceiling(aSlotCount / 8d);


        public override void Rescale()
        {
            //TODO
            outerSpacing = RelativeScreenPosition.GetSquareFromX(spacingX);
            //itemSize = RelativeScreenPosition.GetSquareFromX(itemSizeX).ToAbsoluteScreenPos();
            base.Rescale();
        }
    }
}
