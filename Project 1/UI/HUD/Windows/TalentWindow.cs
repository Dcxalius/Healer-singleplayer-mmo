using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.HUD.Managers;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;

namespace Project_1.UI.HUD.Windows
{
    internal sealed class TalentWindow : Window
    {
        const int TalentUnlockLevel = 10;

        sealed class TalentTreePage : UIElement
        {
            sealed class TalentSlotElement : UIElement, IPageBoxLabeledElement
            {
                readonly TalentWindow ownerWindow;
                readonly Box frame;
                readonly Image icon;
                readonly Label rankLabel;

                TalentUiSnapshot snapshot;
                bool hasTalent;
                public string PageBoxLabel => hasTalent ? snapshot.Name : null;
                public bool HasTalent => hasTalent;
                public int TalentId => snapshot.Id;
                public int[] RequiredTalentIds => hasTalent ? snapshot.RequiredTalentIds : Array.Empty<int>();
                public Point TopAnchor => new Point(AbsolutePos.Center.X, AbsolutePos.Top);
                public Point BottomAnchor => new Point(AbsolutePos.Center.X, AbsolutePos.Bottom);

                public TalentSlotElement(UIElement aParent, TalentWindow aWindow, RelativeScreenPosition aPos, RelativeScreenPosition aSize)
                    : base(aParent, null, aPos, aSize)
                {
                    ownerWindow = aWindow;
                    frame = new Box(this, new UITexture("GrayWhiteBorder", Color.White), RelativeScreenPosition.Zero, RelativeScreenPosition.One);
                    icon = new Image(this, new UITexture(GfxPath.NullPath, Color.White), new RelativeScreenPosition(0.08f, 0.08f), new RelativeScreenPosition(0.84f, 0.7f));
                    rankLabel = new Label(this, new RelativeScreenPosition(0.05f, 0.8f), new RelativeScreenPosition(0.9f, 0.14f), Label.TextAllignment.CentreRight, Color.White, aTextSize: 9f);
                    frame.CapturesClick = false;
                    frame.CapturesRelease = false;
                    frame.CapturesScroll = false;

                    Visible = false;
                    CapturesClick = true;
                    CapturesRelease = true;
                    CapturesScroll = false;
                }

                public void SetTalent(TalentUiSnapshot aSnapshot)
                {
                    snapshot = aSnapshot;
                    hasTalent = true;
                    icon.SetImage(aSnapshot.GfxPath);
                    icon.Color = aSnapshot.Rank > 0 ? Color.White : Color.DarkGray;
                    rankLabel.Text = $"{aSnapshot.Rank}/{aSnapshot.MaxRank}";
                    Visible = true;
                }

                public void ClearTalent()
                {
                    if (hasTalent && isHovered)
                    {
                        MailboxManager.PublishUiEvent(new DescriptorBoxClear());
                    }

                    hasTalent = false;
                    icon.ClearImage();
                    icon.Color = Color.White;
                    rankLabel.Text = null;
                    Visible = false;
                }

                protected override void OnHover()
                {
                    base.OnHover();
                    if (!hasTalent) return;
                    MailboxManager.PublishUiEvent(new DescriptorBoxSet(BuildDescriptorSnapshot(snapshot), RelativePositionOnScreen.ToAbsoluteScreenPos()));
                }

                protected override void OnDeHover()
                {
                    base.OnDeHover();
                    if (!hasTalent) return;
                    MailboxManager.PublishUiEvent(new DescriptorBoxClear());
                }

                static ItemDescriptorSnapshot BuildDescriptorSnapshot(TalentUiSnapshot aSnapshot)
                {
                    return new ItemDescriptorSnapshot(
                        $"{aSnapshot.Name} ({aSnapshot.Rank}/{aSnapshot.MaxRank})",
                        aSnapshot.Description,
                        $"Current Rank: {aSnapshot.Rank}/{aSnapshot.MaxRank}",
                        0,
                        true,
                        false);
                }

                public override void ClickedOnAndReleasedOnMe()
                {
                    if (hasTalent)
                    {
                        ownerWindow.TrySpendTalent(snapshot.Id);
                    }

                    base.ClickedOnAndReleasedOnMe();
                }
            }

            sealed class TalentArrowOverlay : UIElement
            {
                const int LineThickness = 3;
                const int ArrowHeadSize = 12;
                const int BaseSize = 6;

                readonly TalentTreePage owner;
                readonly UITexture lineTexture;
                readonly UITexture arrowHeadTexture;

