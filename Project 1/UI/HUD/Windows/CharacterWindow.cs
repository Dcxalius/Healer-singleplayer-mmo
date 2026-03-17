using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Unit;
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
        protected Label nameLabel;
        CharacterEquipmentPanel equipmentPanel;
        CharacterStatReportBox statReportBox;
        ExpBar expBar;

        protected virtual int BagIndexForItem => -3;

        static RelativeScreenPosition expBarSize = new RelativeScreenPosition(1f, 0.05f);
        static RelativeScreenPosition expBarPos = new RelativeScreenPosition(0, 1f - expBarSize.Y);

        public CharacterWindow() : base(new UITexture("WhiteBackground", Color.Turquoise))
        {
            nameLabel = new Label(null, new RelativeScreenPosition(0, 0), new RelativeScreenPosition(1, CharacterEquipmentPanel.ItemSize.Y), Label.TextAllignment.TopCentre, Color.Black);

            visibleKey = Input.KeyBindManager.KeyListner.Character;
            equipmentPanel = new CharacterEquipmentPanel(BagIndexForItem);

            RelativeScreenPosition reportBoxPos = CharacterEquipmentPanel.StatsTop + CharacterEquipmentPanel.ItemSpacing;
            RelativeScreenPosition reportBoxSize = new RelativeScreenPosition(1 - CharacterEquipmentPanel.ItemSpacing.X * 2, 1 - CharacterEquipmentPanel.StatsTop.Y - CharacterEquipmentPanel.ItemSpacing.Y * 2 - expBarSize.Y);

            statReportBox = new CharacterStatReportBox(reportBoxPos, reportBoxSize);

            expBar = new ExpBar(expBarPos, expBarSize);
            AddChild(nameLabel);
            AddChild(equipmentPanel);
            AddChild(statReportBox);
            AddChild(expBar);
        }

        public virtual void SetData(CharacterWindowSnapshot snapshot)
        {
            nameLabel.Text = snapshot.OwnerSnapshot.Name;
            SetOwnerContext(snapshot.OwnerSnapshot);
            SetReportBox(snapshot.PrimaryStats, snapshot.SecondaryStats);
            RefreshExp(snapshot.CurrentLevel, snapshot.CurrentExperience);
            SetAllSlots(snapshot.EquippedItems);
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
