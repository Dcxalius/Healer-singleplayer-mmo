using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Textures;
using System;
using System.Collections.Generic;
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
            set => underlyingText.Value = value;
        }

        TextAllignment textAlignment;

        Text underlyingText;

        public Label(string aText, RelativeScreenPosition aPos, RelativeScreenPosition aSize, TextAllignment aTextAlignment, Color? aColor = null, string aFontname = "Gloryse") : base(null, aPos, aSize)
        {
            underlyingText = aColor.HasValue ? new Text(aFontname, aText, aColor.Value) : new Text(aFontname, aText);
            textAlignment = aTextAlignment;
        }

        public override void Rescale()
        {
            base.Rescale();
            underlyingText.Rescale();
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);

            switch (textAlignment)
            {
                case TextAllignment.CentreLeft:
                    underlyingText.CentreLeftDraw(aBatch, new AbsoluteScreenPosition(AbsolutePos.Location + new Vector2(0, AbsolutePos.Size.Y / 2).ToPoint()));
                    break;

                case TextAllignment.CentreRight:
                    underlyingText.CentreRightDraw(aBatch, new AbsoluteScreenPosition(AbsolutePos.Location + new Vector2(AbsolutePos.Size.X, AbsolutePos.Size.Y / 2).ToPoint()));
                    break;

                case TextAllignment.Centred:
                    underlyingText.CentredDraw(aBatch, new AbsoluteScreenPosition(AbsolutePos.Location + new Vector2(AbsolutePos.Size.X / 2, AbsolutePos.Size.Y / 2).ToPoint()));
                    break;

                case TextAllignment.TopLeft:
                    underlyingText.TopLeftDraw(aBatch, (AbsoluteScreenPosition)AbsolutePos.Location);
                    //underlyingText.LeftCentredDraw(aBatch, (AbsoluteScreenPosition)AbsolutePos.Location + new AbsoluteScreenPosition(0, (int)(underlyingText.Offset.Y / 2)));
                    break;

                case TextAllignment.TopCentre:
                    underlyingText.TopCentreDraw(aBatch, (AbsoluteScreenPosition)AbsolutePos.Location + new AbsoluteScreenPosition(AbsolutePos.Size.X / 2, 0));
                    break;

                case TextAllignment.TopRight:
                    underlyingText.TopRightDraw(aBatch, (AbsoluteScreenPosition)AbsolutePos.Location + new AbsoluteScreenPosition(AbsolutePos.Size.X, 0));

                    //underlyingText.RightCentredDraw(aBatch, (AbsoluteScreenPosition)AbsolutePos.Location + new AbsoluteScreenPosition(AbsolutePos.Size.X, 0) + new AbsoluteScreenPosition(0, (int)(underlyingText.Offset.Y / 2)));
                    break;

                case TextAllignment.BottomLeft:
                    underlyingText.BottomLeftDraw(aBatch, (AbsoluteScreenPosition)AbsolutePos.Location + new AbsoluteScreenPosition(0, AbsolutePos.Size.Y));
                    break;

                case TextAllignment.BottomCentre:
                    underlyingText.BottomCentreDraw(aBatch, (AbsoluteScreenPosition)AbsolutePos.Location + new AbsoluteScreenPosition(AbsolutePos.Size.X / 2, AbsolutePos.Size.Y));
                    break;

                case TextAllignment.BottomRight:
                    underlyingText.BottomRightDraw(aBatch, (AbsoluteScreenPosition)AbsolutePos.Location + new AbsoluteScreenPosition(AbsolutePos.Size.X, AbsolutePos.Size.Y));
                    break;

                default:
                    throw new NotImplementedException();
            }



        }
    }
}
