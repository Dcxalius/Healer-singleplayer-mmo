using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Bars
{
    internal class ResourceBar : Bar //TODO: find better name
    {

        public float MaxValue
        {
            get => maxValue; 
            set
            {
                if (currentValue == 0 && value == 0)
                {
                    base.Value = 1f;
                    fractionText.Value = null;
                    percentageText.Value = null;
                    return;
                }
                maxValue = value;
                base.Value = currentValue / maxValue;
                fractionText.Value = currentValue + "/" + maxValue;
                percentageText.Value = (int)(currentValue / maxValue * 100) + "%";
            }
        }
        public override float Value
        {
            set
            {
                if (value == 0 && maxValue == 0)
                {
                    base.Value = 1f;
                    fractionText.Value = null;
                    percentageText.Value = null;
                    return;
                }
                currentValue = value;
                base.Value = currentValue / maxValue;
                fractionText.Value = Math.Round(currentValue) + "/" + maxValue;
                percentageText.Value = (int)(currentValue / maxValue * 100) + "%";
            }
        }


        float currentValue;
        float maxValue;
        Text fractionText;
        Text percentageText;

        public ResourceBar(BarTexture aBarGfx, UITexture aBackgroundGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aBarGfx, aBackgroundGfx, aPos, aSize)
        {
            fractionText = new Text("Gloryse", Color.Black);
            percentageText = new Text("Gloryse", Color.Black);
            maxValue = 0f;
            currentValue = 0f;
        }


        public override void Rescale()
        {
            base.Rescale();
            fractionText.Rescale();
            percentageText.Rescale();
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);

            //fractionText.CentredDraw(aBatch, AbsolutePos.Center.ToVector2());
            fractionText.CentreLeftDraw(aBatch, new AbsoluteScreenPosition(AbsolutePos.Left + 5, AbsolutePos.Center.Y));
            percentageText.CentreRightDraw(aBatch, new AbsoluteScreenPosition(AbsolutePos.Right - 5, AbsolutePos.Center.Y));
        }
    }
}
