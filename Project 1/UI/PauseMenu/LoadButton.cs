using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers;
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
        public LoadButton(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aPos, aSize, Color.Magenta, "Load")
        {

        }

        public override void ClickedOnAndReleasedOnMe()
        {
            Mailboxes.PublishSimCommand(new LoadSaveRequested(SaveManager.CurrentSave?.Name));

            base.ClickedOnAndReleasedOnMe();

        }
    }
}
