using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Textures;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.SelectBoxes
{
    internal abstract class SelectBoxValue : UIElement
    {
        public enum SelectBoxValueTypes
        {
            Int,
            String,
            CameraSetting,
            ScreenRez
        }

        public string DisplayText { get => textToDisplay; }
        public SelectBoxValueTypes Type { get => type; }



        //public 

        protected string textToDisplay;

        protected static SpriteFont font;
        protected Vector2 textSize;
        SelectBoxValueTypes type;

        protected SelectBoxValue(SelectBoxValueTypes aType, UITexture aGfx, string aStartText, Vector2 aPos, Vector2 aSize) : base(aGfx, aPos, aSize)
        {
            type = aType;
            if (font == null)
            {
                font = GraphicsManager.buttonFont; //ECH
            }

            textToDisplay = aStartText;
            textSize = font.MeasureString(textToDisplay);
        }

        public override void Rescale()
        {
            base.Rescale();
            
            textSize = font.MeasureString(textToDisplay);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            spriteBatch.DrawString(font, textToDisplay, new Vector2(AbsolutePos.X + AbsolutePos.Size.X / 2, AbsolutePos.Y + AbsolutePos.Size.Y / 2), Color.Teal, 0f, textSize / 2, 1f, SpriteEffects.None, 1f);
        }
    }
}
