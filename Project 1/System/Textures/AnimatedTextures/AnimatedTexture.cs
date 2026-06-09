using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Textures
{
    internal abstract class AnimatedTexture : Texture
    {
        protected Rectangle[] possibleFrames;
        protected int currentFrame = 0;
        protected double durationBetweenFrames;
        protected double lastFrameFlip;

        


        public AnimatedTexture(GfxPath path, Point aVisableSize, int aDeadFrameCount, TimeSpan aTimePerFrame) : base(path, aVisableSize)
        {
            durationBetweenFrames = (double)aTimePerFrame.TotalMilliseconds;

            CreateAnimationFrames(aDeadFrameCount);
            Visible = possibleFrames[0];
        }

        public override void Update()
        {
            CheckForFrameUpdate();
            EndOfFrameLoop();

            base.Update();
        }

        void CreateAnimationFrames(int aDeadFrameCount)
        {
            Point sheetSize = TextureCatalog.GetSize(gfxPath);
            if (size == Point.Zero || sheetSize == Point.Zero)
            {
                Point fallbackSize = size == Point.Zero ? new Point(1, 1) : size;
                possibleFrames = new[] { new Rectangle(Point.Zero, fallbackSize) };
                return;
            }

            int rectsInXDir = Math.Max(1, sheetSize.X / size.X);
            int rectsInYDir = Math.Max(1, sheetSize.Y / size.Y);
            int totalFrames = rectsInXDir * rectsInYDir - aDeadFrameCount;
            if (totalFrames <= 0) totalFrames = 1;
            possibleFrames = new Rectangle[totalFrames];


            for (int i = 0; i < possibleFrames.Length; i++)
            {
                Point topLeft = new Point((i % rectsInXDir) * size.X, (int)Math.Floor((decimal)i / rectsInXDir) * size.Y);
                possibleFrames[i] = new Rectangle(topLeft, size);
            }
        }

        protected abstract void EndOfFrameLoop();

        protected virtual void CheckForFrameUpdate()
        {
            double currentTime = TimeManager.TotalFrameTime;

            if (lastFrameFlip + durationBetweenFrames < currentTime)
            {
                lastFrameFlip = currentTime;
                ChangeVisibleFrame();
            }
        }

        protected virtual void ChangeVisibleFrame()
        {
            Visible = possibleFrames[currentFrame];
        }

        public void ResetCurrentFrame()
        {
            currentFrame = 0;
            lastFrameFlip = TimeManager.TotalFrameTime;
        }
    }
}
