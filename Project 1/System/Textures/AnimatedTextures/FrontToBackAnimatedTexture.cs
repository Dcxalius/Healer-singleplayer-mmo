using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Textures.AnimatedTextures
{
    internal class FrontToBackAnimatedTexture : AnimatedTexture
    {
        public FrontToBackAnimatedTexture(GfxPath path, Point aVisableSize, int aDeadFrameCount, TimeSpan aTimePerFrame) : base(path, aVisableSize, aDeadFrameCount, aTimePerFrame)
        {

        }

        protected override void EndOfFrameLoop()
        {
            if (currentFrame >= possibleFrames.Length - 1)
            {
                currentFrame = 0;
            }
        }

        protected override void ChangeVisibleFrame() 
        {
            currentFrame++;
            base.ChangeVisibleFrame();
        }
    }
}
