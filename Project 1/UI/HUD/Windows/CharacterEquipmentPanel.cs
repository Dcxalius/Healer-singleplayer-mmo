using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Unit;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.HUD.Inventory;
using Project_1.UI.UIElements;
using System;

namespace Project_1.UI.HUD.Windows
{
    internal sealed class CharacterEquipmentPanel : UIElement
    {
        static readonly RelativeScreenPosition itemSize = RelativeScreenPosition.GetSquareFromY(0.09f, Window.WindowSize.ToAbsoluteScreenPos());
        static readonly RelativeScreenPosition itemSpacing = RelativeScreenPosition.GetSquareFromY(0.01f, Window.WindowSize.ToAbsoluteScreenPos());
        static readonly RelativeScreenPosition leftSideTop = itemSpacing;
        static readonly RelativeScreenPosition rightSideTop = new RelativeScreenPosition(1f - itemSpacing.X - itemSize.X, itemSpacing.Y);
        static readonly RelativeScreenPosition bottomSideLeft = new RelativeScreenPosition(0.5f - itemSize.X * 1.5f - itemSpacing.X, (itemSize.Y + itemSpacing.Y) * (int)Equipment.Slot.Hands + itemSpacing.Y);

        public static RelativeScreenPosition ItemSize => itemSize;
        public static RelativeScreenPosition ItemSpacing => itemSpacing;
        public static RelativeScreenPosition StatsTop => new RelativeScreenPosition(0, (itemSize.Y + itemSpacing.Y) * ((int)Equipment.Slot.Hands + 1));

        readonly Item[] equippedItems;

        public CharacterEquipmentPanel(UIElement aParent, int aBagIndex) : base(aParent, null, RelativeScreenPosition.Zero, RelativeScreenPosition.One)
        {
            equippedItems = new Item[(int)Equipment.Slot.Count];

            RelativeScreenPosition yChange = new RelativeScreenPosition(0, itemSpacing.Y + itemSize.Y);
            CreateItems(aBagIndex, Equipment.Slot.Head, Equipment.Slot.Hands, leftSideTop, yChange);
            CreateItems(aBagIndex, Equipment.Slot.Belt, Equipment.Slot.Trinket2, rightSideTop, yChange);

            RelativeScreenPosition xChange = new RelativeScreenPosition(itemSpacing.X + itemSize.X, 0);
            CreateItems(aBagIndex, Equipment.Slot.MainHand, Equipment.Slot.Ranged, bottomSideLeft, xChange);

            CapturesClick = false;
            CapturesRelease = false;
            CapturesScroll = false;
        }

        public void SetSlot(Equipment.Slot aSlot, ItemUiSnapshot aItemSnapshot)
        {
            equippedItems[(int)aSlot].AssignItem(aItemSnapshot);
        }

        public void SetAllSlots(ItemUiSnapshot[] aItems)
        {
            if (aItems == null) return;

            int count = Math.Min(aItems.Length, equippedItems.Length);
            for (int i = 0; i < count; i++)
            {
                equippedItems[i].AssignItem(aItems[i]);
            }
        }

        void CreateItems(int aBagIndex, Equipment.Slot aStart, Equipment.Slot aEnd, RelativeScreenPosition aStartPos, RelativeScreenPosition aChangeInPos)
        {
            for (int i = (int)aStart; i <= (int)aEnd; i++)
            {
                equippedItems[i] = new Item(this, aBagIndex, i, true, Color.Teal, new GfxPath(GfxType.Item, null), aStartPos + aChangeInPos * (i - (int)aStart), itemSize);
            }
        }
    }
}
