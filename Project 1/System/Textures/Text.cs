using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using System;
using System.Diagnostics;

namespace Project_1.Textures
{
    internal class Text
    {
        internal const float DefaultTextSize = 12f;

        public virtual string Value
        {
            get => textToDisplay;

            set
            {
                textToDisplay = value;
                RecalculateOffset();
            }
        }

        string textToDisplay;

        public virtual Color Color
        {
            get => color;
            set => color = value;
        }

        public float TextSize
        {
            get => textSize;
            set
            {
                textSize = value <= 0f ? DefaultTextSize : value;
                UpdateScale();
                RecalculateOffset();
            }
        }

        public Vector2 Offset => offset;
        Vector2 offset;

        public Vector2 CalculatePartialOffset(int aIndexToCalculateTo) => CalculatePartialOffset(0, aIndexToCalculateTo);
        public Vector2 CalculatePartialOffset(int aStartIndex, int aIndexToCalculateTo) => MeasureScaledPartial(textToDisplay, font, scale, aStartIndex, aIndexToCalculateTo);
        public static Vector2 CalculateOffset(string aString, string aFontName, float aTextSize = DefaultTextSize) => CalculateOffset(aString, FontCache.GetFont(aFontName), aTextSize);
        public static Vector2 CalculateOffset(string aString, GameFont aFont, float aTextSize = DefaultTextSize) => MeasureScaled(aString, aFont, GetScale(aFont, aTextSize));
        public static Vector2 CalculatePartialOffset(string aString, string aFontName, int aIndexToCalculateTo, float aTextSize = DefaultTextSize) => CalculatePartialOffset(aString, FontCache.GetFont(aFontName), 0, aIndexToCalculateTo, aTextSize);
        public static Vector2 CalculatePartialOffset(string aString, string aFontName, int aStartIndex, int aIndexToCalculateTo, float aTextSize = DefaultTextSize) => CalculatePartialOffset(aString, FontCache.GetFont(aFontName), aStartIndex, aIndexToCalculateTo, aTextSize);
        public static Vector2 CalculatePartialOffset(string aString, GameFont aFont, int aIndexToCalculateTo, float aTextSize = DefaultTextSize) => CalculatePartialOffset(aString, aFont, 0, aIndexToCalculateTo, aTextSize);
        public static Vector2 CalculatePartialOffset(string aString, GameFont aFont, int aStartIndex, int aIndexToCalculateTo, float aTextSize = DefaultTextSize) => MeasureScaledPartial(aString, aFont, GetScale(aFont, aTextSize), aStartIndex, aIndexToCalculateTo);

        public GameFont Font => font;
        protected GameFont font;
        Color color;
        float scale;
        float textSize;
        Color borderColor;
        float borderWidth;

        public Text(string aFontName, float aTextSize = DefaultTextSize, Color? aBorderColor = null, float aBorderWidth = 0) : this(aFontName, null, Color.White, aTextSize, aBorderColor, aBorderWidth) { }
        public Text(string aFontName, string aTextToStart, float aTextSize = DefaultTextSize, Color? aBorderColor = null, float aBorderWidth = 0) : this(aFontName, aTextToStart, Color.White, aTextSize, aBorderColor, aBorderWidth) { }
        public Text(string aFontName, Color aColor, float aTextSize = DefaultTextSize, Color? aBorderColor = null, float aBorderWidth = 0) : this(aFontName, null, aColor, aTextSize, aBorderColor, aBorderWidth) { }

        public Text(string aFontName, string aTextToStart, Color aColor, float aTextSize = DefaultTextSize, Color? aBorderColor = null, float aBorderWidth = 0)
        {
            font = FontCache.GetFont(aFontName);
            Debug.Assert(font != null, "Font not found");
            Debug.Assert(!((aBorderColor == null && aBorderWidth > 0) || (aBorderColor != null && aBorderWidth == 0)), "Border color and width must be set together");
            borderColor = aBorderColor ?? Color.Transparent;
            borderWidth = aBorderWidth;
            textSize = aTextSize <= 0f ? DefaultTextSize : aTextSize;
            color = aColor;
            UpdateScale();
            Value = aTextToStart;
        }

        public void Rescale()
        {
            UpdateScale();
            RecalculateOffset();
        }

        public void TopLeftDraw(SpriteBatch aBatch, AbsoluteScreenPosition aPos) => Draw(aBatch, aPos, Vector2.Zero);
        public void TopCentreDraw(SpriteBatch aBatch, AbsoluteScreenPosition aPos) => Draw(aBatch, aPos, new Vector2(offset.X / 2, 0));
        public void TopRightDraw(SpriteBatch aBatch, AbsoluteScreenPosition aPos) => Draw(aBatch, aPos, new Vector2(offset.X, 0));

        public void CentreLeftDraw(SpriteBatch aBatch, AbsoluteScreenPosition aPos) => Draw(aBatch, aPos, new Vector2(0, GetCenteredYOffset()));
        public void CentredDraw(SpriteBatch aBatch, AbsoluteScreenPosition aPos) => Draw(aBatch, aPos, new Vector2(offset.X / 2, GetCenteredYOffset()));
        public void CentreRightDraw(SpriteBatch aBatch, AbsoluteScreenPosition aPos) => Draw(aBatch, aPos, new Vector2(offset.X, GetCenteredYOffset()));

        public void BottomLeftDraw(SpriteBatch aBatch, AbsoluteScreenPosition aPos) => Draw(aBatch, aPos, new Vector2(0, offset.Y));
        public void BottomCentreDraw(SpriteBatch aBatch, AbsoluteScreenPosition aPos) => Draw(aBatch, aPos, new Vector2(offset.X / 2, offset.Y));
        public void BottomRightDraw(SpriteBatch aBatch, AbsoluteScreenPosition aPos) => Draw(aBatch, aPos, new Vector2(offset.X, offset.Y));

        void Draw(SpriteBatch aBatch, AbsoluteScreenPosition aPos, Vector2 aOffset)
        {
            ThreadAffinity.AssertMainThread();
            if (textToDisplay == null) return;

            font.DrawString(aBatch, textToDisplay, aPos.ToVector2(), color, aOffset, scale, 1f, borderColor, borderWidth);
        }

        void RecalculateOffset()
        {
            offset = MeasureScaled(textToDisplay, font, scale);
        }

        void UpdateScale()
        {
            scale = GetScale(font, textSize);
        }

        float GetCenteredYOffset()
        {
            float descenderHeight = font?.MeasureDescenderDepth(textToDisplay) * scale ?? 0f;
            return Math.Max(0f, (offset.Y - descenderHeight) / 2f);
        }

        static float GetScale(GameFont aFont, float aTextSize)
        {
            if (aFont == null) return 1f;
            float lineHeight = Math.Max(1f, aFont.LineHeight);
            return aTextSize / lineHeight;
        }

        static Vector2 MeasureScaled(string aString, GameFont aFont, float aScale)
        {
            if (aFont == null || string.IsNullOrEmpty(aString)) return Vector2.Zero;
            return aFont.MeasureString(aString) * aScale;
        }

        static Vector2 MeasureScaledPartial(string aString, GameFont aFont, float aScale, int aStartIndex, int aIndexToCalculateTo)
        {
            if (aFont == null || string.IsNullOrEmpty(aString) || aIndexToCalculateTo <= 0) return Vector2.Zero;
            return aFont.MeasurePartialString(aString, aStartIndex, aIndexToCalculateTo) * aScale;
        }
    }
}
