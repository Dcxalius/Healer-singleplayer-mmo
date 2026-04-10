using System;
using Microsoft.Xna.Framework;
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
        sealed class TalentTreePage : UIElement
        {
            sealed class TalentSlotElement : UIElement
            {
                readonly TalentTreePage page;
                readonly Box frame;
                readonly Image icon;
                readonly Label nameLabel;
                readonly Label rankLabel;

                TalentUiSnapshot snapshot;
                bool hasTalent;

                public TalentSlotElement(TalentTreePage aParent, RelativeScreenPosition aPos, RelativeScreenPosition aSize)
                    : base(aParent, null, aPos, aSize)
                {
                    page = aParent;
                    frame = new Box(this, new UITexture("GrayWhiteBorder", Color.White), RelativeScreenPosition.Zero, RelativeScreenPosition.One);
                    icon = new Image(this, new UITexture(GfxPath.NullPath, Color.White), new RelativeScreenPosition(0.08f, 0.08f), new RelativeScreenPosition(0.84f, 0.54f));
                    rankLabel = new Label(this, new RelativeScreenPosition(0.05f, 0.6f), new RelativeScreenPosition(0.9f, 0.12f), Label.TextAllignment.CentreRight, Color.White, aTextSize: 9f);
                    nameLabel = new Label(this, new RelativeScreenPosition(0.05f, 0.7f), new RelativeScreenPosition(0.9f, 0.22f), Label.TextAllignment.TopCentre, Color.Black, aTextSize: 8f);

                    Visible = false;
                    CapturesClick = false;
                    CapturesRelease = false;
                    CapturesScroll = false;
                }

                public void SetTalent(TalentUiSnapshot aSnapshot)
                {
                    snapshot = aSnapshot;
                    hasTalent = true;
                    icon.SetImage(aSnapshot.GfxPath);
                    icon.Color = aSnapshot.Rank > 0 ? Color.White : Color.DarkGray;
                    nameLabel.Text = aSnapshot.Name;
                    rankLabel.Text = $"{aSnapshot.Rank}/{aSnapshot.MaxRank}";
                    Visible = true;
                }

                public void ClearTalent()
                {
                    hasTalent = false;
                    icon.ClearImage();
                    icon.Color = Color.White;
                    nameLabel.Text = null;
                    rankLabel.Text = null;
                    Visible = false;
                }

                protected override void OnHover()
                {
                    base.OnHover();
                    if (!hasTalent) return;
                    page.ShowDescription(snapshot);
                }

                protected override void OnDeHover()
                {
                    base.OnDeHover();
                    if (!hasTalent) return;
                    page.ResetDescription();
                }
            }

            const int MaxColumns = 4;
            const int MaxRows = 7;

            readonly Image background;
            readonly Box backgroundShade;
            readonly Box descriptionBox;
            readonly Label descriptionLabel;
            readonly TalentSlotElement[] slots;

            string defaultDescription;

            public TalentTreePage(UIElement aParent, RelativeScreenPosition aPos, RelativeScreenPosition aSize)
                : base(aParent, null, aPos, aSize)
            {
                background = new Image(this, new UITexture(GfxPath.NullPath, new Color(255, 255, 255, 140)), new RelativeScreenPosition(0.04f, 0.03f), new RelativeScreenPosition(0.92f, 0.62f));
                backgroundShade = new Box(this, new UITexture("WhiteBackground", new Color(255, 255, 255, 185)), new RelativeScreenPosition(0.04f, 0.03f), new RelativeScreenPosition(0.92f, 0.62f));
                descriptionBox = new Box(this, new UITexture("WhiteBackground", new Color(245, 245, 220, 235)), new RelativeScreenPosition(0.04f, 0.69f), new RelativeScreenPosition(0.92f, 0.26f));
                descriptionLabel = new Label(this, new RelativeScreenPosition(0.07f, 0.72f), new RelativeScreenPosition(0.86f, 0.2f), Label.TextAllignment.TopLeft, Color.Black, aTextSize: 10f);

                slots = new TalentSlotElement[MaxColumns * MaxRows];
                RelativeScreenPosition slotSize = new RelativeScreenPosition(0.18f, 0.07f);
                RelativeScreenPosition slotSpacing = new RelativeScreenPosition(0.03f, 0.012f);
                RelativeScreenPosition start = new RelativeScreenPosition(0.09f, 0.07f);

                for (int row = 0; row < MaxRows; row++)
                {
                    for (int column = 0; column < MaxColumns; column++)
                    {
                        int index = row * MaxColumns + column;
                        RelativeScreenPosition pos = new RelativeScreenPosition(
                            start.X + column * (slotSize.X + slotSpacing.X),
                            start.Y + row * (slotSize.Y + slotSpacing.Y));

                        slots[index] = new TalentSlotElement(this, pos, slotSize);
                    }
                }

                ResetTree();
            }

            public void SetTree(TalentTreeUiSnapshot aSnapshot)
            {
                background.SetImage(aSnapshot.Background);

                for (int i = 0; i < slots.Length; i++)
                {
                    slots[i].ClearTalent();
                }

                TalentUiSnapshot[][] rows = aSnapshot.Rows ?? Array.Empty<TalentUiSnapshot[]>();
                for (int row = 0; row < rows.Length && row < MaxRows; row++)
                {
                    TalentUiSnapshot[] talents = rows[row] ?? Array.Empty<TalentUiSnapshot>();
                    for (int column = 0; column < talents.Length && column < MaxColumns; column++)
                    {
                        slots[row * MaxColumns + column].SetTalent(talents[column]);
                    }
                }

                defaultDescription = rows.Length == 0
                    ? "No talent tree data configured for this page yet."
                    : "Hover a talent to inspect its effect.";
                ResetDescription();
            }

            public void ResetTree()
            {
                SetTree(new TalentTreeUiSnapshot("Tree", GfxPath.NullPath, Array.Empty<TalentUiSnapshot[]>()));
            }

            public void ShowDescription(TalentUiSnapshot aSnapshot)
            {
                descriptionLabel.Text = $"{aSnapshot.Name} ({aSnapshot.Rank}/{aSnapshot.MaxRank})\n{aSnapshot.Description}";
            }

            public void ResetDescription()
            {
                descriptionLabel.Text = defaultDescription;
            }
        }

        const int ExpectedTreesPerClass = 3;

        public static TalentWindow Current { get; private set; }

        public bool ShowingGuildMember => !showingSelf && targetRenderId.HasValue;

        readonly Label nameLabel;
        readonly PageBox treePages;
        readonly TalentTreePage treePage;

        TalentTreeUiSnapshot[] trees;
        bool showingSelf;
        int? targetRenderId;

        public TalentWindow() : base(new UITexture("WhiteBackground", Color.DarkSeaGreen))
        {
            showingSelf = true;
            trees = BuildDefaultTrees();
            visibleKey = KeyBindManager.KeyListner.TalentWindow;
            Func<PageBox, UIElement[]> func; //TODO: This is really gross, but it needs to be here before the itemsForSale array is initialized, and the itemsForSale array needs to be initialized before the pageBox is initialized. Refactor this when possible.
            func = (aPageBox) =>
            {
                return new UIElement[] { new TalentTreePage(aPageBox, new RelativeScreenPosition(0f, 0.12f), new RelativeScreenPosition(1f, 0.88f)) };
            };
            nameLabel = new Label(this, new RelativeScreenPosition(0f, 0f), new RelativeScreenPosition(1f, 0.08f), Label.TextAllignment.TopCentre, Color.Black, aText: "Talents");
            treePages = new PageBox(this, func, new UITexture("WhiteBackground", new Color(255, 255, 255, 30)), new RelativeScreenPosition(0.02f, 0.07f), new RelativeScreenPosition(0.96f, 0.9f), new Point(1, 1), BindTreePage);
            

            treePages.SetPageTitleProvider(GetPageTitle);

            SetTrees(trees);
            Current = this;
        }

        public override void ToggleVisibilty()
        {
            ThreadAffinity.AssertUiThread();

            bool shouldRequestSnapshot = ToggleSelf();
            if (shouldRequestSnapshot)
            {
                MailboxManager.PublishSimCommand(new TalentWindowSnapshotRequested(null));
            }

            HUDManager.InvalidateUi();
        }

        public bool ToggleForMember(in EntityUiSnapshot member)
        {
            ThreadAffinity.AssertUiThread();

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
            nameLabel.Text = $"{snapshot.OwnerSnapshot.Name} Talents";
            SetTrees(snapshot.Trees);
        }

        bool ToggleSelf()
        {
            if (Visible && showingSelf)
            {
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

        void BindTreePage(UIElement aElement, int aIndex)
        {
            TalentTreePage page = aElement as TalentTreePage;
            if (page == null) return;
            page.SetTree(GetTree(aIndex));
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
            return new TalentTreeUiSnapshot($"Tree {index + 1}", GfxPath.NullPath, Array.Empty<TalentUiSnapshot[]>());
        }
    }
}
