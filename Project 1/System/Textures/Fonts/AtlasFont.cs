using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using System;
using System.Collections.Generic;

namespace Project_1.Textures
{
    internal sealed class AtlasFont : GameFont
    {
        readonly Texture2D atlasTexture;
        readonly Dictionary<int, AtlasFontGlyph> glyphs;
        readonly Dictionary<long, float> kerningPairs;
        readonly AtlasFontGlyph? missingGlyph;
        readonly AtlasFontGlyph? spaceGlyph;
        readonly float ascenderPx;
        readonly float lineHeightPx;
        readonly float pxRange;
        readonly Point textureSize;

        public AtlasFont(
            string name,
            Texture2D atlas,
            Dictionary<int, AtlasFontGlyph> glyphMap,
            Dictionary<long, float> kerningMap,
            float ascenderPx,
            float lineHeightPx,
            float pxRange) : base(name)
        {
            atlasTexture = atlas ?? throw new ArgumentNullException(nameof(atlas));
            glyphs = glyphMap ?? throw new ArgumentNullException(nameof(glyphMap));
            kerningPairs = kerningMap ?? throw new ArgumentNullException(nameof(kerningMap));
            this.ascenderPx = ascenderPx;
            this.lineHeightPx = lineHeightPx;
            this.pxRange = pxRange;
            textureSize = atlas.Bounds.Size;
            if (glyphs.TryGetValue(0, out AtlasFontGlyph missing)) missingGlyph = missing;
            if (glyphs.TryGetValue(' ', out AtlasFontGlyph space)) spaceGlyph = space;
        }

        public override float LineHeight => lineHeightPx;
        public override float MeasureDescenderDepth(string text)
        {
            //TODO: Something is wonky here but I don't remember what exactly
            //TODO: Too big
            if (string.IsNullOrEmpty(text)) return 0f;

            float penY = 0f;
            float maxDescenderDepth = 0f;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '\r') continue;
                if (c == '\n')
                {
                    penY += lineHeightPx;
                    continue;
                }

                AtlasFontGlyph glyph = ResolveGlyph(c);
                if (glyph.SourceRect.Width <= 0 || glyph.SourceRect.Height <= 0) continue;

                float glyphBottom = penY + ascenderPx - glyph.PlaneBottomPx;
                float baseline = penY + ascenderPx;
                maxDescenderDepth = Math.Max(maxDescenderDepth, glyphBottom - baseline);
            }

            return Math.Max(0f, maxDescenderDepth);
        }

        public override Vector2 MeasureString(string text)
        {
            return MeasureText(text, 0, text?.Length ?? 0);
        }

        public override Vector2 MeasurePartialString(string text, int startIndex, int length)
        {
            return MeasureText(text, startIndex, length);
        }

        public override void DrawString(SpriteBatch batch, string text, Vector2 position, Color color, Vector2 origin, float scale, float layerDepth, Color aBorderColor, float aBorderWidth)
        {
            //TODO: Too big.

            ThreadAffinity.AssertMainThread();
            if (batch == null || string.IsNullOrEmpty(text)) return;

            Effect effect = TextureManager.MsdfTextEffect;
            if (effect != null)
            {
                //Q: Should these parameters be cached in an effect per AtlasFont instance to avoid setting them every draw call?   
                effect.Parameters["TextureSize"]?.SetValue(new Vector2(textureSize.X, textureSize.Y));
                effect.Parameters["PxRange"]?.SetValue(pxRange);
                //
                effect.Parameters["BorderColor"]?.SetValue(aBorderColor.ToVector4());
                effect.Parameters["BorderWidth"]?.SetValue(aBorderWidth);
            }

            Vector2 basePosition = position - origin;
            GraphicsManager.DrawWithTemporaryEffect(batch, effect, _ =>
            {
                float penX = 0f;
                float penY = 0f;
                int previousCodepoint = -1;

                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];
                    if (c == '\r') continue;
                    if (c == '\n')
                    {
                        penX = 0f;
                        penY += lineHeightPx;
                        previousCodepoint = -1;
                        continue;
                    }

                    AtlasFontGlyph glyph = ResolveGlyph(c);
                    if (previousCodepoint >= 0)
                    {
                        penX += GetKerning(previousCodepoint, glyph.Codepoint);
                    }

                    if (glyph.SourceRect.Width > 0 && glyph.SourceRect.Height > 0)
                    {
                        float glyphX = basePosition.X + (penX + glyph.PlaneLeftPx) * scale;
                        float glyphY = basePosition.Y + (penY + ascenderPx - glyph.PlaneTopPx) * scale;

                        Rectangle destination = new Rectangle(
                            (int)MathF.Round(glyphX),
                            (int)MathF.Round(glyphY),
                            Math.Max(1, (int)MathF.Round(glyph.WidthPx * scale)),
                            Math.Max(1, (int)MathF.Round(glyph.HeightPx * scale)));
                        batch.Draw(atlasTexture, destination, glyph.SourceRect, color, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
                    }

                    penX += glyph.AdvancePx;
                    previousCodepoint = glyph.Codepoint;
                }
            });
        }

        Vector2 MeasureText(string text, int startIndex, int length)
        {
            if (string.IsNullOrEmpty(text) || length <= 0) return Vector2.Zero;
            int end = Math.Min(text.Length, startIndex + length);
            if (startIndex < 0 || startIndex >= end) return Vector2.Zero;


            float penX = 0f;
            float penY = 0f;
            float maxX = 0f;
            float height = lineHeightPx;
            float minVisibleY = float.MaxValue;
            float maxVisibleY = float.MinValue;
            bool hasVisibleGlyph = false;
            int previousCodepoint = -1;

            for (int i = startIndex; i < end; i++)
            {
                char c = text[i];
                if (c == '\r') continue;
                if (c == '\n')
                {
                    maxX = Math.Max(maxX, penX);
                    penX = 0f;
                    penY += lineHeightPx;
                    height = penY + lineHeightPx;
                    previousCodepoint = -1;
                    continue;
                }

                AtlasFontGlyph glyph = ResolveGlyph(c);
                if (previousCodepoint >= 0)
                {
                    penX += GetKerning(previousCodepoint, glyph.Codepoint);
                }

                if (glyph.SourceRect.Width > 0 && glyph.SourceRect.Height > 0)
                {
                    float glyphTop = penY + ascenderPx - glyph.PlaneTopPx;
                    float glyphBottom = penY + ascenderPx - glyph.PlaneBottomPx;
                    minVisibleY = Math.Min(minVisibleY, glyphTop);
                    maxVisibleY = Math.Max(maxVisibleY, glyphBottom);
                    hasVisibleGlyph = true;
                }

                penX += glyph.AdvancePx;
                maxX = Math.Max(maxX, penX);
                previousCodepoint = glyph.Codepoint;
            }

            if (hasVisibleGlyph)
            {
                height = Math.Max(0f, maxVisibleY - minVisibleY);
            }

            return new Vector2(maxX, height);
        }

        AtlasFontGlyph ResolveGlyph(int codepoint)
        {
            if (glyphs.TryGetValue(codepoint, out AtlasFontGlyph glyph)) return glyph;
            if (spaceGlyph.HasValue) return spaceGlyph.Value;
            if (missingGlyph.HasValue) return missingGlyph.Value;
            return default;
        }

        float GetKerning(int left, int right)
        {
            long key = (((long)left) << 32) | (uint)right;
            return kerningPairs.TryGetValue(key, out float value) ? value : 0f;
        }
    }
}
