using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
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
    [DebuggerStepThrough]
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
        
        public Text(string aFontName) : this(aFontName, null, Color.White) { }
        public Text(string aFontName, string aTextToStart) : this(aFontName, aTextToStart, Color.White) { }
        public Text(string aFontName, Color aColor) : this(aFontName, null, aColor) { }

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


        public void TopLeftDraw(SpriteBatch aBatch, AbsoluteScreenPosition aPos, float aLayer) => Draw(aBatch, aPos, Vector2.Zero, aLayer);
        public void TopCentreDraw(SpriteBatch aBatch, AbsoluteScreenPosition aPos, float aLayer) => Draw(aBatch, aPos, new Vector2(offset.X / 2, 0), aLayer);
        public void TopRightDraw(SpriteBatch aBatch, AbsoluteScreenPosition aPos, float aLayer) => Draw(aBatch, aPos, new Vector2(offset.X, 0), aLayer);

        public void CentreLeftDraw(SpriteBatch aBatch, AbsoluteScreenPosition aPos, float aLayer) => Draw(aBatch, aPos, new Vector2(0, offset.Y / 2), aLayer); //Offset by half y
        public void CentredDraw(SpriteBatch aBatch, AbsoluteScreenPosition aPos, float aLayer) => Draw(aBatch, aPos, offset / 2, aLayer); //Offsets by half of textsize
        public void CentreRightDraw(SpriteBatch aBatch, AbsoluteScreenPosition aPos, float aLayer) => Draw(aBatch, aPos, new Vector2(offset.X, offset.Y / 2), aLayer); //Offset by textlength and half y

        public void BottomLeftDraw(SpriteBatch aBatch, AbsoluteScreenPosition aPos, float aLayer) => Draw(aBatch, aPos, new Vector2(0, offset.Y), aLayer);
        public void BottomCentreDraw(SpriteBatch aBatch, AbsoluteScreenPosition aPos, float aLayer) => Draw(aBatch, aPos, new Vector2(offset.X / 2, offset.Y), aLayer);
        public void BottomRightDraw(SpriteBatch aBatch, AbsoluteScreenPosition aPos, float aLayer) => Draw(aBatch, aPos, new Vector2(offset.X, offset.Y), aLayer);



        void Draw(SpriteBatch aBatch, AbsoluteScreenPosition aPos, Vector2 aOffset, float aLayer)
        {
            if (textToDisplay == null) return;

            aBatch.DrawString(font, textToDisplay, aPos.ToVector2(), color, 0f, aOffset, scale, SpriteEffects.None, aLayer);
        }
    }
}
