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
    internal class AnimatedTexture : Texture
    {
        public enum AnimationType
        {
            Normal,
            Looping,
            Random
        }
        
        Rectangle[] possibleFrames;
        int currentFrame = 0;
        double durationBetweenFrames;
        double lastFrameFlip;

        AnimationType animationType;
        
        bool loopingBackwards;


        public AnimatedTexture(GfxPath path, Point aVisableSize, AnimationType aType, int aDeadFrameCount, TimeSpan aTimePerFrame) : base(path, aVisableSize)
        {
            durationBetweenFrames = (double)aTimePerFrame.TotalMilliseconds;
            animationType = aType;

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

        void EndOfFrameLoop()
        {
            if (currentFrame >= possibleFrames.Length - 1 && loopingBackwards == false)
            {
                switch (animationType)
                {
                    case AnimationType.Normal:
                        currentFrame = 0;
                        break;
                    case AnimationType.Looping:
                        loopingBackwards = true;
                        break;
                    case AnimationType.Random:
                        //Do nothing
                        break;
                    default:
                        break;
                }

            }

            if (currentFrame == 0 && animationType == AnimationType.Looping)
            {
                loopingBackwards = false;
            }
        }

        void CheckForFrameUpdate()
        {
            double currentTime = TimeManager.gt.TotalGameTime.TotalMilliseconds;

            if (lastFrameFlip + durationBetweenFrames < TimeManager.gt.ElapsedGameTime.TotalMilliseconds + currentTime)
            {
                //Console.WriteLine(":)");
                lastFrameFlip = currentTime;

                switch (animationType)
                {
                    case AnimationType.Normal:
                        currentFrame++;
                        break;
                    case AnimationType.Looping:
                        if (loopingBackwards == false)
                        {
                            currentFrame++;

                        }
                        else
                        {
                            currentFrame--;
                        }
                        break;
                    case AnimationType.Random:
                        currentFrame = RandomManager.RollIntWithAvoidant(possibleFrames.Length, currentFrame);
                        break;
                    default:
                        break;
                }

                visible = possibleFrames[currentFrame];
            }
        }
    }
}
