using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
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
                if (value < 0) value = 0;
                if (value == 0)
                {
                    maxValue = 0;
                    currentValue = 0;
                    base.Value = 1f;
                    fractionText.Text = null;
                    percentageText.Text = null;
                    return;
                }
                maxValue = value;
                currentValue = Math.Clamp(currentValue, 0f, maxValue);
                float ratio = currentValue / maxValue;
                base.Value = ratio;
                fractionText.Text = Math.Round(currentValue) + "/" + Math.Round(maxValue);
                percentageText.Text = (int)(ratio * 100) + "%";
            }
        }
        public override float Value
        {
            set
            {
                if (value < 0) value = 0;
                currentValue = maxValue > 0 ? Math.Clamp(value, 0f, maxValue) : 0f;

                if (maxValue == 0)
                {
                    base.Value = 1f;
                    fractionText.Text = null;
                    percentageText.Text = null;
                    return;
                }

                float ratio = currentValue / maxValue;
                base.Value = ratio;
                fractionText.Text = Math.Round(currentValue) + "/" + Math.Round(maxValue);
                percentageText.Text = (int)(ratio * 100) + "%";
            }
        }


        float currentValue;
        float maxValue;

        public enum Labels
        {
            Fraction,
            Percentage
        }

        Label fractionText;
        Label percentageText;

        public ResourceBar(UIElement aParent, BarTexture aBarGfx, UITexture aBackgroundGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, aBarGfx, aBackgroundGfx, aPos, aSize)
        {
            fractionText = new Label(this, RelativeScreenPosition.Zero, RelativeScreenPosition.One, Label.TextAllignment.CentreLeft, Color.Black, "Comfortaa-msdf", 13, "");
            percentageText = new Label(this, RelativeScreenPosition.Zero, RelativeScreenPosition.One, Label.TextAllignment.CentreRight, Color.Black, "Comfortaa-msdf", 14, "");
            maxValue = 0f;
            currentValue = 0f;
        }
        public void SetLabelTextSize(Labels aLabel, float aSize)
        {
            switch (aLabel)
            {
                case Labels.Fraction:
                    fractionText.TextSize = aSize;
                    break;
                case Labels.Percentage:
                    percentageText.TextSize = aSize;
                    break;
            }
        }

    }
}
