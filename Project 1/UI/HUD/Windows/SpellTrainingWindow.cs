using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Buttons;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project_1.UI.HUD.Windows
{
    internal class SpellTrainingWindow : Window
    {
        enum SortMode
        {
            LevelRequired,
            Name
        }

        readonly Label titleLabel;
        readonly ScrollableBox entriesBox;
        readonly List<SpellTrainingEntryButton> entryButtons;
        readonly Button sortLevelButton;
        readonly Button sortNameButton;
        readonly Label selectedLabel;
        readonly Label selectedStatusLabel;
        readonly Button buyButton;

        SpellTrainingEntrySnapshot[] entries = Array.Empty<SpellTrainingEntrySnapshot>();
        SpellTrainingEntrySnapshot selectedEntry;
        SortMode sortMode = SortMode.LevelRequired;
        string trainerName = string.Empty;

        public SpellTrainingWindow() : base(new UITexture("WhiteBackground", Color.DarkSlateGray))
        {
            RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.03f, Size);
            titleLabel = new Label("", spacing, new RelativeScreenPosition(0.94f, 0.08f), Label.TextAllignment.CentreLeft, Color.White, aTextSize: 14f);
            AddChild(titleLabel);

            sortLevelButton = new Button(new RelativeScreenPosition(0.05f, 0.10f), new RelativeScreenPosition(0.28f, 0.07f), Color.SteelBlue, "Level Req", Color.White);
            sortLevelButton.AddAction(() => SetSortMode(SortMode.LevelRequired));
            AddChild(sortLevelButton);

            sortNameButton = new Button(new RelativeScreenPosition(0.35f, 0.10f), new RelativeScreenPosition(0.20f, 0.07f), Color.SlateBlue, "Name", Color.White);
            sortNameButton.AddAction(() => SetSortMode(SortMode.Name));
            AddChild(sortNameButton);

            entriesBox = new ScrollableBox(10, UITexture.Null, Color.LightGray, new RelativeScreenPosition(0.05f, 0.19f), new RelativeScreenPosition(0.90f, 0.56f));
            AddChild(entriesBox);

            entryButtons = new List<SpellTrainingEntryButton>();

            selectedLabel = new Label("Select a spell.", new RelativeScreenPosition(0.05f, 0.77f), new RelativeScreenPosition(0.90f, 0.08f), Label.TextAllignment.CentreLeft, Color.White, aTextSize: 12f);
            AddChild(selectedLabel);

            selectedStatusLabel = new Label("", new RelativeScreenPosition(0.05f, 0.84f), new RelativeScreenPosition(0.55f, 0.08f), Label.TextAllignment.CentreLeft, Color.Gainsboro, aTextSize: 11f);
            AddChild(selectedStatusLabel);

            buyButton = new Button(new RelativeScreenPosition(0.72f, 0.82f), new RelativeScreenPosition(0.18f, 0.09f), Color.ForestGreen, "Buy", Color.White);
            buyButton.AddAction(BuySelectedSpell);
            AddChild(buyButton);

            SetSortMode(SortMode.LevelRequired);
            ClearTrainer();
        }

        public void OpenTrainer(SpellTrainingEntrySnapshot[] entries, string trainerName)
        {
            this.entries = entries ?? Array.Empty<SpellTrainingEntrySnapshot>();
            this.trainerName = trainerName ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(selectedEntry.SpellKey))
            {
                selectedEntry = this.entries.FirstOrDefault(x => x.SpellKey == selectedEntry.SpellKey);
            }
            if (string.IsNullOrWhiteSpace(selectedEntry.SpellKey))
            {
                selectedEntry = default;
            }
            titleLabel.Text = string.IsNullOrWhiteSpace(this.trainerName) ? "Spell Training" : this.trainerName;
            RefreshList();
            RefreshSelectionDetails();
        }

        public void ClearTrainer()
        {
            entries = Array.Empty<SpellTrainingEntrySnapshot>();
            trainerName = string.Empty;
            selectedEntry = default;
            titleLabel.Text = "Spell Training";
            RefreshList();
            RefreshSelectionDetails();
        }

        void SetSortMode(SortMode mode)
        {
            sortMode = mode;
            sortLevelButton.Color = mode == SortMode.LevelRequired ? Color.CornflowerBlue : Color.SteelBlue;
            sortNameButton.Color = mode == SortMode.Name ? Color.MediumPurple : Color.SlateBlue;
            RefreshList();
        }

        void RefreshList()
        {
            SpellTrainingEntrySnapshot[] ordered = entries
                .OrderBy(x => sortMode == SortMode.LevelRequired ? x.RequiredLevel : int.MinValue)
                .ThenBy(x => x.DisplayName)
                .ThenBy(x => x.RequiredLevel)
                .ToArray();

            EnsureEntryCapacity(ordered.Length);
            for (int i = 0; i < entryButtons.Count; i++)
            {
                if (i >= ordered.Length)
                {
                    entryButtons[i].Clear();
                    continue;
                }

                bool isSelected = ordered[i].SpellKey == selectedEntry.SpellKey;
                entryButtons[i].Set(ordered[i], isSelected);
            }

            entriesBox.SetScrollValue(0f);
        }

        void EnsureEntryCapacity(int count)
        {
            while (entryButtons.Count < count)
            {
                SpellTrainingEntryButton button = new SpellTrainingEntryButton(SelectEntry);
                entryButtons.Add(button);
                entriesBox.AddScrollableElement(button);
            }
        }

        void SelectEntry(SpellTrainingEntrySnapshot snapshot)
        {
            selectedEntry = snapshot;
            RefreshList();
            RefreshSelectionDetails();
        }

        void RefreshSelectionDetails()
        {
            if (string.IsNullOrWhiteSpace(selectedEntry.SpellKey))
            {
                selectedLabel.Text = "Select a spell.";
                selectedStatusLabel.Text = "";
                buyButton.Color = Color.DarkOliveGreen;
                return;
            }

            selectedLabel.Text = $"{selectedEntry.DisplayName} - Cost {selectedEntry.Cost}";

            if (selectedEntry.Learned)
            {
                selectedStatusLabel.Text = "Already learned";
                buyButton.Color = Color.DarkOliveGreen;
                return;
            }

            bool affordable = UiPlayerStateCache.Gold >= selectedEntry.Cost;
            if (selectedEntry.Learnable && affordable)
            {
                selectedStatusLabel.Text = "Available to learn";
                buyButton.Color = Color.ForestGreen;
                return;
            }

            if (!affordable)
            {
                selectedStatusLabel.Text = "Not enough gold";
                buyButton.Color = Color.Maroon;
                return;
            }

            selectedStatusLabel.Text = $"Requires level {selectedEntry.RequiredLevel} and previous ranks";
            buyButton.Color = Color.DarkOliveGreen;
        }

        void BuySelectedSpell()
        {
            if (string.IsNullOrWhiteSpace(selectedEntry.SpellKey)) return;
            if (selectedEntry.Learned || !selectedEntry.Learnable) return;
            if (UiPlayerStateCache.Gold < selectedEntry.Cost) return;
            MailboxManager.PublishSimCommand(new SpellTrainingPurchaseRequested(selectedEntry.SpellKey));
        }
    }
}
