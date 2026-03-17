using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Textures.AnimatedTextures
{
    internal class LoopingAnimatedTexture : AnimatedTexture
    {
        bool loopingBackwards = false;

        public LoopingAnimatedTexture(GfxPath path, Point aVisableSize, int aDeadFrameCount, TimeSpan aTimePerFrame) : base(path, aVisableSize, aDeadFrameCount, aTimePerFrame)
        {

        }

        protected override void EndOfFrameLoop()
        {
            if (currentFrame >= possibleFrames.Length - 1 && loopingBackwards == false)
            {
                loopingBackwards = true;
            }

            if (currentFrame == 0)
            {
                loopingBackwards = false;
            }
        }


        protected override void ChangeVisibleFrame()
        {
            if (loopingBackwards == false)
            {
                currentFrame++;

            }
            else
            {
                currentFrame--;
            }
            base.ChangeVisibleFrame();
        }
    }
}