                public TalentArrowOverlay(UIElement aParent, TalentTreePage aOwner, RelativeScreenPosition aPos, RelativeScreenPosition aSize)
                    : base(aParent, null, aPos, aSize)
                {
                    owner = aOwner;
                    lineTexture = new UITexture("WhiteBackground", new Color(60, 60, 60));
                    arrowHeadTexture = new UITexture("DownArrow", new Color(60, 60, 60));
                    CapturesClick = false;
                    CapturesRelease = false;
                    CapturesScroll = false;
                }

                public override void Draw(SpriteBatch aBatch)
                {
                    ThreadAffinity.AssertMainThread();
                    if (!Visible) return;

                    Dictionary<int, TalentSlotElement> slotsById = new Dictionary<int, TalentSlotElement>();
                    for (int i = 0; i < owner.slots.Length; i++)
                    {
                        TalentSlotElement slot = owner.slots[i];
                        if (!slot.HasTalent) continue;
                        slotsById[slot.TalentId] = slot;
                    }

                    for (int i = 0; i < owner.slots.Length; i++)
                    {
                        TalentSlotElement target = owner.slots[i];
                        if (!target.HasTalent) continue;

                        int[] requiredIds = target.RequiredTalentIds;
                        for (int j = 0; j < requiredIds.Length; j++)
                        {
                            if (slotsById.TryGetValue(requiredIds[j], out TalentSlotElement source))
                            {
                                DrawConnector(aBatch, source, target);
                            }
                        }
                    }
                }

                void DrawConnector(SpriteBatch batch, TalentSlotElement source, TalentSlotElement target)
                {
                    Point from = source.BottomAnchor;
                    Point tip = target.TopAnchor;
                    int arrowTop = tip.Y - ArrowHeadSize;
                    int shaftEndY = arrowTop + ArrowHeadSize / 2;
                    int midY = from.Y + Math.Max(8, (shaftEndY - from.Y) / 2);

                    DrawBase(batch, from);
                    DrawVertical(batch, from.X, from.Y, midY);
                    DrawHorizontal(batch, from.X, tip.X, midY);
                    DrawVertical(batch, tip.X, midY, shaftEndY);
                    DrawArrowHead(batch, tip.X, arrowTop);
                }

                void DrawBase(SpriteBatch batch, Point at)
                {
                    Rectangle rect = new Rectangle(at.X - BaseSize / 2, at.Y - BaseSize / 2, BaseSize, BaseSize);
                    lineTexture.Draw(batch, rect);
                }

                void DrawVertical(SpriteBatch batch, int x, int startY, int endY)
                {
                    int top = Math.Min(startY, endY);
                    int height = Math.Max(LineThickness, Math.Abs(endY - startY));
                    Rectangle rect = new Rectangle(x - LineThickness / 2, top, LineThickness, height);
                    lineTexture.Draw(batch, rect);
                }

                void DrawHorizontal(SpriteBatch batch, int startX, int endX, int y)
                {
                    int left = Math.Min(startX, endX);
                    int width = Math.Max(LineThickness, Math.Abs(endX - startX));
                    Rectangle rect = new Rectangle(left, y - LineThickness / 2, width, LineThickness);
                    lineTexture.Draw(batch, rect);
                }

                void DrawArrowHead(SpriteBatch batch, int x, int top)
                {
                    Rectangle rect = new Rectangle(x - ArrowHeadSize / 2, top, ArrowHeadSize, ArrowHeadSize);
                    arrowHeadTexture.Draw(batch, rect);
                }
            }

            const int MaxColumns = 4;
            const int MaxRows = 7;
            const float GridTop = 0.08f;
            const float GridHeight = 0.83f;
            readonly Image background;
            readonly Box backgroundShade;
            readonly Label spentPointsLabel;
            readonly TalentArrowOverlay arrowOverlay;
            readonly PageBox talentGrid;
            readonly TalentSlotElement[] slots;

            TalentUiSnapshot?[] talents = CreateEmptyTalentGrid();

