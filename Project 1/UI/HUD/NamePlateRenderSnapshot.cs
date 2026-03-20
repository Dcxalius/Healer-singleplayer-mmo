using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Textures;
using System;

namespace Project_1.UI.HUD
{
    internal readonly struct NamePlateRenderSnapshot
    {
        const float TextSize = 12f;
        static readonly GfxPath platePath = new GfxPath(GfxType.UI, "GrayBackground");
        static readonly GfxPath barBackgroundPath = new GfxPath(GfxType.UI, "WhiteBackground");
        static readonly GfxPath barFillPath = new GfxPath(GfxType.UI, "WhiteGrayBasedBar");
        static readonly Color healthBarBackgroundColor = new Color(120, 50, 50, 80);
        static readonly Point defaultBarSourceSize = new Point(64, 8);

        public NamePlateRenderSnapshot(
            Rectangle plateRect,
            Color plateColor,
            Rectangle healthBarRect,
            float healthRatio,
            string nameText,
            Color nameColor,
            Rectangle nameClipRect,
            AbsoluteScreenPosition nameTopCenter)
        {
            PlateRect = plateRect;
            PlateColor = plateColor;
            HealthBarRect = healthBarRect;
            HealthRatio = healthRatio;
            NameText = nameText ?? string.Empty;
            NameColor = nameColor;
            NameClipRect = nameClipRect;
            NameTopCenter = nameTopCenter;
        }

        Rectangle PlateRect { get; }
        Color PlateColor { get; }
        Rectangle HealthBarRect { get; }
        float HealthRatio { get; }
        string NameText { get; }
        Color NameColor { get; }
        Rectangle NameClipRect { get; }
        AbsoluteScreenPosition NameTopCenter { get; }

        public void Draw(SpriteBatch batch)
        {
            ThreadAffinity.AssertMainThread();

            Texture2D plateTexture = TextureManager.GetTexture(platePath);
            batch.Draw(plateTexture, PlateRect, null, PlateColor, 0f, Vector2.Zero, SpriteEffects.None, 1f);

            Texture2D healthBg = TextureManager.GetTexture(barBackgroundPath);
            batch.Draw(healthBg, HealthBarRect, null, healthBarBackgroundColor, 0f, Vector2.Zero, SpriteEffects.None, 1f);

            float ratio = Math.Clamp(HealthRatio, 0f, 1f);
            if (ratio > 0f)
            {
                int fillWidth = (int)Math.Ceiling(HealthBarRect.Width * ratio);
                if (fillWidth > 0)
                {
                    Rectangle fillDest = new Rectangle(HealthBarRect.X, HealthBarRect.Y, fillWidth, HealthBarRect.Height);
                    int sourceWidth = (int)Math.Ceiling(defaultBarSourceSize.X * ratio);
                    sourceWidth = Math.Clamp(sourceWidth, 1, defaultBarSourceSize.X);
                    Rectangle sourceRect = new Rectangle(0, 0, sourceWidth, defaultBarSourceSize.Y);
                    Texture2D fillTexture = TextureManager.GetTexture(barFillPath);
                    batch.Draw(fillTexture, fillDest, sourceRect, Color.Red, 0f, Vector2.Zero, SpriteEffects.None, 1f);
                }
            }

            if (string.IsNullOrWhiteSpace(NameText)) return;
            Text nameText = new Text("Comfortaa-msdf", NameText, NameColor, TextSize);
            object scissorToken = new object();
            GraphicsManager.CaptureScissor(scissorToken, NameClipRect);
            nameText.TopCentreDraw(batch, NameTopCenter);
            GraphicsManager.ReleaseScissor(scissorToken);
        }
    }
}
