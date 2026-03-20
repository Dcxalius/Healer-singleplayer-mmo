using System;
using System.Collections.Generic;

namespace Project_1.Textures
{
    internal static class FontCache
    {
        static Dictionary<string, GameFont> fonts;
        static string fallbackFont;
        static bool initialized;

        public static void Init(Dictionary<string, GameFont> fontDict, string fallback)
        {
            if (initialized) return;
            if (fontDict == null) throw new ArgumentNullException(nameof(fontDict));
            fonts = new Dictionary<string, GameFont>(fontDict, StringComparer.OrdinalIgnoreCase);
            fallbackFont = fallback;
            initialized = true;
        }

        public static GameFont GetFont(string fontName)
        {
            if (fonts == null)
                throw new InvalidOperationException("FontCache is not initialized.");

            if (!fonts.TryGetValue(fontName, out GameFont font))
            {
                if (!fonts.TryGetValue(fallbackFont, out font))
                    throw new KeyNotFoundException($"Fallback font '{fallbackFont}' not found. Loaded fonts: {string.Join(", ", fonts.Keys)}");
            }

            return font;
        }
    }
}
