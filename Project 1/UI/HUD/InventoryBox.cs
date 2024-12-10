using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Bson;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Input;
using Project_1.Items;
using Project_1.Textures;
using Project_1.UI.UIElements;
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

        Inventory inventory;

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

            inventory = ObjectManager.Player.Inventory;
            bagHolderBox = new BagHolderBox(new RelativeScreenPosition(spacing.X, aSize.Y - (itemSize.Y + spacing.Y * 3)), new RelativeScreenPosition(itemSize.X * (inventory.bagSlots + 1) + spacing.X * (inventory.bagSlots + 2), itemSize.Y + spacing.Y * 2));
            bagHolderBox.SetBags(inventory.GetBags());

            columnCount = CalculateColumns(inventory.defaultSlots, itemSize.X, spacing.X, aSize.X);
            bagBox = new BagBox[inventory.bagSlots + 1];

            CreateBagBoxes(aSize);

            children.Add(bagHolderBox);
            children.AddRange(bagBox);

            CalculateSize();
            ToggleVisibilty();
        }

        void CreateBagBoxes(RelativeScreenPosition aSize)
        {
            RelativeScreenPosition bagPos = RelativeScreenPosition.Zero;
            bagPos.X = spacing.X;
            bagPos.Y = CalculateBagBoxSize(inventory.defaultSlots, itemSize, spacing, aSize.X).Y;
            bagBox[0] = new BagBox(0, inventory.defaultSlots, columnCount, spacing, CalculateBagBoxSize(inventory.defaultSlots, itemSize, spacing, aSize.X));
            bagPos.Y += spacing.Y;
            bagPos.Y += spacing.Y;

            for (int i = 1; i < bagBox.Length; i++)
            {
                if (inventory.bags[i] == null)
                {
                    bagBox[i] = new BagBox(i, 0, 1, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero);
                    continue;
                }

                RelativeScreenPosition size = CalculateBagBoxSize(inventory.bags[i].SlotCount, itemSize, spacing, aSize.X);

                bagBox[i] = new BagBox(i, inventory.bags[i].SlotCount, columnCount, bagPos, size);
                bagPos.Y += size.Y + spacing.Y;
            }
        }

        public void RefreshSlot(int aBag, int aSlot)
        {
            if (aBag == -1)
            {
                bagHolderBox.RefreshSlot(aSlot);
                if (inventory.bags[aSlot] != null)
                {
                    bagBox[aSlot].RefreshBag(inventory.bags[aSlot].SlotCount, columnCount);
                    CalculateSize();
                    return;
                }
                bagBox[aSlot].RefreshBag(0, 1);

                CalculateSize();

                return;
            }
            bagBox[aBag].RefreshSlot(aSlot);
        }

        //public void RefreshAllBags()
        //{
        //    //for (int i = 0; i < bagBox.Length; i++)
        //    //{
        //    //    bagBox[i].GetBagContent();
        //    //}
        //}

        //public void RefreshSlot(int aBag, int aSlot)
        //{
        //    ////if (bagBox[aBag] == null) return false;
        //    //Debug.Assert(bagBox[aBag].SlotCount == 0 || bagBox[aBag].SlotCount < aSlot, "Either tried to refresh empty bag or slot nr was to big.");
        //    //bagBox[aBag].RefreshSlot(aSlot);
        //}

        void CalculateSize() //TODO: Find better name
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
            Move(new RelativeScreenPosition(RelativePos.X, 0.9f - resize.Y));

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

        public override void Update(in UIElement aParent)
        {
            base.Update(aParent);

            

        }

        public override void Rescale()
        {
            base.Rescale();

            itemSize = RelativeScreenPosition.GetSquareFromX(itemSizeX);
            spacing = RelativeScreenPosition.GetSquareFromX(spacingX);
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);
        }
    }
}
