using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects;
using Project_1.Input;
using Project_1.Items;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD
{
    internal class InventoryBox : Box
    {
        public bool visible = false;

        Inventory inventory;

        BagHolderBox bagHolderBox;
        BagBox[] bagBox;

        const float itemSizeX = 0.02f;
        const float spacingX = 0.005f;

        public static Vector2 itemSize = Camera.GetRelativeSquare(itemSizeX);
        public static Vector2 spacing = Camera.GetRelativeSquare(spacingX);
        public InventoryBox(Vector2 aPos, Vector2 aSize) : base(new UITexture("WhiteBackground",new Color(80, 80, 80, 80)), aPos, aSize) //this will scale down size to closest fit
        {
            inventory = ObjectManager.Player.Inventory;
            bagHolderBox = new BagHolderBox(new Vector2(spacing.X, aSize.Y - (itemSize.Y + spacing.Y * 3)), new Vector2(itemSize.X * (inventory.bagSlots + 1) + spacing.X * (inventory.bagSlots + 2), itemSize.Y + spacing.Y * 2));
            bagHolderBox.SetBags(inventory.GetBags());

            int columnCount = CalculateColumns(inventory.defaultSlots, itemSize.X, spacing.X, aSize.X);
            bagBox = new BagBox[inventory.bagSlots + 1];

            Vector2 bagPos = Vector2.Zero;
            bagPos.X = spacing.X;
            bagPos.Y = CalculateBagBoxSize(inventory.defaultSlots, itemSize, spacing, aSize.X).Y;
            bagBox[0] = new BagBox(inventory.defaultSlots, columnCount, spacing, CalculateBagBoxSize(inventory.defaultSlots, itemSize, spacing, aSize.X));
            bagPos.Y += spacing.Y;
            bagPos.Y += spacing.Y;

            for (int i = 1; i < bagBox.Length; i++)
            {
                if (inventory.bags[i - 1] == null)
                {
                    bagBox[i] = new BagBox(0, 1, Vector2.Zero, Vector2.Zero);
                    continue;
                }

                Vector2 size = CalculateBagBoxSize(inventory.bags[i - 1].SlotCount, itemSize, spacing, aSize.X);

                bagBox[i] = new BagBox(inventory.bags[i - 1].SlotCount, columnCount, bagPos, size);
                bagPos.Y += size.Y + spacing.Y;
            }

            children.Add(bagHolderBox);
            children.AddRange(bagBox);

            CalculateSize();
        }

        void CalculateSize() //TODO: Find better name
        {
            Vector2 resize = Vector2.Zero;

            resize.X = bagBox[0].RelativeSize.X + spacing.X * 2;
            
            float bagY = 0;
            for (int i = 0; i < bagBox.Length; i++)
            {
                if (bagBox[i].RelativeSize.Y == 0)
                {
                    continue;
                }
                bagY = bagBox[i].RelativeSize.Y + spacing.Y;
            }

            resize.Y = spacing.Y + bagHolderBox.RelativeSize.Y + spacing.Y + bagBox[0].RelativeSize.Y + spacing.Y + bagY;

            Resize(resize);

            bagHolderBox.Move(new Vector2(spacing.X, RelativeSize.Y - (itemSize.Y + spacing.Y * 3)));
        }

        Vector2 CalculateBagBoxSize(int aSlotCount, Vector2 aItemSize, Vector2 aSpacing, float aWidthOfInventory)
        {
            Vector2 size = new Vector2();

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

            if (KeyBindManager.GetPress(KeyBindManager.KeyListner.Inventory))
            {
                visible = !visible;
            }

        }

        public override void Rescale()
        {
            base.Rescale();

            itemSize = Camera.GetRelativeSquare(itemSizeX);
            spacing = Camera.GetRelativeSquare(spacingX);
        }

        public override void Draw(SpriteBatch aBatch)
        {
            if (!visible) return;

            base.Draw(aBatch);
        }
    }
}
