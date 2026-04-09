using Microsoft.Xna.Framework;
using Project_1.Camera;
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
    internal class LoadButton : Button
    {
        public LoadButton(UI.UIElements.UIElement aParent, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, aPos, aSize, Color.Magenta, "Load", Color.Teal)
        {

        }

        public override void ClickedOnAndReleasedOnMe()
        {
            MailboxManager.PublishSimCommand(new LoadSaveRequested(null));

            base.ClickedOnAndReleasedOnMe();

        }
    }
}
