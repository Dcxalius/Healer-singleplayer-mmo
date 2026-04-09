using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.UI.UIElements.Buttons;

namespace Project_1.UI.StartMenu
{
    internal class ContinueLastSaveButton : Button
    {
        public ContinueLastSaveButton(UI.UIElements.UIElement aParent, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, aPos, aSize, Color.LightSkyBlue, "Continue", Color.Black)
        {
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();

            MailboxManager.PublishSimCommand(new ContinueLastSaveRequested());
        }
    }
}
