using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Textures;
using System;

namespace Project_1.UI.UIElements
{
    internal readonly struct BuffRenderSnapshot
    {
        public BuffRenderSnapshot(GfxPath gfxPath, Rectangle absolutePos, double remainingMsAtBuild)
        {
            GfxPath = gfxPath;
            AbsolutePos = absolutePos;
            RemainingMsAtBuild = remainingMsAtBuild;
        }

        public GfxPath GfxPath { get; }
        public Rectangle AbsolutePos { get; }
        public double RemainingMsAtBuild { get; }

        public void Draw(SpriteBatch batch, SpriteFont font, double elapsedMs)
        {
            ThreadAffinity.AssertMainThread();
            if (GfxPath == null || GfxPath.Name == null) return;

            double remainingMs = RemainingMsAtBuild - elapsedMs;
            if (remainingMs <= 0d) return;
            if (!Camera.Camera.ScreenspaceBoundsCheck(AbsolutePos)) return;

            Texture2D icon = TextureManager.GetTexture(GfxPath);
            batch.Draw(icon, AbsolutePos, Color.White);

            string durationText = Math.Round(remainingMs / 1000d, 1).ToString();
            if (durationText.Length == 0) return;

            Vector2 textSize = font.MeasureString(durationText);
            Vector2 textPos = new Vector2(AbsolutePos.Center.X, AbsolutePos.Center.Y + AbsolutePos.Height - 3);
            batch.DrawString(font, durationText, textPos, Color.Black, 0f, textSize / 2f, Camera.Camera.Zoom, SpriteEffects.None, 1f);
        }
    }

    internal readonly struct BuffBoxRenderSnapshot
    {
        public BuffBoxRenderSnapshot(bool visible, int entryStartIndex, int entryCount, double buildFrameTimeMs)
        {
            Visible = visible;
            EntryStartIndex = Math.Max(0, entryStartIndex);
            EntryCount = Math.Max(0, entryCount);
            BuildFrameTimeMs = buildFrameTimeMs;
        }

        bool Visible { get; }
        int EntryStartIndex { get; }
        int EntryCount { get; }
        double BuildFrameTimeMs { get; }

        public void Draw(SpriteBatch batch, BuffRenderSnapshot[] entries)
        {
            ThreadAffinity.AssertMainThread();
            if (!Visible || EntryCount <= 0 || entries == null) return;

            double elapsedMs = Math.Max(0d, TimeManager.TotalFrameTime - BuildFrameTimeMs);
            SpriteFont font = FontCache.GetFont("Gloryse");
            int end = Math.Min(entries.Length, EntryStartIndex + EntryCount);
            for (int i = EntryStartIndex; i < end; i++)
            {
                entries[i].Draw(batch, font, elapsedMs);
            }
        }
    }
}
