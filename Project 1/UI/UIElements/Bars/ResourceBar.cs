using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Bars
{
    internal class ResourceBar : Bar
    {

        public float MaxValue { get => maxValue; set => maxValue = value; }
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
                base.Value = value / maxValue;
                fractionText.Value = currentValue + "/" + maxValue;
                percentageText.Value = (int)(currentValue / maxValue * 100) + "%";
            }
        }


        float currentValue;
        float maxValue;
        Text fractionText;
        Text percentageText;

        public ResourceBar(BarTexture aBarGfx, UITexture aBackgroundGfx, Vector2 aPos, Vector2 aSize) : base(aBarGfx, aBackgroundGfx, aPos, aSize)
        {
            fractionText = new Text("Gloryse", Color.Black);
            percentageText = new Text("Gloryse", Color.Black);
            MaxValue = 1f;
            Value = 1f;
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

            fractionText.CentredDraw(aBatch, AbsolutePos.Center.ToVector2());
            fractionText.LeftAllignedDraw(aBatch, new Vector2(AbsolutePos.Left + 5, AbsolutePos.Center.Y));
            percentageText.RightAllignedDraw(aBatch, new Vector2(AbsolutePos.Right - 5, AbsolutePos.Center.Y));
        }
    }
}
