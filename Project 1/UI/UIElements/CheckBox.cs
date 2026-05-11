using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
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

        static UITexture checkMark;
        static bool checkMarkInitialized;

        static void EnsureCheckMark()
        {
            if (checkMarkInitialized) return;
            ThreadAffinity.AssertMainThread();
            checkMark = new UITexture("CheckMark", Color.White);
            checkMarkInitialized = true;
        }

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

        public CheckBox(UIElement aParent, bool aStartState, Action aTickedAction, Action aUntickedAction, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : this(aParent, aStartState, new List<Action> { aTickedAction }, new List<Action> { aUntickedAction }, aPos, aSize) { }
        public CheckBox(UIElement aParent, bool aStartState, List<Action> aTickedActions, List<Action> aUntickedActions, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, new UITexture("CheckBox", Color.White), aPos, aSize)
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
            MarkRenderStale();
        }

        protected override void DrawSelf(SpriteBatch aBatch)
        {
            Project_1.Managers.ThreadAffinity.AssertMainThread();
            base.DrawSelf(aBatch);

            if (!ticked) return;

            EnsureCheckMark();
            checkMark.Draw(aBatch, AbsolutePos, Color.White);
        }
    }
}
