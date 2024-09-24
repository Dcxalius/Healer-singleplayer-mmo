using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Textures
{
    internal class Text
    {
        public string Value
        {
            get => textToDisplay;

            set
            {
                textToDisplay = value;
                offset = font.MeasureString(value); 
            }
        }


        string textToDisplay;
        Vector2 offset;
        SpriteFont font;
        Color color;
        
        public Text(string aFontName)
        {
            textToDisplay = null;
            font = TextureManager.GetFont(aFontName);
            color = Color.White;
        }
        public Text(string aFontName, Color aColor)
        {
            textToDisplay = null;
            font = TextureManager.GetFont(aFontName);
            color = aColor;
        }

        public Text(string aFontName, string aTextToStart)
        {
            font = TextureManager.GetFont(aFontName);
            Value = aTextToStart;
            color = Color.White;
        }
        public Text(string aFontName, string aTextToStart, Color aColor)
        {
            font = TextureManager.GetFont(aFontName);
            Value = aTextToStart;
            color = aColor;
        }


        public void CentredDraw(SpriteBatch aBatch, Vector2 aPos) //Offsets by half of textsize
        {
            Draw(aBatch, aPos, offset / 2);
        }

        public void LeftAllignedDraw(SpriteBatch aBatch, Vector2 aPos) //Offset by half y
        {
            Draw(aBatch, aPos, new Vector2(0, offset.Y / 2));
        }

        public void RightAllignedDraw(SpriteBatch aBatch, Vector2 aPos) //Offset by textlength and half y
        {
            Draw(aBatch, aPos, new Vector2(offset.X, offset.Y / 2));
        }

        void Draw(SpriteBatch aBatch, Vector2 aPos, Vector2 aOffset)
        {
            if (textToDisplay == null)
            {
                DebugManager.Print(GetType(), "Tried to print empty text");
                return;
            }
            aBatch.DrawString(font, textToDisplay, aPos, color, 0f, aOffset, 1f, SpriteEffects.None, 1f);
        }
    }
}
