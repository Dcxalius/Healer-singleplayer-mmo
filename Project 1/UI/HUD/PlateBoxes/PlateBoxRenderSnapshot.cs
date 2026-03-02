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
        static readonly GfxPath whiteBackgroundPath = new GfxPath(GfxType.UI, "WhiteBackground");
        static readonly GfxPath barFillPath = new GfxPath(GfxType.UI, "WhiteGrayBasedBar");
        static readonly GfxPath levelCirclePath = new GfxPath(GfxType.UI, "LevelCircle");
        static readonly Color barBackgroundColor = new Color(255, 211, 211, 120);

        public PlateBoxRenderSnapshot(
            bool visible,
            Rectangle boxRect,
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
            SpriteFont font = FontCache.GetFont("Gloryse");

            Rectangle nameRect = SegmentRect(0f, 0.5f);
            Rectangle healthRect = SegmentRect(0.5f, 0.25f);
            Rectangle resourceRect = SegmentRect(0.75f, 0.25f);

            batch.Draw(whiteTexture, nameRect, null, RelationColor, 0f, Vector2.Zero, SpriteEffects.None, 1f);
            DrawCenteredText(batch, font, Name, Color.Black, nameRect);

            DrawResourceLike(batch, whiteTexture, fillTexture, healthRect, CurrentHealth, MaxHealth, Color.Red, true, font);
            DrawResourceLike(batch, whiteTexture, fillTexture, resourceRect, CurrentResource, MaxResource, ResourceColor, true, font);

            DrawLevel(batch, font);
            if (ShowCommandBorder)
            {
                DrawBorder(batch, whiteTexture, Color.YellowGreen, 2);
            }
        }

        Rectangle SegmentRect(float yStartRatio, float heightRatio)
        {
            int y = BoxRect.Y + (int)Math.Round(BoxRect.Height * yStartRatio);
            int h = Math.Max(1, (int)Math.Round(BoxRect.Height * heightRatio));
            int bottom = Math.Min(BoxRect.Bottom, y + h);
            if (bottom <= y) bottom = y + 1;
            return new Rectangle(BoxRect.X, y, BoxRect.Width, bottom - y);
        }

        void DrawResourceLike(SpriteBatch batch, Texture2D bgTexture, Texture2D fillTexture, Rectangle rect, float current, float max, Color fillColor, bool drawText, SpriteFont font)
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
            DrawTextLeft(batch, font, fraction, Color.Black, rect, 5);
            DrawTextRight(batch, font, percent, Color.Black, rect, 5);
        }

        void DrawLevel(SpriteBatch batch, SpriteFont font)
        {
            int circleSize = Math.Max(1, (int)Math.Round(BoxRect.Width * 0.05f));
            Rectangle circleRect = new Rectangle(BoxRect.Right - circleSize, BoxRect.Y, circleSize, circleSize);
            Texture2D levelTexture = TextureManager.GetTexture(levelCirclePath);
            batch.Draw(levelTexture, circleRect, null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1f);
            DrawCenteredText(batch, font, Level.ToString(), Color.Pink, circleRect);
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

        static void DrawCenteredText(SpriteBatch batch, SpriteFont font, string text, Color color, Rectangle rect)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            Vector2 size = font.MeasureString(text);
            Vector2 pos = new Vector2(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
            Vector2 origin = size / 2f;
            batch.DrawString(font, text, pos, color, 0f, origin, Camera.Camera.Zoom, SpriteEffects.None, 1f);
        }

        static void DrawTextLeft(SpriteBatch batch, SpriteFont font, string text, Color color, Rectangle rect, int padding)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            Vector2 size = font.MeasureString(text);
            Vector2 pos = new Vector2(rect.Left + padding, rect.Center.Y);
            Vector2 origin = new Vector2(0f, size.Y / 2f);
            batch.DrawString(font, text, pos, color, 0f, origin, Camera.Camera.Zoom, SpriteEffects.None, 1f);
        }

        static void DrawTextRight(SpriteBatch batch, SpriteFont font, string text, Color color, Rectangle rect, int padding)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            Vector2 size = font.MeasureString(text);
            Vector2 pos = new Vector2(rect.Right - padding, rect.Center.Y);
            Vector2 origin = new Vector2(size.X, size.Y / 2f);
            batch.DrawString(font, text, pos, color, 0f, origin, Camera.Camera.Zoom, SpriteEffects.None, 1f);
        }
    }
}
