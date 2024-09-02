using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.SelectBoxes
{
    internal abstract class SelectBoxValue : UIElement
    {
        public enum SelectBoxValueTypes
        {
            Int,
            String,
            ScreenRez
        }

        public string DisplayText { get => textToDisplay; }
        public SelectBoxValueTypes Type { get => type; }
        
        

        //public 

        string textToDisplay;

        static SpriteFont font;
        Vector2 textSize;
        SelectBoxValueTypes type;
        
        protected SelectBoxValue(SelectBoxValueTypes aType, UITexture aGfx, string aStartText, Vector2 aPos, Vector2 aSize) : base(aGfx, aPos, aSize)
        { 
            type = aType;
            if (font == null)
            {
                font = GraphicsManager.buttonFont; //ECH
            }

            textToDisplay = aStartText;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            spriteBatch.DrawString(font, textToDisplay, pos.Location.ToVector2(), Color.White);
        }
    }
}
