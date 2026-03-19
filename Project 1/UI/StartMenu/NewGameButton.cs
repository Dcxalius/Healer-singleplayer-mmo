using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.UI.UIElements.Buttons;

namespace Project_1.UI.StartMenu
{
    internal class NewGameButton : Button
    {
        public NewGameButton(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aPos, aSize, Color.LightCyan, "New Game", Color.Black)
        {

        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();

            MailboxManager.PublishSimCommand(new NewGameRequested());
        }
    }
}
