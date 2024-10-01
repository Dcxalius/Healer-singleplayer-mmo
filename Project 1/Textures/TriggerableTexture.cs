using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Textures
{
    internal class TriggerableTexture : AnimatedTexture
    {
        //AnimatedTexture restTexture;
        bool triggered = false;
        public TriggerableTexture(GfxPath aRestTexture,  Point aVisableSize, AnimationType aType, int aDeadFrameCount, TimeSpan aTimePerFrame) : base(aRestTexture, aVisableSize, aType, aDeadFrameCount, aTimePerFrame)
        {
        }

        public override void Update()
        {
            base.Update();

            if (triggered == false) return;

            switch (animationType)
            {
                case AnimationType.Normal:
                    if (currentFrame == possibleFrames.Length) triggered = true;
                    break;
                case AnimationType.Looping:
                    break;
                case AnimationType.Random:
                    break;
                default:
                    break;
            }
        }

        public override void Draw(SpriteBatch aBatch, Rectangle aPosRectangle)
        {
            if (triggered == true)
            {
                base.Draw(aBatch, aPosRectangle);
            }
            //restTexture.Draw(aBatch, aPosRectangle);
        }

        public override void Draw(SpriteBatch aBatch, Vector2 aPos, Vector2 aFeetPos)
        {
            if (triggered == true)
            {
                base.Draw(aBatch, aPos, aFeetPos);
            }
           // restTexture.Draw(aBatch, aPos, aFeetPos);
        }
    }
}
