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

namespace Project_1.UI.LoadingMenu
{
    internal class ExitButton : GFXButton
    {
        public ExitButton(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new GfxPath(GfxType.UI, "XButton"), aPos, aSize, Color.White)
        {
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();

            StateManager.SetState(StateManager.States.StartScreen);
        }
    }
}
