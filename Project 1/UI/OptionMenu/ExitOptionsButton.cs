using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers.States;
using Project_1.Textures;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.OptionMenu
{
    internal class ExitOptionsButton : Button
    {
        List<Action> onExit;

        public ExitOptionsButton(RelativeScreenPosition aPos, RelativeScreenPosition aSize, Color aColor, Color aTextColor) : base(new RelativeScreenPosition(0.9f), new RelativeScreenPosition(0.08f, 0.05f), Color.Beige, "Close", Color.Black)
        {
            onExit = new List<Action>();

        }

        public void AddFuncToTriggerOnExit(Action aAction)
        {
            if (onExit.Contains(aAction)) return;
            onExit.Add(aAction);
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();
         
            OptionManager.CloseAllOptionMenuStuff();

            OptionManager.ChangesMade = false;

            for (int i = onExit.Count - 1; i >= 0; i--)
            {
                onExit[i].Invoke();
            }
            onExit.Clear();

            StateManager.SetState(StateManager.PreviousState);
        }

        public void ClearActions()
        {
            onExit.Clear();
        }
    }
}
