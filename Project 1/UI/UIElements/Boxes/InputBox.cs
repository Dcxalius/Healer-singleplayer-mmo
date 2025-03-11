using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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

namespace Project_1.UI.UIElements.Boxes
{
    internal class InputBox : Box
    {
        public enum ValidInputs
        {
            Letters,
            Digits,
            UpperCaseLetters,
            LowerCaseLetters,
            Symbols
        }

        Label inputLabel;
        Label beforeWindowLabel;
        public string Input => text;
        string text;

        Text cursor;
        Color postClickColor;

        ValidInputs[] validInputs;


        public bool ValidInput(Keys aKey)
        {
            for (int i = 0; i < validInputs.Length; i++)
            {


                switch (validInputs[i])
                {
                    case ValidInputs.Letters: //TODO: Should this be settable or should the two bellow be merged to this one
                    case ValidInputs.UpperCaseLetters:
                    case ValidInputs.LowerCaseLetters:
                        if (Keys.A < aKey || Keys.Z > aKey) return false;
                        break;
                    case ValidInputs.Digits:
                        if (Keys.D0 > aKey || Keys.D9 < aKey) return false;
                        break;
                    case ValidInputs.Symbols:
                        throw new NotImplementedException();

                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            return true;
        }
        public InputBox(string aTextBeforeInputWindow, ValidInputs[] aSetOfValidInputs, Color aTextBeforeColor, string aDisplayText, Color aBackgroundColor, Color aPassiveColor, Color aPostClickColor, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("GrayBackground", aBackgroundColor), aPos, aSize)
        {
            Debug.Assert(aSetOfValidInputs.Length != 0, "Made an inputbox without any legal inputs");
            validInputs = aSetOfValidInputs;
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
            inputLabel.Text = text;
            base.ClickedOnAndReleasedOnMe();
        }

        public void WriteTo(char aCharToWrite, int aIndex)
        {
            if (validInputs.Contains(ValidInputs.LowerCaseLetters) && !validInputs.Contains(ValidInputs.UpperCaseLetters) && char.IsUpper(aCharToWrite)) aCharToWrite = char.ToLower(aCharToWrite);
            if (validInputs.Contains(ValidInputs.UpperCaseLetters) && !validInputs.Contains(ValidInputs.LowerCaseLetters) && char.IsLower(aCharToWrite)) aCharToWrite = char.ToUpper(aCharToWrite);
            if (aIndex == text.Length) text += aCharToWrite;
            else text = text.Insert(aIndex, aCharToWrite.ToString());
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
