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
                    base.Value = 1f;
                    fractionText.Text = null;
                    percentageText.Text = null;
                    return;
                }
                maxValue = value;
                base.Value = currentValue / maxValue;
                fractionText.Text = currentValue + "/" + maxValue;
                percentageText.Text = (int)(currentValue / maxValue * 100) + "%";
            }
        }
        public override float Value
        {
            set
            {
                if (value < 0) value = 0;

                if (value == 0 && maxValue == 0)
                {
                    //currentValue = 0;
                    base.Value = 1f;
                    fractionText.Text = null;
                    percentageText.Text = null;
                    return;
                }
                currentValue = value;
                base.Value = currentValue / maxValue;
                fractionText.Text = Math.Round(currentValue) + "/" + maxValue;
                percentageText.Text = (int)(currentValue / maxValue * 100) + "%";
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

        public ResourceBar(BarTexture aBarGfx, UITexture aBackgroundGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize, UIElement aParent = null) : base(aBarGfx, aBackgroundGfx, aPos, aSize, aParent)
        {
            fractionText = new Label("", RelativeScreenPosition.Zero, aSize, Label.TextAllignment.CentreLeft, Color.Black, "Comforaa-msdf", 13, this);
            percentageText = new Label("", RelativeScreenPosition.Zero, aSize, Label.TextAllignment.CentreRight, Color.Black, "Comforaa-msdf", 14, this);
            AddChild(fractionText);
            AddChild(percentageText);
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
