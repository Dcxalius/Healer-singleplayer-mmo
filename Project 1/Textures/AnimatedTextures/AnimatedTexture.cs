using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using SharpDX.Direct3D11;
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
            visible = possibleFrames[0];
        }

        public override void Update()
        {
            CheckForFrameUpdate();
            EndOfFrameLoop();

            base.Update();
        }

        void CreateAnimationFrames(int aDeadFrameCount)
        {
            int rectsInXDir = gfx.Bounds.Width / size.X;
            int rectsInYDir = gfx.Bounds.Height / size.Y;
            possibleFrames = new Rectangle[rectsInXDir * rectsInYDir - aDeadFrameCount];


            for (int i = 0; i < possibleFrames.Length; i++)
            {
                Point topLeft = new Point((i % rectsInXDir) * size.X, (int)Math.Floor((decimal)i / rectsInXDir) * size.Y);
                possibleFrames[i] = new Rectangle(topLeft, size);
            }
        }

        protected abstract void EndOfFrameLoop();

        protected virtual void CheckForFrameUpdate()
        {
            double currentTime = TimeManager.CurrentFrameTime;

            if (lastFrameFlip + durationBetweenFrames < currentTime)
            {
                lastFrameFlip = currentTime;
                ChangeVisibleFrame();
            }
        }

        protected virtual void ChangeVisibleFrame()
        {
            visible = possibleFrames[currentFrame];
        }

        public void ResetCurrentFrame()
        {
            currentFrame = 0;
            lastFrameFlip = TimeManager.CurrentFrameTime;
        }
    }
}
