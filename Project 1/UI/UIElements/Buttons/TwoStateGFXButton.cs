using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Buttons
{
    internal class TwoStateGFXButton : Button
    {
        public enum State
        {
            First,
            Second
        }

        public override List<Action> Actions
        {
            get
            {
                if (state == State.First) return base.Actions;
                if (state == State.Second) return secondStateActions;
                throw new NotImplementedException();
            }
        }

        protected UITexture firstGfx;
        protected UITexture secondGfx;
        protected List<Action> secondStateActions;

        Rectangle gfxRectangle;

        public State state;

        public TwoStateGFXButton(List<Action> aActions, GfxPath aPath, List<Action> aSecondActions, GfxPath aSecondPath, RelativeScreenPosition aPos, RelativeScreenPosition aSize, Color aColor, string aText = null, Color? aTextColor = null) : base(aActions, aPos, aSize, aColor, aText, aTextColor)
        {
            state = State.First;

            firstGfx = new UITexture(aPath, Color.White);
            secondGfx = new UITexture(aSecondPath, Color.White);

            secondStateActions = aSecondActions;
        }

        public override void Update()
        {
            base.Update();

            gfxRectangle = ConstructGfxRect();
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();

            if (state == State.First) state = State.Second;
            else if (state == State.Second) state = State.First;
        }

        protected void AddAction(Action aAction, State aState)
        {
            if (aState == State.First) AddAction(aAction);
            if (aState == State.Second) secondStateActions.Add(aAction);
        }


        Rectangle ConstructGfxRect()
        {
            Point pos = new Vector2(AbsolutePos.X + AbsolutePos.Size.X / 10, AbsolutePos.Y + AbsolutePos.Size.Y / 10).ToPoint();
            Point size = new Vector2(AbsolutePos.Size.X * 0.8f, AbsolutePos.Size.Y * 0.8f).ToPoint();

            return new Rectangle(pos, size);

        }

        public override void Rescale()
        {
            base.Rescale();
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);

            if (state == State.First)
            {
                Draw(aBatch, firstGfx);
                return;
            }

            if (state == State.Second)
            {
                Draw(aBatch, secondGfx);
            }
        }

        void Draw(SpriteBatch aBatch, UITexture aCurrentStateTexture)
        {
            if (!Pressed)
            {
                aCurrentStateTexture.Draw(aBatch, gfxRectangle);
            }
            else
            {
                aCurrentStateTexture.Draw(aBatch, gfxRectangle, Color.DarkGray);

            }
        }
    }
}