            public TalentTreePage(UIElement aParent, TalentWindow aWindow, RelativeScreenPosition aPos, RelativeScreenPosition aSize)
                : base(aParent, null, aPos, aSize)
            {
                background = new Image(this, new UITexture(GfxPath.NullPath, new Color(255, 255, 255, 140)), new RelativeScreenPosition(0.04f, 0.03f), new RelativeScreenPosition(0.92f, 0.88f));
                backgroundShade = new Box(this, new UITexture("WhiteBackground", new Color(255, 255, 255, 185)), new RelativeScreenPosition(0.04f, 0.03f), new RelativeScreenPosition(0.92f, 0.88f));
                spentPointsLabel = new Label(this, new RelativeScreenPosition(0.55f, 0.01f), new RelativeScreenPosition(0.35f, 0.06f), Label.TextAllignment.CentreRight, Color.Black, aText: "Spent: 0");
                backgroundShade.CapturesClick = false;
                backgroundShade.CapturesRelease = false;
                backgroundShade.CapturesScroll = false;
                CapturesClick = false;
                CapturesRelease = false;
                CapturesScroll = false;
                slots = new TalentSlotElement[MaxColumns * MaxRows];
                arrowOverlay = new TalentArrowOverlay(this, this, new RelativeScreenPosition(0.05f, GridTop), new RelativeScreenPosition(0.9f, GridHeight));

                talentGrid = new PageBox(
                    this,
                    aPageBox => CreateTalentSlots(aPageBox, aWindow, slots),
                    new UITexture("WhiteBackground", Color.Transparent),
                    new RelativeScreenPosition(0.05f, GridTop),
                    new RelativeScreenPosition(0.9f, GridHeight),
                    new Point(MaxColumns, MaxRows),
                    BindTalentSlot,
                    ClearTalentSlot,
                    CreateTalentLabels);
                talentGrid.SetPageTitleProvider(_ => string.Empty);
                talentGrid.CapturesClick = false;
                talentGrid.CapturesRelease = false;
                talentGrid.CapturesScroll = false;

                ResetTree();
            }

            public void SetTree(TalentTreeUiSnapshot aSnapshot)
            {
                background.SetImage(aSnapshot.Background);
                spentPointsLabel.Text = $"Spent: {aSnapshot.SpentPoints}";
                MailboxManager.PublishUiEvent(new DescriptorBoxClear());
                talents = BuildTalentGrid(aSnapshot.Rows);
                talentGrid.Reset(slots.Length);
            }

            public void ResetTree()
            {
                SetTree(new TalentTreeUiSnapshot("Tree", GfxPath.NullPath, Array.Empty<TalentUiSnapshot[]>(), 0));
            }

            void BindTalentSlot(UIElement aElement, int aIndex)
            {
                TalentSlotElement slot = aElement as TalentSlotElement;
                if (slot == null) return;

                if (aIndex < 0 || aIndex >= talents.Length || !talents[aIndex].HasValue)
                {
                    slot.ClearTalent();
                    return;
                }

                slot.SetTalent(talents[aIndex].Value);
            }

            static void ClearTalentSlot(UIElement aElement)
            {
                TalentSlotElement slot = aElement as TalentSlotElement;
                slot?.ClearTalent();
            }

            static UIElement[] CreateTalentSlots(PageBox aPageBox, TalentWindow aWindow, TalentSlotElement[] slots)
            {
                RelativeScreenPosition slotSize = new RelativeScreenPosition(0.18f, 0.078f);
                RelativeScreenPosition labelSize = new RelativeScreenPosition(0.22f, 0.042f);
                RelativeScreenPosition start = new RelativeScreenPosition(0.02f, 0.02f);
                RelativeScreenPosition step = new RelativeScreenPosition(0.24f, 0.115f);

                for (int row = 0; row < MaxRows; row++)
                {
                    for (int column = 0; column < MaxColumns; column++)
                    {
                        int index = row * MaxColumns + column;
                        RelativeScreenPosition pos = new RelativeScreenPosition(
                            start.X + column * step.X + (labelSize.X - slotSize.X) / 2f,
                            start.Y + row * step.Y);
                        slots[index] = new TalentSlotElement(aPageBox, aWindow, pos, slotSize);
                    }
                }

                UIElement[] elements = new UIElement[slots.Length];
                for (int i = 0; i < slots.Length; i++)
                {
                    elements[i] = slots[i];
                }

                return elements;
            }

