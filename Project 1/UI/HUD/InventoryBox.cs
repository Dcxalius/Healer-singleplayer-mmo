using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Bson;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Input;
using Project_1.Items;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;
using Project_1.UI.UIElements.Inventory;
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

        const float itemSizeX = 0.02f;
        const float spacingX = 0.005f;

        public static RelativeScreenPosition itemSize = RelativeScreenPosition.GetSquareFromX(itemSizeX);
        public static RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(spacingX);

        int columnCount;
        public InventoryBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("WhiteBackground",new Color(80, 80, 80, 80)), aPos, aSize) //this will scale down size to closest fit
        {
            visibleKey = KeyBindManager.KeyListner.Inventory;
            Visible = false;
            dragable = true;
            //inventory = ObjectManager.Player.Inventory;
            bagHolderBox = new BagHolderBox(new RelativeScreenPosition(spacing.X, aSize.Y - (itemSize.Y + spacing.Y * 3)), new RelativeScreenPosition(itemSize.X * (Inventory.bagSlots + 1) + spacing.X * (Inventory.bagSlots + 2), itemSize.Y + spacing.Y * 2));

            columnCount = CalculateColumns(Inventory.defaultSlots, itemSize.X, spacing.X, aSize.X);
            bagBox = new BagBox[Inventory.bagSlots + 1];

            bagBox[0] = new BagBox(null, 0, 0, columnCount, spacing, CalculateBagBoxSize(Inventory.defaultSlots, itemSize, spacing, aSize.X));
            for (int i = 1; i < bagBox.Length; i++)
            {
                bagBox[i] = new BagBox(null, i, 0, 1, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero);
            }
            AddChild(bagHolderBox);
            AddChildren(bagBox);
        }

        public void SetInventory(Inventory aInventory)
        {
            bagHolderBox.SetBags(aInventory.GetBags());

            CreateBagBoxes(RelativeSize, aInventory);
            CalculateSize(RelativePos); //TODO: Make this accept a enum that dictates wheter it grows up and down
        }

        void CreateBagBoxes(RelativeScreenPosition aSize, Inventory aInventory)
        {
            KillAllChildren();
            AddChild(bagHolderBox);
            RelativeScreenPosition bagPos = RelativeScreenPosition.Zero;
            bagPos.X = spacing.X;
            bagPos.Y = CalculateBagBoxSize(Inventory.defaultSlots, itemSize, spacing, aSize.X).Y;
            bagBox[0] = new BagBox(aInventory, 0, Inventory.defaultSlots, columnCount, spacing, CalculateBagBoxSize(Inventory.defaultSlots, itemSize, spacing, aSize.X));
            bagPos.Y += spacing.Y;
            bagPos.Y += spacing.Y;

            for (int i = 1; i < bagBox.Length; i++)
            {
                if (aInventory.bags[i] == null)
                {
                    bagBox[i] = new BagBox(aInventory, i, 0, 1, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero);
                    continue;
                }

                RelativeScreenPosition size = CalculateBagBoxSize(aInventory.bags[i].SlotCount, itemSize, spacing, aSize.X);

                bagBox[i] = new BagBox(aInventory, i, aInventory.bags[i].SlotCount, columnCount, bagPos, size);
                bagPos.Y += size.Y + spacing.Y;
            }

            AddChildren(bagBox);

        }

        public void RefreshSlot(int aBag, int aSlot, Inventory aInventory)
        {
            if (aBag == -1)
            {
                bagHolderBox.RefreshSlot(aSlot);
                if (aInventory.bags[aSlot] != null)
                {
                    bagBox[aSlot].RefreshBag(aInventory, aInventory.bags[aSlot].SlotCount, columnCount);
                    CalculateSize(RelativePos + RelativeSize.OnlyY);
                    return;
                }
                bagBox[aSlot].RefreshBag(aInventory, 0, 1);

                CalculateSize(RelativePos + RelativeSize.OnlyY);

                return;
            }
            bagBox[aBag].RefreshSlot(aSlot);
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
            bagHolderBox.Move(new RelativeScreenPosition(spacing.X, RelativeSize.Y - (itemSize.Y + spacing.Y * 3)));
            Move(new RelativeScreenPosition(RelativePos.X, aPos.Y - resize.Y));

        }

        RelativeScreenPosition CalculateBagBoxSize(int aSlotCount, RelativeScreenPosition aItemSize, RelativeScreenPosition aSpacing, float aWidthOfInventory)
        {
            RelativeScreenPosition size = new RelativeScreenPosition();

            size.X = CalculateColumns(aSlotCount, aItemSize.X, aSpacing.X, aWidthOfInventory) * (aItemSize.X + aSpacing.X) + aSpacing.X;
            size.Y = (CalculateRows(aSlotCount, aItemSize.X, aSpacing.X, aWidthOfInventory) * (aItemSize.Y + aSpacing.Y)) + aSpacing.Y;

            return size;
        }

        int CalculateColumns(int aSlotCount, float aItemSizeX, float aSpacingX, float aWidthOfInventory)
        {
            //float xdd = (aItemSizeX + aSpacingX) * aSlotCount + aSpacingX;

            float xdd = aItemSizeX + aSpacingX + aSpacingX / aSlotCount;

            return (int)Math.Floor(aWidthOfInventory/xdd);
        }

        int CalculateRows(int aSlotCount, float aItemSizeX, float aSpacingX, float aWidthOfInventory)
        {
            return (int)Math.Ceiling((aSpacingX + (aItemSizeX + aSpacingX) * aSlotCount) / aWidthOfInventory);
        }


        public override void Rescale()
        {

            itemSize = RelativeScreenPosition.GetSquareFromX(itemSizeX);
            spacing = RelativeScreenPosition.GetSquareFromX(spacingX);
            base.Rescale();
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);
        }
    }
}
