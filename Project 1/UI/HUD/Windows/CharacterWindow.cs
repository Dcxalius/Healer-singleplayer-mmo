using Microsoft.Xna.Framework;
using Newtonsoft.Json.Bson;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Unit;
using Project_1.Textures;
using Project_1.UI.HUD.Inventory;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Bars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Project_1.UI.HUD.Windows
{
    internal class CharacterWindow : Window
    {
        protected Label nameLabel;
        Item[] equiped;

        Label nrStatReport;
        Label stringStatReport;
        ExpBar expBar;

        protected virtual int BagIndexForItem => -3;

        static RelativeScreenPosition itemSize = RelativeScreenPosition.GetSquareFromY(0.09f, WindowSize.ToAbsoluteScreenPos());
        static RelativeScreenPosition itemSpacing = RelativeScreenPosition.GetSquareFromY(0.01f, WindowSize.ToAbsoluteScreenPos());
        static RelativeScreenPosition leftSideTop = itemSpacing;
        static RelativeScreenPosition rightSideTop = new RelativeScreenPosition(1f - itemSpacing.X - itemSize.X, itemSpacing.Y);
        static RelativeScreenPosition bottomSideLeft = new RelativeScreenPosition(1 / 2f - itemSize.X * 1.5f - itemSpacing.X, (itemSize.Y + itemSpacing.Y) * (int)Equipment.Slot.Hands + itemSpacing.Y);

        static RelativeScreenPosition expBarSize = new RelativeScreenPosition(1f, 0.05f);
        static RelativeScreenPosition expBarPos = new RelativeScreenPosition(0, 1f - expBarSize.Y);

        public CharacterWindow() : base(new UITexture("WhiteBackground", Color.Turquoise))
        {
            nameLabel = new Label(null, new RelativeScreenPosition(0, 0), new RelativeScreenPosition(1, itemSize.Y), Label.TextAllignment.TopCentre, Color.Black);

            visibleKey = Input.KeyBindManager.KeyListner.Character;
            equiped = new Item[(int)Equipment.Slot.Count];

            RelativeScreenPosition yChange = new RelativeScreenPosition(0, itemSpacing.Y) + new RelativeScreenPosition(0, itemSize.Y);
            CreateItems(Equipment.Slot.Head, Equipment.Slot.Hands, leftSideTop, yChange);
            CreateItems(Equipment.Slot.Belt, Equipment.Slot.Trinket2, rightSideTop, yChange);
            RelativeScreenPosition xChange = new RelativeScreenPosition(itemSpacing.X, 0) + new RelativeScreenPosition(itemSize.X, 0);
            CreateItems(Equipment.Slot.MainHand, Equipment.Slot.Ranged, bottomSideLeft, xChange);

            RelativeScreenPosition topPart = new RelativeScreenPosition(0, yChange.Y * ((int)Equipment.Slot.Hands + 1));
            RelativeScreenPosition textBoxPos = topPart + itemSpacing;
            RelativeScreenPosition textBoxSize = new RelativeScreenPosition(1 - itemSpacing.X * 2, 1 - topPart.Y - itemSpacing.Y * 2) / 2;

            nrStatReport = new Label(null, textBoxPos, textBoxSize - new RelativeScreenPosition(itemSpacing.X, 0), Label.TextAllignment.TopRight, Color.Black);
            stringStatReport = new Label(null, textBoxPos + new RelativeScreenPosition(textBoxSize.X, 0f) + new RelativeScreenPosition(itemSpacing.X, 0), textBoxSize - new RelativeScreenPosition(itemSpacing.X, 0), Label.TextAllignment.TopLeft, Color.Black);

            expBar = new ExpBar(expBarPos, expBarSize);
            AddChild(nameLabel);
            AddChildren(equiped);
            AddChild(nrStatReport);
            AddChild(stringStatReport);
            AddChild(expBar);

        }

        public virtual void SetData(Friendly aFriendly)
        {
            nameLabel.Text = aFriendly.Name;
            SetReportBox(aFriendly.PrimaryStatReport);
            RefreshExp(aFriendly.Level);
            SetAllSlots(aFriendly.Equipment);
        }

        void CreateItems(Equipment.Slot aStart, Equipment.Slot aEnd, RelativeScreenPosition aStartPos, RelativeScreenPosition aChangeInPos)
        {
            for (int i = (int)aStart; i <= (int)aEnd; i++)
            {
                equiped[i] = new Item(BagIndexForItem, i, true, Color.Teal, new GfxPath(GfxType.Item, null), aStartPos + aChangeInPos * (i - (int)aStart), itemSize);
            }
        }

        void SetAllSlots(Equipment aEquipment)
        {
            for (int i = 0; i < (int)Equipment.Slot.Count; i++)
            {
                SetSlot((Equipment.Slot)i, aEquipment);
            }
        }

        public void SetSlot(Equipment.Slot aSlot, Equipment aEquipment)
        {
            equiped[(int)aSlot].AssignItem(aEquipment.EquipedInSlot(aSlot));
        }

        public void SetReportBox(PairReport aReport)
        {
            stringStatReport.Text = aReport.StringsOnly;
            nrStatReport.Text = aReport.NumbersOnly;
        }

        public void RefreshExp(Level aLevel)
        {
            expBar.MaxValue = Level.ExpToNextLevel(aLevel.CurrentLevel);
            expBar.Value = aLevel.Experience;
        }
    }
}
