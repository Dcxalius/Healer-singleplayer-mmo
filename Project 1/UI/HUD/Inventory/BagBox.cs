using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities.Players;
using Project_1.Items;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.HUD;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;

namespace Project_1.UI.HUD.Inventory
{
    internal class BagBox : Box
    {
        public int SlotCount { get => slots.Length; }
        Item[] slots;
        int bagNr;


        public BagBox(int aBagNr, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("WhiteBackground", Color.Beige), aPos, aSize)
        {
            bagNr = aBagNr;
        }

        void GetBagContent(Items.Inventory aInventory, int aSlotCount, int aColumnCount)
        {
            if (aSlotCount == 0)
            {
                Empty();
                return;
            }

            slots = new Item[aSlotCount];
            Items.Item[] items = aInventory.GetItemsInBox(bagNr);

            float rowCount = MathF.Ceiling(aSlotCount / (float)aColumnCount);

            //DebugManager.Print(GetType(), "spacing: " + InventoryBox.bagBoxSpacing + "   | itemSize: " + itemSize);
            //DebugManager.Print(GetType(), "spacing: " + InventoryBox.bagBoxSpacing.ToAbsoluteScreenPos(Size) + "   | itemSize: " + itemSize.ToAbsoluteScreenPos(Size));

            AbsoluteScreenPosition absItem = InventoryBox.ItemSize.ToAbsoluteScreenPos(Size);
            AbsoluteScreenPosition absSpacing = InventoryBox.BagBoxSpacing.ToAbsoluteScreenPos(Size);

            AbsoluteScreenPosition r = absItem * rowCount + absSpacing * (rowCount + 1);
            RelativeScreenPosition r2 = RelativeSize;
            if (aSlotCount < aColumnCount)
            {
                AbsoluteScreenPosition xdd = absItem * aSlotCount + absSpacing * (aSlotCount + 1);
                r2.X = xdd.ToRelativeScreenPosition(ParentSize).X; 
                aColumnCount = aSlotCount;
            }
            r2.Y = r.ToRelativeScreenPosition(ParentSize).Y;
            Resize(r2);
            RelativeScreenPosition itemSize = absItem.ToRelativeScreenPosition(Size);
            RelativeScreenPosition spacing = absSpacing.ToRelativeScreenPosition(Size);

            for (int i = 0; i < slots.Length; i++)
            {
               // float x = spacing.X + i % aColumnCount * (itemSize.X + spacing.X);
                float x = spacing.X + (i % aColumnCount) * ((1 - spacing.X) / (float)aColumnCount);

                float y = spacing.Y + (itemSize.Y + spacing.Y) * (float)Math.Floor((double)i / aColumnCount);
                RelativeScreenPosition pos = new RelativeScreenPosition(x, y);
                DebugManager.Print(GetType(), pos.ToAbsoluteScreenPos(Size).ToString());
                if (items[i] != null)
                {
                    slots[i] = new Item(bagNr, i, true, items[i].ItemQualityColor, items[i].GfxPath, pos, itemSize);
                    
                }
                else
                {
                    slots[i] = new Item(bagNr, i, true, Color.DarkGray, new GfxPath(GfxType.Item, null), pos, itemSize);
                    //DebugManager.Print(GetType(), slots[i].Size.ToString() );
                }
            }

            AddChildren(slots);
        }

        public void Empty()
        {
            KillAllChildren();
            Resize(RelativeScreenPosition.Zero);
        }

        public void RefreshSlot(int aSlot)
        {
            slots[aSlot].AssignItem(ObjectManager.Player.Inventory.GetItemInSlot(bagNr, aSlot));
        }

        public void RefreshBag(Items.Inventory aInventory, int aSlotCount, int aColumnCount)
        {
            KillAllChildren();
            GetBagContent(aInventory, aSlotCount, aColumnCount);
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);
        }
    }
}
