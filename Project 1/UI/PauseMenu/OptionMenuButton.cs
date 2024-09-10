using Microsoft.Xna.Framework;
using Project_1.Managers;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.PauseMenu
{
    internal class OptionMenuButton : Button
    {
        public OptionMenuButton(in Rectangle? aParentPos, Vector2 aPos, Vector2 aSize) : base(in aParentPos, aPos, aSize, Color.Blue)
        {
            ButtonText = "Options Menu";
        }

        public override void HoldReleaseOnMe()
        {
            StateManager.SetState(State.Options);

            base.HoldReleaseOnMe();
        }
    }
}
