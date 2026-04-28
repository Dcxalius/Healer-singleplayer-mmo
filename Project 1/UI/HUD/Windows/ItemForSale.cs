using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;

namespace Project_1.UI.HUD.Windows
{
    internal class ItemForSale : Box
    {
        readonly SquareImage displayItem;
        ItemUiSnapshot itemForSale;
        readonly Label itemName;
        readonly Label goldCost;
        readonly SquareImage goldImage;

        public static RelativeScreenPosition size;
        static RelativeScreenPosition spacing;
        static bool initialized;

        static void Init()
        {
            if (initialized) return;
            initialized = true;
            size = new RelativeScreenPosition(0.4f, 0.10f);
            spacing = RelativeScreenPosition.GetSquareFromX(0.06f, size.ToAbsoluteScreenPos(Window.WindowSize.ToAbsoluteScreenPos()));
        }

        static RelativeScreenPosition EnsureInitAndGetSize()
        {
            Init();
            return size;
        }

        public ItemForSale(UIElement aParent, RelativeScreenPosition aPos) : base(aParent, new UITexture("WhiteBackground", Color.Lavender), aPos, EnsureInitAndGetSize())
        {
            displayItem = new SquareImage(this, UITexture.Null, spacing, new RelativeScreenPosition(0, 1f - spacing.Y * 2));
            itemName = new Label(this, displayItem.RelativeSize.OnlyX + spacing.OnlyX * 2, new RelativeScreenPosition(1, 0.5f) - displayItem.RelativeSize.OnlyX - spacing.OnlyX * 2, Label.TextAllignment.CentreLeft, Color.Black);
            goldCost = new Label(this, displayItem.RelativeSize.OnlyX + displayItem.RelativeSize.OnlyY / 2 + spacing.OnlyX, new RelativeScreenPosition(0.9f, 0.5f) - displayItem.RelativeSize.OnlyX - spacing.OnlyX * 2, Label.TextAllignment.CentreRight, Color.Black);
            goldImage = new SquareImage(this, new UITexture("Gold", Color.White), new RelativeScreenPosition(1f - spacing.X, 0.75f), new RelativeScreenPosition(0.1f, 0));

            itemForSale = ItemUiSnapshot.Empty;
        }

        protected override void OnHover()
        {
            base.OnHover();
            if (!Visible || !itemForSale.HasValue || !itemForSale.HasDescriptor) return;
            MailboxManager.PublishUiEvent(new DescriptorBoxSet(itemForSale.Descriptor, RelativePositionOnScreen.ToAbsoluteScreenPos()));
        }

        protected override void OnDeHover()
        {
            base.OnDeHover();
            MailboxManager.PublishUiEvent(new DescriptorBoxClear());
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();
            if (!Visible || !itemForSale.HasValue) return;
            if (!UiPlayerStateCache.Valid) return;
            if (itemForSale.Cost > UiPlayerStateCache.Gold) return; //TODO: Print error msg
            MailboxManager.PublishSimCommand(new ShopPurchaseRequested(itemForSale.Id, itemForSale.Count));
        }

        public void Set(in ItemUiSnapshot snapshot)
        {
            itemForSale = snapshot;
            if (!snapshot.HasValue)
            {
                Clear();
                return;
            }

            displayItem.SetImage(snapshot.GfxPath);
            goldCost.Text = snapshot.Cost.ToString();
            itemName.Text = snapshot.Name;
            goldImage.Visible = true;
        }

        public void Clear()
        {
            displayItem.ClearImage();
            goldImage.Visible = false;
            itemName.Text = null;
            goldCost.Text = null;
            itemForSale = ItemUiSnapshot.Empty;
        }
    }
}
