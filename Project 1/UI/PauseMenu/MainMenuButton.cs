﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.PauseMenu
{
    internal class MainMenuButton : Button
    {

        public MainMenuButton(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aPos, aSize, Color.Gray, "Main Menu")
        {

        }

        public override void HoldReleaseOnMe()
        {
            StateManager.SetState(StateManager.State.StartMenu);

            base.HoldReleaseOnMe();
        }
    }
}
