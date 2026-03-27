using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Buttons;
using System;

namespace Project_1.UI.HUD.Windows
{
    internal class SpellTrainingEntryButton : Button
    {
        readonly Image iconImage;
        readonly Label nameLabel;
        readonly Label levelLabel;
        SpellTrainingEntrySnapshot snapshot;
        bool selected;

        public SpellTrainingEntrySnapshot Snapshot => snapshot;

        public SpellTrainingEntryButton(Action<SpellTrainingEntrySnapshot> onSelected) : base(RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Color.SlateGray)
        {
            Color = Color.SlateGray;
            RelativeScreenPosition iconSize = new RelativeScreenPosition(0.08f, 0.80f);
            iconImage = new Image(UITexture.Null, new RelativeScreenPosition(0.02f, 0.10f), iconSize);
            nameLabel = new Label("", new RelativeScreenPosition(0.12f, 0f), new RelativeScreenPosition(0.63f, 1f), Label.TextAllignment.CentreLeft, Color.White, aTextSize: 11f);
            levelLabel = new Label("", new RelativeScreenPosition(0.76f, 0f), new RelativeScreenPosition(0.20f, 1f), Label.TextAllignment.CentreRight, Color.White, aTextSize: 11f);
            AddChild(iconImage);
            AddChild(nameLabel);
            AddChild(levelLabel);
            if (onSelected != null)
            {
                AddAction(() => onSelected(snapshot));
            }
        }

        public void Set(SpellTrainingEntrySnapshot snapshot, bool selected)
        {
            this.snapshot = snapshot;
            this.selected = selected;
            iconImage.SetImage(snapshot.GfxPath);
            nameLabel.Text = snapshot.DisplayName;
            levelLabel.Text = $"Lv {snapshot.RequiredLevel}";
            ApplyColors();
        }

        public void Clear()
        {
            snapshot = default;
            selected = false;
            iconImage.ClearImage();
            nameLabel.Text = string.Empty;
            levelLabel.Text = string.Empty;
            Visible = false;
            CapturesClick = false;
        }

        protected override void OnHover()
        {
            base.OnHover();
            if (!Visible || !snapshot.HasDescriptor) return;
            MailboxManager.PublishUiEvent(new SpellDescriptorBoxSet(snapshot.Descriptor, RelativePositionOnScreen.ToAbsoluteScreenPos()));
        }

        protected override void OnDeHover()
        {
            base.OnDeHover();
            MailboxManager.PublishUiEvent(new DescriptorBoxClear());
        }

        void ApplyColors()
        {
            Visible = true;
            CapturesClick = true;

            if (selected)
            {
                Color = Color.DarkGoldenrod;
            }
            else if (snapshot.Learned)
            {
                Color = Color.DarkOliveGreen;
            }
            else if (snapshot.Learnable)
            {
                Color = Color.DarkSlateBlue;
            }
            else
            {
                Color = Color.DimGray;
            }

            Color textColor = snapshot.Learned ? Color.Honeydew : snapshot.Learnable ? Color.White : Color.Gainsboro;
            nameLabel.Color = textColor;
            levelLabel.Color = textColor;
        }

        public override string ToString()
        {
            return $"{snapshot.DisplayName}";
        }
    }
}
