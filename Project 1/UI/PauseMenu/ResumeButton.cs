﻿using Microsoft.Xna.Framework;
using Project_1.Managers;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.PauseMenu
{
    internal class ResumeButton : Button
    {
        public ResumeButton(Vector2 aPos, Vector2 aSize) : base(aPos, aSize, Color.PaleVioletRed)
        {
            ButtonText = "Resume Game";
        }

        public override void HoldReleaseOnMe()
        {
            base.HoldReleaseOnMe();

            StateManager.currentState = State.Game;
        }
    }
}