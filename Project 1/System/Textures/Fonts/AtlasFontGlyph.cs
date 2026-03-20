using Microsoft.Xna.Framework;

namespace Project_1.Textures
{
    internal readonly struct AtlasFontGlyph
    {
        public AtlasFontGlyph(int codepoint, float advancePx, float planeLeftPx, float planeRightPx, float planeBottomPx, float planeTopPx, Rectangle sourceRect)
        {
            Codepoint = codepoint;
            AdvancePx = advancePx;
            PlaneLeftPx = planeLeftPx;
            PlaneRightPx = planeRightPx;
            PlaneBottomPx = planeBottomPx;
            PlaneTopPx = planeTopPx;
            SourceRect = sourceRect;
        }

        public int Codepoint { get; }
        public float AdvancePx { get; }
        public float PlaneLeftPx { get; }
        public float PlaneRightPx { get; }
        public float PlaneBottomPx { get; }
        public float PlaneTopPx { get; }
        public Rectangle SourceRect { get; }
        public float WidthPx => PlaneRightPx - PlaneLeftPx;
        public float HeightPx => PlaneTopPx - PlaneBottomPx;
    }
}
