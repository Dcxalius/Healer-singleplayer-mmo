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
        const float TextSize = 12f;

        public BuffRenderSnapshot(GfxPath gfxPath, Rectangle absolutePos, double remainingMsAtBuild)
        {
            GfxPath = gfxPath;
            AbsolutePos = absolutePos;
            RemainingMsAtBuild = remainingMsAtBuild;
        }

        public GfxPath GfxPath { get; }
        public Rectangle AbsolutePos { get; }
        public double RemainingMsAtBuild { get; }

        public void Draw(SpriteBatch batch, double elapsedMs)
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

            Text label = new Text("Comfortaa-msdf", durationText, Color.Black, TextSize);
            label.BottomCentreDraw(batch, new AbsoluteScreenPosition(AbsolutePos.Center.X, AbsolutePos.Center.Y + AbsolutePos.Height - 3));
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
            int end = Math.Min(entries.Length, EntryStartIndex + EntryCount);
            for (int i = EntryStartIndex; i < end; i++)
            {
                entries[i].Draw(batch, elapsedMs);
            }
        }
    }
}
