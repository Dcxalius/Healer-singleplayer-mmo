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
    internal class ExitOptionsButton : GFXButton
    {
        List<Action> onExit;

        public ExitOptionsButton() : base(new GfxPath(GfxType.UI, "XButton"), new RelativeScreenPosition(0.9f), new RelativeScreenPosition(0.05f), Color.Beige)
        {
            onExit = new List<Action>();

        }

        public void AddFuncToTriggerOnExit(Action aAction)
        {
            if (onExit.Contains(aAction))
            {
                return;
            }
            
            onExit.Add(aAction);
        }

        public override void HoldReleaseOnMe()
        {
            base.HoldReleaseOnMe();
         
            OptionManager.CloseAllOptionMenuStuff();

            for (int i = 0; i < onExit.Count; i++)
            {
                onExit[i].Invoke();
            }
            onExit.Clear();

            StateManager.SetState(StateManager.States.PauseMenu);
        }
    }
}
