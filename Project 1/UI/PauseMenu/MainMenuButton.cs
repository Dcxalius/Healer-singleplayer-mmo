using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers.States;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.PauseMenu
{
    internal class MainMenuButton : Button
    {

        public MainMenuButton(UI.UIElements.UIElement aParent, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, aPos, aSize, Color.Gray, "Main Menu")
        {

        }

        public override void ClickedOnAndReleasedOnMe()
        {
            MailboxManager.PublishSimCommand(new ResetToMainMenuRequested());
            base.ClickedOnAndReleasedOnMe();
        }
    }
}
