using Microsoft.Xna.Framework;
using Newtonsoft.Json.Bson;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Unit;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Bars;
using Project_1.UI.UIElements.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.Windows
{
    internal class CharacterWindow : Window
    {
        Entity owner;

        Label nameLabel;
        Item[] equiped;

        Label nrStatReport;
        Label stringStatReport;
        ExpBar expBar;

        static RelativeScreenPosition itemSize = RelativeScreenPosition.GetSquareFromY(0.05f);
        static RelativeScreenPosition itemSpacing = RelativeScreenPosition.GetSquareFromY(0.003f);
        static RelativeScreenPosition leftSideTop = itemSpacing;
        static RelativeScreenPosition bottomSideLeft = new RelativeScreenPosition(WindowSize.X / 2 - itemSize.X * 1.5f - itemSpacing.X, (itemSize.Y + itemSpacing.Y) * (int)Equipment.Slot.Hands + itemSpacing.Y);
        static RelativeScreenPosition rightSideTop = new RelativeScreenPosition(WindowSize.X - itemSpacing.X - itemSize.X, itemSpacing.Y);

        static RelativeScreenPosition expBarSize = new RelativeScreenPosition(WindowSize.X, 0.05f);
        static RelativeScreenPosition expBarPos = new RelativeScreenPosition(0, WindowSize.Y - expBarSize.Y);

        public CharacterWindow(Entity aEntity) : base(new UITexture("WhiteBackground", Color.Turquoise))
        {
            owner = aEntity;

            nameLabel = new Label(owner.Name, new RelativeScreenPosition(0, 0), new RelativeScreenPosition(WindowSize.X, itemSize.Y), Label.TextAllignment.TopCentre, Color.Black);

            visibleKey = Input.KeyBindManager.KeyListner.Character;
            equiped = new Item[(int)Equipment.Slot.Count];

            RelativeScreenPosition yChange = new RelativeScreenPosition(0, itemSpacing.Y) + new RelativeScreenPosition(0, itemSize.Y);
            CreateItems(Equipment.Slot.Head, Equipment.Slot.Hands, leftSideTop, yChange);
            CreateItems(Equipment.Slot.Belt, Equipment.Slot.Trinket2, rightSideTop, yChange);
            RelativeScreenPosition xChange = new RelativeScreenPosition(itemSpacing.X, 0) + new RelativeScreenPosition(itemSize.X, 0);
            CreateItems(Equipment.Slot.MainHand, Equipment.Slot.Ranged, bottomSideLeft, xChange);

            RelativeScreenPosition topPart = new RelativeScreenPosition(0, yChange.Y * ((int)Equipment.Slot.Hands + 1));
            RelativeScreenPosition textBoxPos = topPart + itemSpacing;
            RelativeScreenPosition textBoxSize = new RelativeScreenPosition(WindowSize.X - itemSpacing.X * 2, WindowSize.Y - topPart.Y - itemSpacing.Y * 2) / 2;

            nrStatReport = new Label(owner.PrimaryStatReport.NumbersOnly, textBoxPos, textBoxSize - new RelativeScreenPosition(itemSpacing.X, 0), Label.TextAllignment.TopRight, Color.Black);
            stringStatReport = new Label(owner.PrimaryStatReport.StringsOnly, textBoxPos + new RelativeScreenPosition(textBoxSize.X, 0f) + new RelativeScreenPosition(itemSpacing.X, 0), textBoxSize - new RelativeScreenPosition(itemSpacing.X, 0), Label.TextAllignment.TopLeft, Color.Black);

            expBar = new ExpBar(expBarPos, expBarSize);
            RefreshExp();
            children.Add(nameLabel);
            children.AddRange(equiped);
            children.Add(nrStatReport);
            children.Add(stringStatReport);
            children.Add(expBar);
            ToggleVisibilty();
        }

        void CreateItems(Equipment.Slot aStart, Equipment.Slot aEnd, RelativeScreenPosition aStartPos, RelativeScreenPosition aChangeInPos)//TODO: Make this take an array 
        {
            for (int i = (int)aStart; i <= (int)aEnd; i++)
            {
                Items.Item item = owner.Equipment.EquipedInSlot((Equipment.Slot)i); //and use that to get item here instead
                if (item != null)
                {
                    equiped[i] = new Item(-3, i, true, item.GfxPath, aStartPos + aChangeInPos * (i - (int)aStart), itemSize);
                }
                else
                {
                    equiped[i] = new Item(-3, i, true, new GfxPath(GfxType.Item, null), aStartPos + aChangeInPos * (i - (int)aStart), itemSize);
                }
            }
        }

        public void SetSlot(Equipment.Slot aSlot)
        {
            equiped[(int)aSlot].AssignItem(owner.Equipment.EquipedInSlot(aSlot));
        }

        public void SetReportBox(PairReport aReport)
        {
            stringStatReport.Text = aReport.StringsOnly;
            nrStatReport.Text = aReport.NumbersOnly;
        }

        public void RefreshExp()
        {
            expBar.MaxValue = Level.ExpToNextLevel(owner.Level.CurrentLevel);
            expBar.Value = owner.Level.Experience;
        }
    }
}
