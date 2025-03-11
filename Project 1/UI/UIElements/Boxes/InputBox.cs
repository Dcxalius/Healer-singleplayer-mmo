using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Boxes
{
    internal class InputBox : Box //TODO: Bug, when unselected displays nothing
    {
        Label inputLabel;
        Label beforeWindowLabel;
        public string Input => text;
        string text;

        Text cursor;
        Color postClickColor;
        public InputBox(string aTextBeforeInputWindow, Color aTextBeforeColor, string aDisplayText, Color aBackgroundColor, Color aPassiveColor, Color aPostClickColor, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("GrayBackground", aBackgroundColor), aPos, aSize)
        {
            postClickColor = aPostClickColor;
            RelativeScreenPosition spacingSquare = RelativeScreenPosition.GetSquareFromY(0.01f);
            RelativeScreenPosition position = spacingSquare;
            RelativeScreenPosition size = aSize - spacingSquare * 2;

            RelativeScreenPosition textSize = new AbsoluteScreenPosition(TextureManager.GetFont("Gloryse").MeasureString(aTextBeforeInputWindow).ToPoint()).ToRelativeScreenPosition();
            beforeWindowLabel = new Label(aTextBeforeInputWindow, position, size.OnlyY + textSize.OnlyX, Label.TextAllignment.CentreLeft, aTextBeforeColor);
            AddChild(beforeWindowLabel);


            Box textBackgroundBox = new Box(new UITexture("WhiteBackground", aBackgroundColor), position + beforeWindowLabel.RelativeSize.OnlyX + position.OnlyX, size - beforeWindowLabel.RelativeSize.OnlyX - position.OnlyX);
            textBackgroundBox.CapturesClick = false;
            AddChild(textBackgroundBox);


            inputLabel = new Label(aDisplayText, position + position.OnlyX * 2 + beforeWindowLabel.RelativeSize.OnlyX, size - beforeWindowLabel.RelativeSize.OnlyX - position.OnlyX * 2, Label.TextAllignment.CentreLeft, aPassiveColor);
            AddChild(inputLabel);

            cursor = new Textures.Text("Gloryse", "|", aPostClickColor);
            text = "";
        }


        public override void ClickedOnAndReleasedOnMe()
        {
            inputLabel.Color = Color.Black;
            InputManager.InputToWriteTo = this;
            inputLabel.Text = "";
            base.ClickedOnAndReleasedOnMe();
        }

        public void WriteTo(string aString, int aIndex)
        {
            if (aIndex == text.Length) text += aString;
            else text = text.Insert(aIndex, aString);

            inputLabel.Text = text;
        }

        public void Backstep(bool aControlHeld, int aIndex)
        {
            if (aIndex == 0) return;
            if (!aControlHeld)
            {
                text = text.Remove(aIndex - 1, 1);
                inputLabel.Text = text;

                return;
            }
            int index = text.LastIndexOf(' ');
            if (index != -1) text.Remove(index, aIndex - index);
            else text = text.Remove(0, aIndex);
           

            inputLabel.Text = text;

        }

        public void Delete(bool aControlHeld, int aIndex)
        {
            if (aIndex == text.Length) return;
            if(!aControlHeld)
            {
                text = text.Remove(aIndex, 1);
                inputLabel.Text = text;

                return;
            }
            int index = text.LastIndexOf(' ', aIndex);
            if (index != -1) text.Remove(index, aIndex - index);
            else text = "";


            inputLabel.Text = text;
        }

        public AbsoluteScreenPosition WhereToDrawCursor(int aCursorPosition)
        {
            return new AbsoluteScreenPosition(inputLabel.CalculatePartialOffset(aCursorPosition).ToPoint()).OnlyX + inputLabel.Location + inputLabel.Size.OnlyY / 2;
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);

            if (InputManager.InputToWriteTo != this) return;
            if (TimeManager.TotalFrameTimeAsTimeSpan.TotalMilliseconds % 750 < 250) return;
            
            cursor.CentredDraw(aBatch, WhereToDrawCursor(InputManager.CursorPosition));
            
        }
    }
}