            static Label[] CreateTalentLabels(PageBox aPageBox)
            {
                Label[] labels = new Label[MaxColumns * MaxRows];
                RelativeScreenPosition labelSize = new RelativeScreenPosition(0.22f, 0.042f);
                RelativeScreenPosition start = new RelativeScreenPosition(0.02f, 0.101f);
                RelativeScreenPosition step = new RelativeScreenPosition(0.24f, 0.115f);

                for (int row = 0; row < MaxRows; row++)
                {
                    for (int column = 0; column < MaxColumns; column++)
                    {
                        int index = row * MaxColumns + column;
                        RelativeScreenPosition pos = new RelativeScreenPosition(start.X + column * step.X, start.Y + row * step.Y);
                        labels[index] = new Label(aPageBox, pos, labelSize, Label.TextAllignment.TopCentre, Color.Black, aTextSize: 7f);
                        labels[index].Visible = false;
                        labels[index].CapturesClick = false;
                        labels[index].CapturesRelease = false;
                        labels[index].CapturesScroll = false;
                    }
                }

                return labels;
            }

            static TalentUiSnapshot?[] BuildTalentGrid(TalentUiSnapshot[][] aRows)
            {
                TalentUiSnapshot?[] grid = CreateEmptyTalentGrid();
                if (aRows == null || aRows.Length == 0)
                {
                    return grid;
                }

                for (int row = 0; row < aRows.Length && row < MaxRows; row++)
                {
                    TalentUiSnapshot[] rowTalents = aRows[row] ?? Array.Empty<TalentUiSnapshot>();
                    for (int column = 0; column < rowTalents.Length && column < MaxColumns; column++)
                    {
                        grid[row * MaxColumns + column] = rowTalents[column];
                    }
                }

                return grid;
            }

            static TalentUiSnapshot?[] CreateEmptyTalentGrid()
            {
                return new TalentUiSnapshot?[MaxColumns * MaxRows];
            }
        }

        const int ExpectedTreesPerClass = 3;

        public static TalentWindow Current { get; private set; }

        public bool ShowingGuildMember => !showingSelf && targetRenderId.HasValue;

        readonly Label nameLabel;
        readonly Label talentPointsLabel;
        readonly PageBox treePages;

        TalentTreeUiSnapshot[] trees;
        bool showingSelf;
        int? targetRenderId;
        int playerLevel = 1;
        int remainingTalentPoints;

        public TalentWindow() : base(new UITexture("WhiteBackground", Color.DarkSeaGreen))
        {
            showingSelf = true;
            trees = BuildDefaultTrees();
            visibleKey = KeyBindManager.KeyListner.TalentWindow;
            Func<PageBox, UIElement[]> func; 
            func = (aPageBox) =>
            {
                return new UIElement[] { new TalentTreePage(aPageBox, this, new RelativeScreenPosition(0f, 0.12f), new RelativeScreenPosition(1f, 0.88f)) };
            };
            nameLabel = new Label(this, new RelativeScreenPosition(0f, 0f), new RelativeScreenPosition(1f, 0.08f), Label.TextAllignment.TopCentre, Color.Black, aText: "Talents");
            treePages = new PageBox(this, func, new UITexture("WhiteBackground", new Color(255, 255, 255, 30)), new RelativeScreenPosition(0.02f, 0.07f), new RelativeScreenPosition(0.96f, 0.9f), new Point(1, 1), BindTreePage, ClearTreePage);
            talentPointsLabel = new Label(this, new RelativeScreenPosition(0.05f, 0.08f), new RelativeScreenPosition(0.4f, 0.05f), Label.TextAllignment.CentreLeft, Color.Black, aText: "Talent Points: 0");

            treePages.SetPageTitleProvider(GetPageTitle);

            SetTrees(trees);
            Current = this;
        }

        public override void ToggleVisibilty()
        {
            ThreadAffinity.AssertUiThread();

            bool shouldRequestSnapshot = ToggleSelfFromKeybind();
            if (shouldRequestSnapshot)
            {
                MailboxManager.PublishSimCommand(new TalentWindowSnapshotRequested(null));
            }

            HUDManager.InvalidateUi();
        }

        public bool ToggleForMember(in EntityUiSnapshot member)
        {
            ThreadAffinity.AssertUiThread();
            if (member.Level < TalentUnlockLevel) return false;

            if (member.RelationToPlayer == RelationToPlayerKind.Self)
            {
                return ToggleSelf();
            }

            if (Visible && !showingSelf && targetRenderId == member.RenderId)
            {
                CloseWindow();
                return false;
            }

            showingSelf = false;
            targetRenderId = member.RenderId;
            nameLabel.Text = $"{member.Name} Talents";
            if (!Visible)
            {
                OpenWindow();
            }

            return true;
        }

