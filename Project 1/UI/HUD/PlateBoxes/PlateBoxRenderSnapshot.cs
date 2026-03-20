using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Textures;
using System;

namespace Project_1.UI.HUD.PlateBoxes
{
    internal readonly struct PlateBoxRenderSnapshot
    {
        const float TextSize = 12f;
        static readonly GfxPath whiteBackgroundPath = new GfxPath(GfxType.UI, "WhiteBackground");
        static readonly GfxPath barFillPath = new GfxPath(GfxType.UI, "WhiteGrayBasedBar");
        static readonly GfxPath levelCirclePath = new GfxPath(GfxType.UI, "LevelCircle");
        static readonly Color barBackgroundColor = new Color(255, 211, 211, 120);

        public PlateBoxRenderSnapshot(
            bool visible,
            Rectangle boxRect,
            Rectangle nameRect,
            Rectangle healthRect,
            Rectangle resourceRect,
            Rectangle levelRect,
            string name,
            Color relationColor,
            float currentHealth,
            float maxHealth,
            float currentResource,
            float maxResource,
            Color resourceColor,
            int level,
            bool showCommandBorder)
        {
            Visible = visible;
            BoxRect = boxRect;
            NameRect = nameRect;
            HealthRect = healthRect;
            ResourceRect = resourceRect;
            LevelRect = levelRect;
            Name = name ?? string.Empty;
            RelationColor = relationColor;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            CurrentResource = currentResource;
            MaxResource = maxResource;
            ResourceColor = resourceColor;
            Level = level;
            ShowCommandBorder = showCommandBorder;
        }

        bool Visible { get; }
        Rectangle BoxRect { get; }
        Rectangle NameRect { get; }
        Rectangle HealthRect { get; }
        Rectangle ResourceRect { get; }
        Rectangle LevelRect { get; }
        string Name { get; }
        Color RelationColor { get; }
        float CurrentHealth { get; }
        float MaxHealth { get; }
        float CurrentResource { get; }
        float MaxResource { get; }
        Color ResourceColor { get; }
        int Level { get; }
        bool ShowCommandBorder { get; }

        public void Draw(SpriteBatch batch)
        {
            ThreadAffinity.AssertMainThread();
            if (!Visible) return;
            if (BoxRect.Width <= 0 || BoxRect.Height <= 0) return;

            Texture2D whiteTexture = TextureManager.GetTexture(whiteBackgroundPath);
            Texture2D fillTexture = TextureManager.GetTexture(barFillPath);

            batch.Draw(whiteTexture, NameRect, null, RelationColor, 0f, Vector2.Zero, SpriteEffects.None, 1f);
            DrawCenteredText(batch, Name, Color.Black, NameRect);

            DrawResourceLike(batch, whiteTexture, fillTexture, HealthRect, CurrentHealth, MaxHealth, Color.Red, true);
            DrawResourceLike(batch, whiteTexture, fillTexture, ResourceRect, CurrentResource, MaxResource, ResourceColor, true);

            DrawLevel(batch);
            if (ShowCommandBorder)
            {
                DrawBorder(batch, whiteTexture, Color.YellowGreen, 2);
            }
        }

        void DrawResourceLike(SpriteBatch batch, Texture2D bgTexture, Texture2D fillTexture, Rectangle rect, float current, float max, Color fillColor, bool drawText)
        {
            batch.Draw(bgTexture, rect, null, barBackgroundColor, 0f, Vector2.Zero, SpriteEffects.None, 1f);

            float ratio;
            if (max <= 0f)
            {
                ratio = 1f;
            }
            else
            {
                float clampedCurrent = Math.Max(0f, current);
                ratio = Math.Clamp(clampedCurrent / max, 0f, 1f);
            }

            int fillWidth = (int)Math.Ceiling(rect.Width * ratio);
            if (fillWidth > 0)
            {
                int srcWidth = Math.Clamp((int)Math.Ceiling(64 * ratio), 1, 64);
                Rectangle dest = new Rectangle(rect.X, rect.Y, fillWidth, rect.Height);
                Rectangle src = new Rectangle(0, 0, srcWidth, 8);
                batch.Draw(fillTexture, dest, src, fillColor, 0f, Vector2.Zero, SpriteEffects.None, 1f);
            }

            if (!drawText || max <= 0f) return;
            string fraction = $"{Math.Round(current)}/{max:0.##}";
            string percent = $"{(int)Math.Clamp((current / max) * 100f, 0f, 100f)}%";
            DrawTextLeft(batch, fraction, Color.Black, rect, 5);
            DrawTextRight(batch, percent, Color.Black, rect, 5);
        }

        void DrawLevel(SpriteBatch batch)
        {
            Texture2D levelTexture = TextureManager.GetTexture(levelCirclePath);
            batch.Draw(levelTexture, LevelRect, null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1f);
            DrawCenteredText(batch, Level.ToString(), Color.Pink, LevelRect);
        }

        void DrawBorder(SpriteBatch batch, Texture2D texture, Color color, int thickness)
        {
            int t = Math.Max(1, thickness);
            Rectangle top = new Rectangle(BoxRect.X, BoxRect.Y, BoxRect.Width, t);
            Rectangle bottom = new Rectangle(BoxRect.X, BoxRect.Bottom - t, BoxRect.Width, t);
            Rectangle left = new Rectangle(BoxRect.X, BoxRect.Y, t, BoxRect.Height);
            Rectangle right = new Rectangle(BoxRect.Right - t, BoxRect.Y, t, BoxRect.Height);
            batch.Draw(texture, top, null, color, 0f, Vector2.Zero, SpriteEffects.None, 1f);
            batch.Draw(texture, bottom, null, color, 0f, Vector2.Zero, SpriteEffects.None, 1f);
            batch.Draw(texture, left, null, color, 0f, Vector2.Zero, SpriteEffects.None, 1f);
            batch.Draw(texture, right, null, color, 0f, Vector2.Zero, SpriteEffects.None, 1f);
        }

        static void DrawCenteredText(SpriteBatch batch, string text, Color color, Rectangle rect)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            Text label = new Text("Comfortaa-msdf", text, color, TextSize);
            label.CentredDraw(batch, new AbsoluteScreenPosition(rect.Center));
        }

        static void DrawTextLeft(SpriteBatch batch, string text, Color color, Rectangle rect, int padding)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            Text label = new Text("Comfortaa-msdf", text, color, TextSize);
            label.CentreLeftDraw(batch, new AbsoluteScreenPosition(rect.Left + padding, rect.Center.Y));
        }

        static void DrawTextRight(SpriteBatch batch, string text, Color color, Rectangle rect, int padding)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            Text label = new Text("Comfortaa-msdf", text, color, TextSize);
            label.CentreRightDraw(batch, new AbsoluteScreenPosition(rect.Right - padding, rect.Center.Y));
        }
    }
}
