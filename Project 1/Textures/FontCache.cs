using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Project_1.Textures
{
    internal static class FontCache
    {
        static Dictionary<string, SpriteFont> fonts;
        static string fallbackFont;
        static bool initialized;

        public static void Init(Dictionary<string, SpriteFont> fontDict, string fallback)
        {
            if (initialized) return;
            if (fontDict == null) throw new ArgumentNullException(nameof(fontDict));
            fonts = new Dictionary<string, SpriteFont>(fontDict);
            fallbackFont = fallback;
            initialized = true;
        }

        public static SpriteFont GetFont(string fontName)
        {
            if (fonts == null)
                throw new InvalidOperationException("FontCache is not initialized.");

            if (!fonts.TryGetValue(fontName, out var font))
            {
                if (!fonts.TryGetValue(fallbackFont, out font))
                    throw new KeyNotFoundException($"Fallback font '{fallbackFont}' not found. Loaded fonts: {string.Join(", ", fonts.Keys)}");
            }
            return font;
        }
    }
}
