using Microsoft.Xna.Framework;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Textures.AnimatedTextures
{
    internal class RandomAnimatedTexture : AnimatedTexture
    {
        public RandomAnimatedTexture(GfxPath path, Point aVisableSize, int aDeadFrameCount, TimeSpan aTimePerFrame) : base(path, aVisableSize, aDeadFrameCount, aTimePerFrame)
        {
        }

        protected override void CheckForFrameUpdate()
        {
            base.CheckForFrameUpdate();

        }

        protected override void EndOfFrameLoop(){ /*Do nothing */ }
        protected override void ChangeVisibleFrame()
        {
            currentFrame = RandomManager.RollIntWithAvoidant(possibleFrames.Length, currentFrame);
            base.ChangeVisibleFrame();
        }

    }
}
