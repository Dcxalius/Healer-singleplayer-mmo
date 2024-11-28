using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using SharpDX.MediaFoundation;
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
        public virtual string Value
        {
            get => textToDisplay;

            set
            {
                textToDisplay = value;
                if (value == null) return;
                offset = font.MeasureString(value) * Camera.Camera.Zoom; 
            }
        }

        public Vector2 Offset { get => offset; }

        string textToDisplay;
        Vector2 offset;
        protected SpriteFont font;
        Color color;
        float scale;
        
        public Text(string aFontName)
        {
            font = TextureManager.GetFont(aFontName);
            scale = Camera.Camera.Zoom;
            textToDisplay = null;
            color = Color.White;
        }

        public Text(string aFontName, string aTextToStart)
        {
            font = TextureManager.GetFont(aFontName);
            scale = Camera.Camera.Zoom;
            Value = aTextToStart;
            color = Color.White;
        }

        public Text(string aFontName, Color aColor)
        {
            font = TextureManager.GetFont(aFontName);
            scale = Camera.Camera.Zoom;
            textToDisplay = null;
            color = aColor;
        }

        public Text(string aFontName, string aTextToStart, Color aColor)
        {
            font = TextureManager.GetFont(aFontName);
            scale = Camera.Camera.Zoom;
            Value = aTextToStart;
            color = aColor;
        }

        public void Rescale()
        {
            scale = Camera.Camera.Zoom;
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
                return;
            }
            aBatch.DrawString(font, textToDisplay, aPos, color, 0f, aOffset, scale, SpriteEffects.None, 1f);
        }
    }
}
