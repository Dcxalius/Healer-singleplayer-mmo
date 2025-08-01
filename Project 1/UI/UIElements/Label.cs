﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
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
            set => underlyingText.Value = TextWidthFixer(value); //TODO: If length is longer than Size.Y and a toggle is on then the text should become scrollable.
        }

        public string TextWidthFixer(string s)
        {
            if (s == null) return null;
            if (Textures.Text.CalculateOffset(s, underlyingText.Font).X <= Size.X || Textures.Text.CalculateOffset(s, underlyingText.Font).X == 0 || Size.X == 0) return s;

            
            int lastNewlineIndex = s.LastIndexOf('\n');
            float ratioOfSizes = Size.X / Textures.Text.CalculateOffset(s.Substring(lastNewlineIndex + 1), underlyingText.Font).X;
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
            if (Textures.Text.CalculateOffset(s.Substring(aLastNewlineIndex + 1, aSpaceIndex - (aLastNewlineIndex + 1)), underlyingText.Font).X / Size.X < 1) return aSpaceIndex;

            int newSpaceIndex = s.Substring(aLastNewlineIndex + 1, aSpaceIndex - (aLastNewlineIndex + 1)).LastIndexOf(' ') + aLastNewlineIndex + 1;
            return TextWidthLineLengthCheck(s, aLastNewlineIndex, newSpaceIndex);
            
        }

        public Vector2 UnderlyingTextOffset => underlyingText.Offset;
        public Vector2 CalculatePartialOffset(int aIndexToCalculateTo) => underlyingText.CalculatePartialOffset(aIndexToCalculateTo);

        TextAllignment textAlignment;

        public override Color Color
        {
            get
            {
                return underlyingText.Color;
            }
            set
            {
                underlyingText.Color = value;
            }
        }
        Text  underlyingText;

        public Label(string aText, RelativeScreenPosition aPos, RelativeScreenPosition aSize, TextAllignment aTextAlignment, Color? aTextColor = null, string aFontname = "Gloryse") : base(null, aPos, aSize)
        {
            underlyingText = aTextColor.HasValue ? new Text(aFontname, aText, aTextColor.Value) : new Text(aFontname, aText);
            textAlignment = aTextAlignment;
            capturesClick = false;
            capturesRelease = false;
        }

        public override void Rescale()
        {
            base.Rescale();
            underlyingText.Rescale();
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);


            GraphicsManager.CaptureScissor(this, AbsolutePos);

            switch (textAlignment)
            {
                case TextAllignment.CentreLeft:
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
                    //underlyingText.LeftCentredDraw(aBatch, (AbsoluteScreenPosition)AbsolutePos.Location + new AbsoluteScreenPosition(0, (int)(underlyingText.Offset.Y / 2)));
                    break;

                case TextAllignment.TopCentre:
                    underlyingText.TopCentreDraw(aBatch, Location + new AbsoluteScreenPosition(Size.X / 2, 0));
                    break;

                case TextAllignment.TopRight:
                    underlyingText.TopRightDraw(aBatch, Location + new AbsoluteScreenPosition(Size.X, 0));

                    //underlyingText.RightCentredDraw(aBatch, (AbsoluteScreenPosition)AbsolutePos.Location + new AbsoluteScreenPosition(AbsolutePos.Size.X, 0) + new AbsoluteScreenPosition(0, (int)(underlyingText.Offset.Y / 2)));
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
