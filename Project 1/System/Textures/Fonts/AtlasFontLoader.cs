using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Project_1.Textures
{
    internal static class AtlasFontLoader
    {
        sealed class AtlasFontJson
        {
            public AtlasInfo atlas { get; set; }
            public string name { get; set; }
            public MetricsInfo metrics { get; set; }
            public GlyphInfo[] glyphs { get; set; }
            public KerningInfo[] kerning { get; set; }
        }

        sealed class AtlasInfo
        {
            public string type { get; set; }
            public float distanceRange { get; set; }
            public float size { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string yOrigin { get; set; }
        }

        sealed class MetricsInfo
        {
            public float lineHeight { get; set; }
            public float ascender { get; set; }
            public float descender { get; set; }
        }

        sealed class GlyphInfo
        {
            public int index { get; set; }
            public float advance { get; set; }
            public GlyphPlaneBounds planeBounds { get; set; }
            public GlyphAtlasBounds atlasBounds { get; set; }
        }

        sealed class GlyphPlaneBounds
        {
            public float left { get; set; }
            public float bottom { get; set; }
            public float right { get; set; }
            public float top { get; set; }
        }

        sealed class GlyphAtlasBounds
        {
            public float left { get; set; }
            public float bottom { get; set; }
            public float right { get; set; }
            public float top { get; set; }
        }

        sealed class KerningInfo
        {
            public int index1 { get; set; }
            public int index2 { get; set; }
            public float advance { get; set; }
        }

        public static Dictionary<string, GameFont> LoadFonts(string fontDir)
        {
            //TODO: Too big
            Dictionary<string, GameFont> fonts = new Dictionary<string, GameFont>(StringComparer.OrdinalIgnoreCase);
            if (!Directory.Exists(fontDir)) return fonts;

            string[] jsonFiles = Directory.GetFiles(fontDir, "*.json", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < jsonFiles.Length; i++)
            {
                string jsonPath = jsonFiles[i];
                AtlasFontJson fontJson = JsonConvert.DeserializeObject<AtlasFontJson>(File.ReadAllText(jsonPath));
                AtlasFont font = LoadFont(fontDir, jsonPath, fontJson);
                if (font == null) continue;

                fonts[font.Name] = font;
                string fileNameAlias = Path.GetFileNameWithoutExtension(jsonPath);
                if (!string.IsNullOrWhiteSpace(fileNameAlias) && !fonts.ContainsKey(fileNameAlias))
                {
                    fonts[fileNameAlias] = font;
                }
            }

            return fonts;
        }

        static AtlasFont LoadFont(string fontDir, string jsonPath, AtlasFontJson fontJson)
        {
            //TODO: Waaaay to big
            if (fontJson?.atlas == null || fontJson.metrics == null || fontJson.glyphs == null) return null;
            string fontName = string.IsNullOrWhiteSpace(fontJson.name) ? Path.GetFileNameWithoutExtension(jsonPath) : fontJson.name;
            string texturePath = ResolveAtlasTexturePath(fontDir, jsonPath, fontName);
            if (texturePath == null || !File.Exists(texturePath)) return null;
            string ttfPath = ResolveFontFilePath(fontDir, jsonPath, fontName);

            var atlasTexture = Managers.GraphicsManager.CreateTextureFromFile(texturePath);
            float scale = fontJson.atlas.size <= 0f ? 1f : fontJson.atlas.size;
            Dictionary<int, int> codepointToGlyphIndex = TrueTypeGlyphMap.LoadCodepointToGlyphIndex(ttfPath);
            Dictionary<int, List<int>> glyphIndexToCodepoints = InvertGlyphIndexMap(codepointToGlyphIndex);

            Dictionary<int, AtlasFontGlyph> glyphs = new Dictionary<int, AtlasFontGlyph>();
            for (int i = 0; i < fontJson.glyphs.Length; i++)
            {
                GlyphInfo glyph = fontJson.glyphs[i];
                GlyphPlaneBounds plane = glyph.planeBounds;
                GlyphAtlasBounds atlas = glyph.atlasBounds;
                Rectangle sourceRect = Rectangle.Empty;
                if (atlas != null)
                {
                    int left = (int)MathF.Floor(atlas.left);
                    int width = Math.Max(0, (int)MathF.Ceiling(atlas.right - atlas.left));
                    int height = Math.Max(0, (int)MathF.Ceiling(atlas.top - atlas.bottom));
                    int top = (int)MathF.Floor(fontJson.atlas.height - atlas.top);
                    sourceRect = new Rectangle(left, top, width, height);
                }

                AtlasFontGlyph atlasGlyph = new AtlasFontGlyph(
                    glyph.index,
                    glyph.advance * scale,
                    plane?.left * scale ?? 0f,
                    plane?.right * scale ?? 0f,
                    plane?.bottom * scale ?? 0f,
                    plane?.top * scale ?? 0f,
                    sourceRect);

                if (glyphIndexToCodepoints != null && glyphIndexToCodepoints.TryGetValue(glyph.index, out List<int> mappedCodepoints))
                {
                    for (int j = 0; j < mappedCodepoints.Count; j++)
                    {
                        glyphs[mappedCodepoints[j]] = atlasGlyph;
                    }
                }
                else
                {
                    glyphs[glyph.index] = atlasGlyph;
                }
            }

            Dictionary<long, float> kerningPairs = new Dictionary<long, float>();
            if (fontJson.kerning != null)
            {
                for (int i = 0; i < fontJson.kerning.Length; i++)
                {
                    KerningInfo pair = fontJson.kerning[i];
                    long key = (((long)pair.index1) << 32) | (uint)pair.index2;
                    kerningPairs[key] = pair.advance * scale;
                }
            }

            return new AtlasFont(
                fontName,
                atlasTexture,
                glyphs,
                kerningPairs,
                fontJson.metrics.ascender * scale,
                fontJson.metrics.lineHeight * scale,
                fontJson.atlas.distanceRange <= 0f ? 4f : fontJson.atlas.distanceRange);
        }

        static Dictionary<int, List<int>> InvertGlyphIndexMap(Dictionary<int, int> codepointToGlyphIndex)
        {
            if (codepointToGlyphIndex == null || codepointToGlyphIndex.Count == 0)
            {
                return null;
            }

            Dictionary<int, List<int>> glyphIndexToCodepoints = new Dictionary<int, List<int>>();
            foreach (KeyValuePair<int, int> pair in codepointToGlyphIndex)
            {
                if (!glyphIndexToCodepoints.TryGetValue(pair.Value, out List<int> codepoints))
                {
                    codepoints = new List<int>();
                    glyphIndexToCodepoints[pair.Value] = codepoints;
                }

                codepoints.Add(pair.Key);
            }

            return glyphIndexToCodepoints;
        }

        static string ResolveFontFilePath(string fontDir, string jsonPath, string fontName)
        {
            string[] candidates =
            {
                Path.Combine(fontDir, Path.GetFileNameWithoutExtension(jsonPath) + ".ttf"),
                Path.Combine(fontDir, fontName + ".ttf"),
                Path.Combine(fontDir, StripAtlasSuffix(fontName) + ".ttf"),
                Path.Combine(fontDir, Path.GetFileNameWithoutExtension(jsonPath) + ".otf"),
                Path.Combine(fontDir, fontName + ".otf"),
                Path.Combine(fontDir, StripAtlasSuffix(fontName) + ".otf"),
            };

            for (int i = 0; i < candidates.Length; i++)
            {
                if (File.Exists(candidates[i]))
                {
                    return candidates[i];
                }
            }

            return null;
        }

        static string StripAtlasSuffix(string fontName)
        {
            if (string.IsNullOrWhiteSpace(fontName))
            {
                return fontName;
            }

            string[] suffixes = { "-msdf", "-mtsdf", "-sdf" };
            for (int i = 0; i < suffixes.Length; i++)
            {
                if (fontName.EndsWith(suffixes[i], StringComparison.OrdinalIgnoreCase))
                {
                    return fontName.Substring(0, fontName.Length - suffixes[i].Length);
                }
            }

            return fontName;
        }

        static string ResolveAtlasTexturePath(string fontDir, string jsonPath, string fontName)
        {
            string sameBase = Path.Combine(fontDir, Path.GetFileNameWithoutExtension(jsonPath) + ".bmp");
            if (File.Exists(sameBase)) return sameBase;

            string sameName = Path.Combine(fontDir, fontName + ".bmp");
            if (File.Exists(sameName)) return sameName;

            string pngSameBase = Path.Combine(fontDir, Path.GetFileNameWithoutExtension(jsonPath) + ".png");
            if (File.Exists(pngSameBase)) return pngSameBase;

            string pngSameName = Path.Combine(fontDir, fontName + ".png");
            if (File.Exists(pngSameName)) return pngSameName;

            return null;
        }
    }
}
