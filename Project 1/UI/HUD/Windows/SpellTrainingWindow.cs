using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using Project_1.UI.UIElements.Buttons;
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
        readonly ScrollableBox<SpellTrainingEntryButton> entriesBox;
        readonly Button sortLevelButton;
        readonly Button sortNameButton;
        readonly DescriptCheckBox hideLearnedCheckBox;
        readonly DescriptCheckBox hideUnavailableCheckBox;
        readonly Label selectedLabel;
        readonly Label selectedStatusLabel;
        readonly Button buyButton;

        SpellTrainingEntrySnapshot[] entries = Array.Empty<SpellTrainingEntrySnapshot>();
        SpellTrainingEntrySnapshot selectedEntry;
        SortMode sortMode = SortMode.LevelRequired;
        string trainerName = string.Empty;
        bool hideLearned;
        bool hideUnavailable;
        Color clickableButton;
        Color unclickableButton;

        public SpellTrainingWindow() : base(new UITexture("WhiteBackground", Color.DarkSlateGray))
        {
            clickableButton = Color.ForestGreen;
            unclickableButton = Color.Maroon;

            RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.03f, Size);
            titleLabel = new Label(this, spacing, new RelativeScreenPosition(0.94f, 0.08f), Label.TextAllignment.CentreLeft, Color.White, aTextSize: 14f);

            sortLevelButton = new Button(this, new RelativeScreenPosition(0.05f, 0.10f), new RelativeScreenPosition(0.28f, 0.07f), Color.SteelBlue, "Level Req", Color.White);
            sortLevelButton.AddAction(() => SetSortMode(SortMode.LevelRequired));

            sortNameButton = new Button(this, new RelativeScreenPosition(0.35f, 0.10f), new RelativeScreenPosition(0.20f, 0.07f), Color.SlateBlue, "Name", Color.White);
            sortNameButton.AddAction(() => SetSortMode(SortMode.Name));

            hideLearnedCheckBox = new DescriptCheckBox(
                this,
                "Hide learned",
                Color.White,
                false,
                () => SetHideLearned(true),
                () => SetHideLearned(false),
                new RelativeScreenPosition(0.58f, 0.10f),
                new RelativeScreenPosition(0.16f, 0.07f),
                Size);

            hideUnavailableCheckBox = new DescriptCheckBox(
                this,
                "Hide unavailable",
                Color.White,
                false,
                () => SetHideUnavailable(true),
                () => SetHideUnavailable(false),
                new RelativeScreenPosition(0.75f, 0.10f),
                new RelativeScreenPosition(0.20f, 0.07f),
                Size);

            entriesBox = new ScrollableBox<SpellTrainingEntryButton>(this, 10, UITexture.Null, Color.LightGray, new RelativeScreenPosition(0.05f, 0.19f), new RelativeScreenPosition(0.90f, 0.56f));

            selectedLabel = new Label(this, new RelativeScreenPosition(0.05f, 0.77f), new RelativeScreenPosition(0.90f, 0.08f), Label.TextAllignment.CentreLeft, Color.White, aTextSize: 12f, aText: "Select a spell.");

            selectedStatusLabel = new Label(this, new RelativeScreenPosition(0.05f, 0.84f), new RelativeScreenPosition(0.55f, 0.08f), Label.TextAllignment.CentreLeft, Color.Gainsboro, aTextSize: 11f);

            buyButton = new Button(this, new RelativeScreenPosition(0.72f, 0.82f), new RelativeScreenPosition(0.18f, 0.09f), Color.ForestGreen, "Buy", Color.White);
            buyButton.AddAction(BuySelectedSpell);

            SetSortMode(SortMode.LevelRequired); //TODO: Load and save this from settings 
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
            sortLevelButton.Color = mode == SortMode.LevelRequired ? Color.MediumPurple : Color.SteelBlue;
            sortNameButton.Color = mode == SortMode.Name ? Color.MediumPurple : Color.SlateBlue;
            RefreshList();
        }

        void SetHideLearned(bool value)
        {
            hideLearned = value;
            RefreshList();
        }

        void SetHideUnavailable(bool value)
        {
            hideUnavailable = value;
            RefreshList();
        }

        void RefreshSort()
        {
            entriesBox.Sort(new Sorter(sortMode));
        }

        private class Sorter : IComparer<SpellTrainingEntryButton>
        {
            private readonly SortMode sortMode;
            public Sorter(SortMode sortMode)
            {
                this.sortMode = sortMode;
            }
            public int Compare(SpellTrainingEntryButton x, SpellTrainingEntryButton y)
            {
                if (x == null || y == null) return 0;
                if (sortMode == SortMode.LevelRequired)
                {
                    int levelComparison = x.Snapshot.RequiredLevel.CompareTo(y.Snapshot.RequiredLevel);
                    if (levelComparison != 0) return levelComparison;
                }
                int nameComparison = string.Compare(x.Snapshot.DisplayName, y.Snapshot.DisplayName, StringComparison.Ordinal);
                if (nameComparison != 0) return nameComparison;
                return x.Snapshot.RequiredLevel.CompareTo(y.Snapshot.RequiredLevel);
            }
        }

        void RefreshList()
        {
            SpellTrainingEntrySnapshot[] ordered = entries
                .Where(ShouldShowEntry)
                .OrderBy(x => sortMode == SortMode.LevelRequired ? x.RequiredLevel : int.MinValue)
                .ThenBy(x => x.DisplayName)
                .ThenBy(x => x.RequiredLevel)
                .ToArray();

            if (!string.IsNullOrWhiteSpace(selectedEntry.SpellKey) && !ordered.Any(x => x.SpellKey == selectedEntry.SpellKey))
            {
                selectedEntry = default;
            }

            EnsureEntryCapacity(ordered.Length);
            for (int i = 0; i < ordered.Length; i++)
            {
                bool isSelected = ordered[i].SpellKey == selectedEntry.SpellKey;
                entriesBox[i].Set(ordered[i], isSelected);
            }

            for (int i = ordered.Length; i < entriesBox.ScrollableElementsCount; i++)
            {
                entriesBox[i].Clear();
            }

            entriesBox.SetScrollValue(0f);
            RefreshSelectionDetails();
        }

        void EnsureEntryCapacity(int count)
        {
            while (entriesBox.ScrollableElementsCount < count)
            {
                SpellTrainingEntryButton button = new SpellTrainingEntryButton(entriesBox, SelectEntry);
                entriesBox.AddScrollableElement(button);
            }
        }

        void SelectEntry(SpellTrainingEntrySnapshot snapshot)
        {
            selectedEntry = snapshot;
            RefreshList();
        }

        bool ShouldShowEntry(SpellTrainingEntrySnapshot entry)
        {
            if (hideLearned && entry.Learned) return false;
            if (hideUnavailable && !entry.Learnable) return false;
            return true;
        }

        void RefreshSelectionDetails()
        {
            if (string.IsNullOrWhiteSpace(selectedEntry.SpellKey))
            {
                selectedLabel.Text = "Select a spell.";
                selectedStatusLabel.Text = "";
                buyButton.Color = unclickableButton;
                return;
            }

            selectedLabel.Text = $"{selectedEntry.DisplayName}";

            bool affordable = UiPlayerStateCache.Gold >= selectedEntry.Cost;

            if (selectedEntry.Learned || !selectedEntry.PassRankReq || !selectedEntry.PassLevelReq || !affordable)
            {
                buyButton.Color = unclickableButton;
                if (selectedEntry.Learned) selectedStatusLabel.Text = "Already learned";
                else if (!selectedEntry.PassLevelReq && !selectedEntry.PassRankReq) selectedStatusLabel.Text = $"Requires level {selectedEntry.RequiredLevel} and previous ranks";
                else if(!selectedEntry.PassLevelReq) selectedStatusLabel.Text = $"Requires level {selectedEntry.RequiredLevel}";
                else if (!selectedEntry.PassRankReq) selectedStatusLabel.Text = $"Requires previous ranks";
                else if (!affordable) selectedStatusLabel.Text = "Not enough gold";

                return;
            }
            selectedStatusLabel.Text = $"Available to learn for {selectedEntry.Cost} gold.";
            buyButton.Color = clickableButton;
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
