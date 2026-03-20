using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Project_1.Textures
{
    internal abstract class GameFont
    {
        protected GameFont(string name)
        {
            Name = name ?? string.Empty;
        }

        public string Name { get; }
        public abstract float LineHeight { get; }
        public abstract Vector2 MeasureString(string text);
        public abstract Vector2 MeasurePartialString(string text, int startIndex, int length);
        public abstract void DrawString(SpriteBatch batch, string text, Vector2 position, Color color, Vector2 origin, float scale, float layerDepth);
    }
}
