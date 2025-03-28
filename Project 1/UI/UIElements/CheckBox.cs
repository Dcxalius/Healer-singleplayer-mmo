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
    internal class CheckBox : UIElement
    {
        public bool Ticked
        {
            get
            {
                return ticked;
            }
            protected set
            {
                ticked = value;
                if (ticked) DoTickedActions();
                else DoUntickActions();
            }
        }

        bool ticked;

        static UITexture checkMark = new UITexture("CheckMark", Color.White);

        void DoTickedActions()
        {
            for (int i = 0; i < tickedActions.Count; i++)
            {
                tickedActions[i].Invoke();
            }
        }

        void DoUntickActions()
        {
            for (int i = 0; i < untickedActions.Count; i++)
            {
                untickedActions[i].Invoke();
            }
        }
        List<Action> tickedActions;
        List<Action> untickedActions;

        public CheckBox(bool aStartState, Action aTickedAction, Action aUntickedAction, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : this(aStartState, new List<Action> { aTickedAction }, new List<Action> { aUntickedAction }, aPos, aSize) { }
        public CheckBox(bool aStartState, List<Action> aTickedActions, List<Action> aUntickedActions, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("CheckBox", Color.White), aPos, aSize)
        {
            capturesClick = true;
            ticked = aStartState;
            tickedActions = new List<Action>(aTickedActions);
            untickedActions = new List<Action>(aUntickedActions);
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();

            Ticked = !Ticked;
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);

            if (!ticked) return;

            checkMark.Draw(aBatch, AbsolutePos, Color.White);
        }
    }
}
