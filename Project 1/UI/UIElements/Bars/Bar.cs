using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Bars
{
    internal class Bar : Box
    {
        public float MaxValue { get => maxValue; set => maxValue = value; }
        public float Value 
        { 
            get => currentValue;
            set
            {
                currentValue = value;
                barComponent.UpdateBar(value / maxValue);
                barText = currentValue + "/" + maxValue;
                textSize = font.MeasureString(barText);
            }
        }

        BarComponent barComponent;
        float currentValue;
        float maxValue;
        SpriteFont font = GraphicsManager.buttonFont;
        string barText;
        Vector2 textSize;
        //TextAnchor
        

        public Bar(float aMaxValue, BarTexture aBarGfx, UITexture aBackgroundGfx, Vector2 aPos, Vector2 aSize) : base(aBackgroundGfx, aPos, aSize)
        {
            barComponent = new BarComponent(aBarGfx, Vector2.Zero, aSize);
            children.Add(barComponent);
            Value = aMaxValue;
            maxValue = aMaxValue;
        }
        
        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);
            aBatch.DrawString(font, barText, new Vector2(AbsolutePos.X + Size.X / 2, AbsolutePos.Y + Size.Y / 2), Color.Black, 0f, textSize / 2, 1f, SpriteEffects.None, 1f);

        }
    }
}