        public bool Matches(in EntityUiSnapshot ownerSnapshot)
        {
            if (!Visible) return false;

            if (ownerSnapshot.RelationToPlayer == RelationToPlayerKind.Self)
            {
                return showingSelf;
            }

            return !showingSelf && targetRenderId == ownerSnapshot.RenderId;
        }

        public void SetData(TalentWindowSnapshot snapshot)
        {
            if (snapshot.OwnerSnapshot.Level < TalentUnlockLevel)
            {
                MailboxManager.PublishUiEvent(new DescriptorBoxClear());
                CloseWindow();
                return;
            }

            MailboxManager.PublishUiEvent(new DescriptorBoxClear());
            nameLabel.Text = $"{snapshot.OwnerSnapshot.Name} Talents";
            remainingTalentPoints = snapshot.RemainingTalentPoints;
            talentPointsLabel.Text = $"Talent Points: {remainingTalentPoints}";
            SetTrees(snapshot.Trees);
        }

        public void SetPlayerLevel(int aLevel)
        {
            playerLevel = aLevel;
            if (playerLevel >= TalentUnlockLevel || !Visible) return;

            MailboxManager.PublishUiEvent(new DescriptorBoxClear());
            CloseWindow();
        }

        bool ToggleSelf()
        {
            if (playerLevel < TalentUnlockLevel)
            {
                MailboxManager.PublishUiEvent(new DescriptorBoxClear());
                return false;
            }

            if (Visible && showingSelf)
            {
                MailboxManager.PublishUiEvent(new DescriptorBoxClear());
                CloseWindow();
                return false;
            }

            showingSelf = true;
            targetRenderId = null;
            nameLabel.Text = "Talents";
            if (!Visible)
            {
                OpenWindow();
            }

            return true;
        }

        internal void TrySpendTalent(int talentId)
        {
            if (!showingSelf) return;
            if (remainingTalentPoints <= 0) return;
            MailboxManager.PublishSimCommand(new TalentLearnRequested(talentId));
        }

        bool ToggleSelfFromKeybind()
        {
            if (playerLevel < TalentUnlockLevel)
            {
                MailboxManager.PublishUiEvent(new DescriptorBoxClear());
                return false;
            }

            if (Visible && showingSelf)
            {
                MailboxManager.PublishUiEvent(new DescriptorBoxClear());
                CloseWindow();
                return false;
            }

            showingSelf = true;
            targetRenderId = null;
            nameLabel.Text = "Talents";
            MailboxManager.PublishUiEvent(new DescriptorBoxClear());
            if (!Visible)
            {
                OpenWindow();
            }

            return true;
        }

        void BindTreePage(UIElement aElement, int aIndex)
        {
            TalentTreePage page = aElement as TalentTreePage;
            if (page == null) return;
            page.SetTree(GetTree(aIndex));
        }

        void ClearTreePage(UIElement aElement)
        {
            TalentTreePage page = aElement as TalentTreePage;
            if (page == null) return;
            page.ResetTree();
        }

        string GetPageTitle(int aPageIndex)
        {
            TalentTreeUiSnapshot tree = GetTree(aPageIndex);
            return string.IsNullOrWhiteSpace(tree.Name) ? $"Tree {aPageIndex + 1}" : tree.Name;
        }

        TalentTreeUiSnapshot GetTree(int index)
        {
            if (index < 0 || index >= trees.Length)
            {
                return BuildDefaultTree(index);
            }

            return trees[index];
        }

        void SetTrees(TalentTreeUiSnapshot[] aTrees)
        {
            int currentPage = treePages.CurrentPage;
            trees = BuildDefaultTrees();
            if (aTrees != null)
            {
                for (int i = 0; i < trees.Length && i < aTrees.Length; i++)
                {
                    trees[i] = aTrees[i];
                }
            }

            treePages.Reset(trees.Length);
            treePages.SetPage(Math.Min(currentPage, trees.Length - 1));
        }

        static TalentTreeUiSnapshot[] BuildDefaultTrees()
        {
            TalentTreeUiSnapshot[] defaultTrees = new TalentTreeUiSnapshot[ExpectedTreesPerClass];
            for (int i = 0; i < defaultTrees.Length; i++)
            {
                defaultTrees[i] = BuildDefaultTree(i);
            }

            return defaultTrees;
        }

        static TalentTreeUiSnapshot BuildDefaultTree(int index)
        {
            return new TalentTreeUiSnapshot($"Tree {index + 1}", GfxPath.NullPath, Array.Empty<TalentUiSnapshot[]>(), 0);
        }
    }
}
