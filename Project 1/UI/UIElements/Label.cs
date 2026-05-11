using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
{
    internal class Label : UIElement
    {
        public enum TextAllignment
        {
            CentreLeft,
            CentreRight,
            Centred,
            TopLeft,
            TopCentre,
            TopRight,
            BottomLeft,
            BottomCentre,
            BottomRight
        }

        public string Text
        {
            get => underlyingText.Value;
            set
            {
                string fixedValue = TextWidthFixer(value); //TODO: If length is longer than Size.Y and a toggle is on then the text should become scrollable.
                if (underlyingText.Value == fixedValue) return;
                underlyingText.Value = fixedValue;
                MarkRenderStale();
            }
        }

        public string TextWidthFixer(string s)
        {
            if (s == null) return null;
            if (Textures.Text.CalculateOffset(s, underlyingText.Font, underlyingText.TextSize).X <= Size.X
                || Textures.Text.CalculateOffset(s, underlyingText.Font, underlyingText.TextSize).X == 0
                || Size.X == 0)
            {
                return s;
            }

            
            int lastNewlineIndex = s.LastIndexOf('\n');
            float ratioOfSizes = Size.X / Textures.Text.CalculateOffset(s.Substring(lastNewlineIndex + 1), underlyingText.Font, underlyingText.TextSize).X;
            int len = (int)((s.Length - lastNewlineIndex + 1) * ratioOfSizes) + lastNewlineIndex + 1;
            if (ratioOfSizes > 1 || len >= s.Length) return s;
            
            int spaceIndex = s.IndexOf(' ', len);

            if (spaceIndex == -1) return s; //TODO: Think more about this

            spaceIndex = TextWidthLineLengthCheck(s, lastNewlineIndex, spaceIndex);

            s = s.Remove(spaceIndex, 1);
            s = s.Insert(spaceIndex, "\n");

            return TextWidthFixer(s);
        }

        int TextWidthLineLengthCheck(string s, int aLastNewlineIndex, int aSpaceIndex)
        {
            if (Textures.Text.CalculateOffset(s.Substring(aLastNewlineIndex + 1, aSpaceIndex - (aLastNewlineIndex + 1)), underlyingText.Font, underlyingText.TextSize).X / Size.X < 1) return aSpaceIndex;

            int newSpaceIndex = s.Substring(aLastNewlineIndex + 1, aSpaceIndex - (aLastNewlineIndex + 1)).LastIndexOf(' ') + aLastNewlineIndex + 1;
            return TextWidthLineLengthCheck(s, aLastNewlineIndex, newSpaceIndex);
            
        }

        public Vector2 UnderlyingTextOffset => underlyingText.Offset;
        public Vector2 CalculatePartialOffset(int aIndexToCalculateTo) => underlyingText.CalculatePartialOffset(aIndexToCalculateTo);

        public bool MouseOverText => TextBounds.Contains(UiMouseStateCache.Absolute.ToPoint());

        public Rectangle TextBounds
        {
            get
            {
                Vector2 offset = underlyingText.Offset;
                if (offset == Vector2.Zero) return Rectangle.Empty;

                AbsoluteScreenPosition drawPos;
                Vector2 drawOffset;
                GetDrawTransform(out drawPos, out drawOffset);

                Point location = new Point(
                    drawPos.X - (int)MathF.Ceiling(drawOffset.X),
                    drawPos.Y - (int)MathF.Ceiling(drawOffset.Y));

                Point size = new Point(
                    (int)MathF.Ceiling(offset.X),
                    (int)MathF.Ceiling(offset.Y));

                return new Rectangle(location, size);
            }
        }

        void GetDrawTransform(out AbsoluteScreenPosition drawPos, out Vector2 drawOffset)
        {
            Vector2 textOffset = underlyingText.Offset;
            float centeredYOffset = underlyingText.CenteredYOffset;
            switch (textAlignment)
            {
                case TextAllignment.CentreLeft:
                    drawPos = Location + Size.OnlyY / 2;
                    drawOffset = new Vector2(0, centeredYOffset);
                    return;
                case TextAllignment.CentreRight:
                    drawPos = Location + new AbsoluteScreenPosition(Size.X, Size.Y / 2);
                    drawOffset = new Vector2(textOffset.X, centeredYOffset);
                    return;
                case TextAllignment.Centred:
                    drawPos = Location + Size / 2;
                    drawOffset = new Vector2(textOffset.X / 2, centeredYOffset);
                    return;
                case TextAllignment.TopLeft:
                    drawPos = Location;
                    drawOffset = Vector2.Zero;
                    return;
                case TextAllignment.TopCentre:
                    drawPos = Location + new AbsoluteScreenPosition(Size.X / 2, 0);
                    drawOffset = new Vector2(textOffset.X / 2, 0);
                    return;
                case TextAllignment.TopRight:
                    drawPos = Location + new AbsoluteScreenPosition(Size.X, 0);
                    drawOffset = new Vector2(textOffset.X, 0);
                    return;
                case TextAllignment.BottomLeft:
                    drawPos = Location + new AbsoluteScreenPosition(0, Size.Y);
                    drawOffset = new Vector2(0, textOffset.Y);
                    return;
                case TextAllignment.BottomCentre:
                    drawPos = Location + new AbsoluteScreenPosition(Size.X / 2, Size.Y);
                    drawOffset = new Vector2(textOffset.X / 2, textOffset.Y);
                    return;
                case TextAllignment.BottomRight:
                    drawPos = Location + new AbsoluteScreenPosition(Size.X, Size.Y);
                    drawOffset = new Vector2(textOffset.X, textOffset.Y);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }



        public override Color Color
        {
            get
            {
                return underlyingText.Color;
            }
            set
            {
                if (underlyingText.Color == value) return;
                underlyingText.Color = value;
                MarkRenderStale();
            }
        }

        public float TextSize
        {
            get => underlyingText.TextSize;
            set
            {
                underlyingText.TextSize = value;
                Text = underlyingText.Value;
                MarkRenderStale();
            }
        }

        TextAllignment textAlignment;
        Text  underlyingText;

        public Label(UIElement aParent, RelativeScreenPosition aPos, RelativeScreenPosition aSize, TextAllignment aTextAlignment, Color? aTextColor = null, string aFontname = "Comfortaa-msdf", float aTextSize = 12f, string aText = "", UITexture aBackground = null, float aBorderWidthPx = 0f, Color? aBorderColor = null) : base(aParent, aBackground, aPos, aSize)
        {
            underlyingText = aTextColor.HasValue ? new Text(aFontname, aText, aTextColor.Value, aTextSize, aBorderColor, aBorderWidthPx) : new Text(aFontname, aText, aTextSize, aBorderColor, aBorderWidthPx);
            textAlignment = aTextAlignment;
            capturesClick = false;
            capturesRelease = false;
        }

        public override void Rescale()
        {
            base.Rescale();
            underlyingText.Rescale();
        }

        protected override void DrawSelf(SpriteBatch aBatch)
        {
            Project_1.Managers.ThreadAffinity.AssertMainThread();
            base.DrawSelf(aBatch);


            GraphicsManager.CaptureScissor(this, AbsolutePos);

            switch (textAlignment)
            {
                case TextAllignment.CentreLeft:
                    //TODO: Add settable offset?
                    underlyingText.CentreLeftDraw(aBatch, Location + Size.OnlyY / 2);
                    break;

                case TextAllignment.CentreRight:
                    underlyingText.CentreRightDraw(aBatch, Location + new AbsoluteScreenPosition(Size.X, Size.Y / 2));
                    break;

                case TextAllignment.Centred:
                    underlyingText.CentredDraw(aBatch, Location + Size / 2);
                    break;

                case TextAllignment.TopLeft:
                    underlyingText.TopLeftDraw(aBatch, Location);
                    break;

                case TextAllignment.TopCentre:
                    underlyingText.TopCentreDraw(aBatch, Location + new AbsoluteScreenPosition(Size.X / 2, 0));
                    break;

                case TextAllignment.TopRight:
                    underlyingText.TopRightDraw(aBatch, Location + new AbsoluteScreenPosition(Size.X, 0));
                    break;

                case TextAllignment.BottomLeft:
                    underlyingText.BottomLeftDraw(aBatch, Location + new AbsoluteScreenPosition(0, Size.Y));
                    break;

                case TextAllignment.BottomCentre:
                    underlyingText.BottomCentreDraw(aBatch, Location + new AbsoluteScreenPosition(Size.X / 2, Size.Y));
                    break;

                case TextAllignment.BottomRight:
                    underlyingText.BottomRightDraw(aBatch, Location + new AbsoluteScreenPosition(Size.X, Size.Y));
                    break;

                default:
                    throw new NotImplementedException();
            }

            GraphicsManager.ReleaseScissor(this);
        }
    }
}
