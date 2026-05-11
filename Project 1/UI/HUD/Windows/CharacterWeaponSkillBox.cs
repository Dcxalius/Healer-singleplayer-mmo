using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Bars;
using Project_1.UI.UIElements.Boxes;
using System;

namespace Project_1.UI.HUD.Windows
{
    internal sealed class CharacterWeaponSkillBox : Box
    {
        sealed class WeaponSkillLine : UIElement
        {
            readonly Label nameLabel;
            readonly ResourceBar progressBar;

            public WeaponSkillLine(UIElement parent, RelativeScreenPosition pos, RelativeScreenPosition size)
                : base(parent, null, pos, size)
            {
                nameLabel = new Label(this, new RelativeScreenPosition(0f, 0f), new RelativeScreenPosition(0.34f, 1f), Label.TextAllignment.CentreLeft, Color.Black, "Comfortaa-msdf", 11f);
                progressBar = new ResourceBar(
                    this,
                    new BarTexture(BarTexture.FillingDirection.Right, Color.ForestGreen),
                    new UITexture("WhiteGrayBasedBar", Color.DarkSlateGray),
                    new RelativeScreenPosition(0.36f, 0.18f),
                    new RelativeScreenPosition(0.64f, 0.64f));
                progressBar.SetLabelTextSize(ResourceBar.Labels.Fraction, 10f);
                progressBar.SetLabelTextSize(ResourceBar.Labels.Percentage, 10f);
                CapturesClick = false;
                CapturesRelease = false;
                CapturesScroll = false;
            }

            public void Set(WeaponSkillUiSnapshot snapshot)
            {
                Visible = true;
                nameLabel.Text = snapshot.DisplayName;
                progressBar.MaxValue = snapshot.MaxValue;
                progressBar.Value = snapshot.Value;
            }

            public void Clear()
            {
                nameLabel.Text = null;
                progressBar.MaxValue = 1;
                progressBar.Value = 0;
                Visible = false;
            }
        }

        const int MaxVisibleRows = 13;
        const float TitleHeight = 0.12f;
        readonly Label titleLabel;
        readonly WeaponSkillLine[] rows;

        public CharacterWeaponSkillBox(UIElement parent, RelativeScreenPosition pos, RelativeScreenPosition size)
            : base(parent, new UITexture("WhiteBackground", Color.Transparent), pos, size)
        {
            titleLabel = new Label(this, RelativeScreenPosition.Zero, new RelativeScreenPosition(1f, TitleHeight), Label.TextAllignment.TopCentre, Color.Black, "Comfortaa-msdf", 13f, "Weapon Skills");
            rows = new WeaponSkillLine[MaxVisibleRows];
            float rowHeight = (1f - TitleHeight) / MaxVisibleRows;
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i] = new WeaponSkillLine(this, new RelativeScreenPosition(0.05f, TitleHeight + rowHeight * i), new RelativeScreenPosition(0.9f, rowHeight));
                rows[i].Clear();
            }

            CapturesClick = false;
            CapturesRelease = false;
            CapturesScroll = false;
        }

        public void SetSkills(WeaponSkillUiSnapshot[] skills)
        {
            skills ??= Array.Empty<WeaponSkillUiSnapshot>();
            int count = Math.Min(skills.Length, rows.Length);
            for (int i = 0; i < count; i++)
            {
                rows[i].Set(skills[i]);
            }

            for (int i = count; i < rows.Length; i++)
            {
                rows[i].Clear();
            }
        }
    }
}
