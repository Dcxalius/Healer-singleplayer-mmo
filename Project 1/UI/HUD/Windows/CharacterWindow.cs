using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Unit;
using Project_1.Input;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Bars;
using System;

namespace Project_1.UI.HUD.Windows
{
    internal class CharacterWindow : Window
    {
        enum CharacterWindowPage
        {
            CharacterPane,
            Skills
        }

        sealed class CharacterWindowTabButton : ClipChild
        {
            readonly Action action;
            readonly Label label;
            readonly Color normalColor;
            readonly Color selectedColor;
            bool selected;

            public CharacterWindowTabButton(UIElement parent, string text, Action action)
                : base(parent, new UITexture("WhiteBackground", Color.Teal), RelativeScreenPosition.Zero, new RelativeScreenPosition(0.01f, 0.01f))
            {
                this.action = action;
                normalColor = Color.Teal;
                selectedColor = Color.DarkCyan;
                label = new Label(this, RelativeScreenPosition.Zero, RelativeScreenPosition.One, Label.TextAllignment.Centred, Color.White, "Comfortaa-msdf", 10f, text);
                CapturesClick = true;
                CapturesRelease = true;
            }

            public bool Selected
            {
                get => selected;
                set
                {
                    if (selected == value) return;
                    selected = value;
                    Color = selected ? selectedColor : normalColor;
                }
            }

            protected override void ClickedOnMe(ClickEvent aClick)
            {
                Color = Microsoft.Xna.Framework.Color.DarkSlateGray;
                base.ClickedOnMe(aClick);
            }

            public override void ClickedOnAndReleasedOnMe()
            {
                action?.Invoke();
                Color = selected ? selectedColor : normalColor;
                base.ClickedOnAndReleasedOnMe();
            }

            protected override void HoldReleaseAwayFromMe()
            {
                Color = selected ? selectedColor : normalColor;
                base.HoldReleaseAwayFromMe();
            }
        }

        protected Label nameLabel;
        CharacterEquipmentPanel equipmentPanel;
        CharacterStatReportBox statReportBox;
        ExpBar expBar;
        CharacterWeaponSkillBox weaponSkillBox;
        CharacterWindowTabButton characterPaneButton;
        CharacterWindowTabButton skillsButton;
        CharacterWindowPage activePage;

        protected virtual int BagIndexForItem => -3;

        static RelativeScreenPosition expBarSize = new RelativeScreenPosition(1f, 0.05f);
        static RelativeScreenPosition expBarPos = new RelativeScreenPosition(0, 1f - expBarSize.Y);
        const int TabButtonSize = 42;
        const int TabButtonGap = 4;

        public CharacterWindow() : base(new UITexture("WhiteBackground", Color.Turquoise))
        {
            nameLabel = new Label(this, new RelativeScreenPosition(0, 0), new RelativeScreenPosition(1, CharacterEquipmentPanel.ItemSize.Y), Label.TextAllignment.TopCentre, Color.Black);

            visibleKey = Input.KeyBindManager.KeyListner.Character;
            equipmentPanel = new CharacterEquipmentPanel(this, BagIndexForItem);

            RelativeScreenPosition reportBoxPos = CharacterEquipmentPanel.StatsTop + CharacterEquipmentPanel.ItemSpacing;
            RelativeScreenPosition reportBoxSize = new RelativeScreenPosition(1 - CharacterEquipmentPanel.ItemSpacing.X * 2, 1 - CharacterEquipmentPanel.StatsTop.Y - CharacterEquipmentPanel.ItemSpacing.Y * 2 - expBarSize.Y);

            statReportBox = new CharacterStatReportBox(this, reportBoxPos, reportBoxSize);

            expBar = new ExpBar(this, expBarPos, expBarSize);
            RelativeScreenPosition skillBoxPos = CharacterEquipmentPanel.ItemSpacing + CharacterEquipmentPanel.ItemSize.OnlyY;
            RelativeScreenPosition skillBoxSize = new RelativeScreenPosition(1 - CharacterEquipmentPanel.ItemSpacing.X * 2, 1 - CharacterEquipmentPanel.ItemSize.Y - CharacterEquipmentPanel.ItemSpacing.Y * 2 - expBarSize.Y);
            weaponSkillBox = new CharacterWeaponSkillBox(this, skillBoxPos, skillBoxSize);

            characterPaneButton = ClipRight(new CharacterWindowTabButton(this, "Char\nPane", () => SetPage(CharacterWindowPage.CharacterPane)), 0f);
            characterPaneButton.Resize(new AbsoluteScreenPosition(TabButtonSize, TabButtonSize));
            skillsButton = ClipRight(new CharacterWindowTabButton(this, "Skills", () => SetPage(CharacterWindowPage.Skills)), TabButtonSize + TabButtonGap);
            skillsButton.Resize(new AbsoluteScreenPosition(TabButtonSize, TabButtonSize));

            expBar.Visible = true;
            SetPage(CharacterWindowPage.CharacterPane);
        }

        public virtual void SetData(CharacterWindowSnapshot snapshot)
        {
            nameLabel.Text = snapshot.OwnerSnapshot.Name;
            SetOwnerContext(snapshot.OwnerSnapshot);
            SetReportBox(snapshot.PrimaryStats, snapshot.SecondaryStats);
            RefreshExp(snapshot.CurrentLevel, snapshot.CurrentExperience);
            SetAllSlots(snapshot.EquippedItems);
            weaponSkillBox.SetSkills(snapshot.WeaponSkills);
        }

        void SetPage(CharacterWindowPage page)
        {
            activePage = page;
            bool characterPaneVisible = activePage == CharacterWindowPage.CharacterPane;
            equipmentPanel.Visible = characterPaneVisible;
            statReportBox.Visible = characterPaneVisible;
            expBar.Visible = characterPaneVisible;
            weaponSkillBox.Visible = activePage == CharacterWindowPage.Skills;
            characterPaneButton.Selected = characterPaneVisible;
            skillsButton.Selected = !characterPaneVisible;
        }

        protected void SetOwnerContext(in EntityUiSnapshot aOwnerSnapshot)
        {
            SetOwnerContext(aOwnerSnapshot.ClassName, aOwnerSnapshot.RelationToPlayer);
        }

        protected void SetOwnerContext(string aClassName, RelationToPlayerKind aRelation)
        {
            statReportBox.SetOwnerContext(aClassName, aRelation);
        }

        public void SetSlot(Equipment.Slot aSlot, ItemUiSnapshot itemSnapshot)
        {
            equipmentPanel.SetSlot(aSlot, itemSnapshot);
        }

        public void SetReportBox(StatReportSnapshot primary, StatReportSnapshot secondary)
        {
            statReportBox.SetReport(primary, secondary);
        }

        public void RefreshExp(int currentLevel, int currentExperience)
        {
            expBar.MaxValue = Level.ExpToNextLevel(currentLevel);
            expBar.Value = currentExperience;
        }

        void SetAllSlots(ItemUiSnapshot[] items)
        {
            equipmentPanel.SetAllSlots(items);
        }
    }
}
