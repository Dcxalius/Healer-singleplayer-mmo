using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Textures.AnimatedTextures
{
    internal class TriggerableAnimatedTexture : Texture
    {
        AnimatedTexture restAnimation;
        AnimatedTexture triggeredAnimation;
        bool triggered = false;
        public TriggerableAnimatedTexture(AnimatedTexture aRestAnimation, AnimatedTexture aTriggeredAnimation,Point aVisableSize) : base(null, aVisableSize)
        {
            restAnimation = aRestAnimation;
            triggeredAnimation = aTriggeredAnimation;
        }

        public void Trigger()
        {
            triggered = true;
            triggeredAnimation.ResetCurrentFrame();
        }

        public override void Update()
        {
            base.Update();
            restAnimation.Update();
            if (triggered == false) return;

            triggeredAnimation.Update();
        }

        public override void Draw(SpriteBatch aBatch, Vector2 aPos, Vector2 aFeetPos)
        {
            if (triggered == true)
            {
                triggeredAnimation.Draw(aBatch, aPos, aFeetPos);
            }
            restAnimation.Draw(aBatch, aPos, aFeetPos);
        }
    }
}
