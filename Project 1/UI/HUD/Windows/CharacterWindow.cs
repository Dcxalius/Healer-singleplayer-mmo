using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Entities.Friendlies;
using Project_1.GameObjects.Unit;
using Project_1.Textures;
using Project_1.UI.HUD.Inventory;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Bars;
using Project_1.UI.UIElements.Boxes;
using Project_1.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Project_1.UI.HUD.Windows
{
    internal class CharacterWindow : Window
    {
        sealed class StatLineElement : UIElement
        {
            readonly Label numberLabel;
            readonly Label textLabel;

            public StatLineElement(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(null, aPos, aSize)
            {
                numberLabel = new Label(null, new RelativeScreenPosition(0f, 0f), new RelativeScreenPosition(0.45f, 1f), Label.TextAllignment.CentreRight, Color.Black);
                textLabel = new Label(null, new RelativeScreenPosition(0.5f, 0f), new RelativeScreenPosition(0.5f, 1f), Label.TextAllignment.CentreLeft, Color.Black);
                AddChild(numberLabel);
                AddChild(textLabel);

                CapturesClick = false;
                CapturesRelease = false;
                CapturesScroll = false;
            }

            public void Set(string aNumber, string aText)
            {
                numberLabel.Text = aNumber;
                textLabel.Text = aText;
            }

            public void Clear()
            {
                numberLabel.Text = null;
                textLabel.Text = null;
            }
        }

        protected Label nameLabel;
        Item[] equiped;
        PageBox statPageBox;
        StatLineElement[] statLines;
        ExpBar expBar;

        protected virtual int BagIndexForItem => -3;

        PairReport primaryReport = new PairReport();
        PairReport secondaryReport = new PairReport();
        Friendly owner;

        const int StatRowsPerPage = 5;
        static readonly Point StatPageSize = new Point(1, StatRowsPerPage);

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
            RelativeScreenPosition reportBoxPos = topPart + itemSpacing;
            RelativeScreenPosition reportBoxSize = new RelativeScreenPosition(1 - itemSpacing.X * 2, 1 - topPart.Y - itemSpacing.Y * 2) / 2;

            statPageBox = new PageBox(new UITexture("WhiteBackground", Color.Transparent), reportBoxPos, reportBoxSize, StatPageSize);
            statLines = new StatLineElement[StatRowsPerPage];
            for (int i = 0; i < statLines.Length; i++)
            {
                float rowHeight = 1f / StatRowsPerPage;
                float y = rowHeight * i;
                statLines[i] = new StatLineElement(new RelativeScreenPosition(0.05f, y), new RelativeScreenPosition(0.9f, rowHeight));
            }
            statPageBox.SetPageElements(statLines, BindStatLine, ClearStatLine);

            expBar = new ExpBar(expBarPos, expBarSize);
            AddChild(nameLabel);
            AddChildren(equiped);
            AddChild(statPageBox);
            AddChild(expBar);
        }

        public virtual void SetData(Friendly aFriendly)
        {
            owner = aFriendly;
            nameLabel.Text = aFriendly.Name;
            SetReportBox(aFriendly.PrimaryStatReport, BuildSecondaryReport(aFriendly));
            RefreshExp(aFriendly.Level);
            SetAllSlots(aFriendly.Equipment);
        }

        public virtual void SetData(CharacterWindowSnapshot snapshot)
        {
            owner = null;
            nameLabel.Text = snapshot.OwnerSnapshot.Name;
            SetReportBox(snapshot.PrimaryStats, snapshot.SecondaryStats);
            RefreshExp(snapshot.CurrentLevel, snapshot.CurrentExperience);
            SetAllSlots(snapshot.EquippedItems);
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

        public void SetSlot(Equipment.Slot aSlot, Project_1.Items.Item itemSnapshot)
        {
            equiped[(int)aSlot].AssignItem(itemSnapshot);
        }

        void SetAllSlots(Project_1.Items.Item[] items)
        {
            if (items == null) return;
            int count = Math.Min(items.Length, equiped.Length);
            for (int i = 0; i < count; i++)
            {
                equiped[i].AssignItem(items[i]);
            }
        }

        public void SetReportBox(PairReport aReport)
        {
            primaryReport = aReport ?? new PairReport();
            secondaryReport = BuildSecondaryReport(owner);
            RefreshStatPage();
        }

        public void SetReportBox(PairReport primary, PairReport secondary)
        {
            primaryReport = primary ?? new PairReport();
            secondaryReport = secondary ?? new PairReport();
            RefreshStatPage();
        }

        void RefreshStatPage()
        {
            int currentPage = statPageBox.CurrentPage;
            int pageCount = ReportHasAnyLine(secondaryReport) ? 2 : 1;
            statPageBox.Reset(pageCount * statPageBox.ItemsPerPage);
            statPageBox.SetPage(Math.Min(currentPage, pageCount - 1));
        }

        static bool ReportHasAnyLine(PairReport aReport)
        {
            return aReport != null && aReport.Count > 0;
        }

        void BindStatLine(UIElement aElement, int aIndex)
        {
            StatLineElement line = aElement as StatLineElement;
            if (line == null) return;

            int page = aIndex / statPageBox.ItemsPerPage;
            int lineOnPage = aIndex % statPageBox.ItemsPerPage;
            PairReport report = page == 0 ? primaryReport : secondaryReport;
            if (report == null || lineOnPage >= report.Count)
            {
                line.Clear();
                return;
            }

            (string Name, double Value) pair = report.Lines[lineOnPage];
            line.Set(FormatValue(pair.Value), pair.Name);
        }

        static void ClearStatLine(UIElement aElement)
        {
            StatLineElement line = aElement as StatLineElement;
            if (line == null) return;
            line.Clear();
        }

        static string FormatValue(double aValue)
        {
            double rounded = Math.Round(aValue);
            if (Math.Abs(aValue - rounded) < 0.01d)
            {
                return rounded.ToString(CultureInfo.InvariantCulture);
            }

            return aValue.ToString("0.##", CultureInfo.InvariantCulture);
        }

        static PairReport BuildSecondaryReport(Friendly aFriendly) //TODO: I think the future implementation of secondary stats will be that the user self can set what stats to show per class through the option menu
        {
            PairReport report = new PairReport();
            if (aFriendly == null) return report;

            report.AddLine("Crit Chance", aFriendly.SecondaryStats.Attack.CriticalChance);
            report.AddLine("Crit Damage", aFriendly.SecondaryStats.Attack.CriticalDamage);
            report.AddLine("Hit Chance", aFriendly.SecondaryStats.Attack.BonusHitChance);
            report.AddLine("Dodge Chance", aFriendly.SecondaryStats.Defense.DodgeChance);
            report.AddLine("Parry Chance", aFriendly.SecondaryStats.Defense.ParryChance);
            return report;
        }

        public void RefreshExp(Level aLevel)
        {
            expBar.MaxValue = Level.ExpToNextLevel(aLevel.CurrentLevel);
            expBar.Value = aLevel.Experience;
        }

        public void RefreshExp(int currentLevel, int currentExperience)
        {
            expBar.MaxValue = Level.ExpToNextLevel(currentLevel);
            expBar.Value = currentExperience;
        }
    }
}
