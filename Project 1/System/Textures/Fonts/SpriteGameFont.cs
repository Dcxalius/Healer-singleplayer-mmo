using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Project_1.Textures
{
    internal sealed class SpriteGameFont : GameFont
    {
        readonly SpriteFont spriteFont;

        public SpriteGameFont(string name, SpriteFont font) : base(name)
        {
            spriteFont = font;
        }

        public override float LineHeight => spriteFont?.LineSpacing ?? 0f;

        public override Vector2 MeasureString(string text)
        {
            if (spriteFont == null || string.IsNullOrEmpty(text)) return Vector2.Zero;
            return spriteFont.MeasureString(text);
        }

        public override Vector2 MeasurePartialString(string text, int startIndex, int length)
        {
            if (spriteFont == null || string.IsNullOrEmpty(text) || length <= 0) return Vector2.Zero;
            return spriteFont.MeasureString(text.Substring(startIndex, length));
        }

        public override void DrawString(SpriteBatch batch, string text, Vector2 position, Color color, Vector2 origin, float scale, float layerDepth)
        {
            if (spriteFont == null || string.IsNullOrEmpty(text)) return;
            batch.DrawString(spriteFont, text, position, color, 0f, origin, scale, SpriteEffects.None, layerDepth);
        }
    }
}
