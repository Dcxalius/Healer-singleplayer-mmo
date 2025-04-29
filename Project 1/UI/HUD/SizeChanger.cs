using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using Project_1.UI.UIElements.Buttons;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD
{
    internal class SizeChanger : Box
    {
        enum Cardinals
        {
            Up,
            Down,
            Left,
            Right,
            Count
        }

        public bool Active
        {
            get => active; 
            set
            {
                active = value;

                if (value == true) return;

                target = null;
                directionBoxX.Input = "";
                directionBoxY.Input = "";
                Visible = false;
            }
        }
        private bool active;

        Label name;
        InputBox directionBoxX;
        InputBox directionBoxY;

        GFXButton[] directionButtons;

       

        UIElement target;

        public SizeChanger() : base(new UITexture("GrayBackground", Color.Gray), RelativeScreenPosition.Zero, new RelativeScreenPosition(0.2f, 0.2f))
        {
            RelativeScreenPosition labelSpacing = new RelativeScreenPosition(0.025f);

            name = new Label("", labelSpacing, RelativeScreenPosition.One - labelSpacing * 2, Label.TextAllignment.TopCentre, Color.White);
            AddChild(name);

            directionButtons = new GFXButton[(int)Cardinals.Count];

            RelativeScreenPosition buttonSize = RelativeScreenPosition.GetSquareFromX(0.08f, Size);
            

            RelativeScreenPosition pos = RelativeScreenPosition.One / 2 - buttonSize / 2;

            directionButtons[0] = new GFXButton(new List<Action> { NudgeValues(Cardinals.Up) }, new GfxPath(GfxType.UI, "UpArrow"), pos, buttonSize, Color.White);
            directionButtons[1] = new GFXButton(new List<Action> { NudgeValues(Cardinals.Down) }, new GfxPath(GfxType.UI, "DownArrow"), pos + buttonSize.OnlyY, buttonSize, Color.White);
            directionButtons[2] = new GFXButton(new List<Action> { NudgeValues(Cardinals.Left) }, new GfxPath(GfxType.UI, "LeftArrow"), pos - buttonSize.OnlyX + buttonSize.OnlyY / 2, buttonSize, Color.White);
            directionButtons[3] = new GFXButton(new List<Action> { NudgeValues(Cardinals.Right) }, new GfxPath(GfxType.UI, "RightArrow"), pos + buttonSize.OnlyX + buttonSize.OnlyY / 2, buttonSize, Color.White);


            AddChildren(directionButtons);

            RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.005f, Size);
            RelativeScreenPosition inputSize = new RelativeScreenPosition(1 - spacing.X * 2, 1 / 12f);
            directionBoxX = new InputBox("X", Size, new InputBox.ValidInputs[] { InputBox.ValidInputs.Digits }, Color.Black, "", Color.White, false, Color.Black, Color.Black, RelativeScreenPosition.One - inputSize - inputSize.OnlyY - spacing - spacing.OnlyY, inputSize);
            directionBoxY = new InputBox("Y", Size, new InputBox.ValidInputs[] { InputBox.ValidInputs.Digits }, Color.Black, "", Color.White, false, Color.Black, Color.Black, RelativeScreenPosition.One - inputSize - spacing, inputSize);

            AddChild(directionBoxX);
            AddChild(directionBoxY);

            AlwaysFullyOnScreen = true;
            Visible = false;
        }

        public void SetElement(UIElement aUIElement)
        {
            target = aUIElement;
            directionBoxX.Input = target.Size.X.ToString();
            directionBoxY.Input = target.Size.Y.ToString();
            

            directionBoxX.SetEnter(new List<Action> { EnterValues });
            directionBoxY.SetEnter(new List<Action> { EnterValues });

            name.Text = aUIElement.GetType().Name;

            Move(aUIElement.RelativePos - RelativeSize);
            Visible = true;
        }

        void EnterValues()
        {
            int x = int.Parse(directionBoxX.Input);
            int y = int.Parse(directionBoxY.Input);

            target.Resize(new AbsoluteScreenPosition(x, y).ToRelativeScreenPosition());
        }

        Action NudgeValues(Cardinals aCardinal)
        {
            AbsoluteScreenPosition sizeChange;

            switch (aCardinal)
            {
                case Cardinals.Up:
                    sizeChange = new AbsoluteScreenPosition(0, -1);
                    break;
                case Cardinals.Down:
                    sizeChange = new AbsoluteScreenPosition(0, 1);
                    break;
                case Cardinals.Left:
                    sizeChange = new AbsoluteScreenPosition(-1, 0);
                    break;
                case Cardinals.Right:
                    sizeChange = new AbsoluteScreenPosition(1, 0);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return new Action(() =>
            {
                target.Resize((target.Size + sizeChange).ToRelativeScreenPosition());
                Move(target.RelativePos - RelativeSize);
            });
        }
    }
}
