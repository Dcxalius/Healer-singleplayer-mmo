using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.UI.UIElements.Buttons;

namespace Project_1.UI.OptionMenu
{
    internal class ChatOptionsButton : Button
    {
        public ChatOptionsButton(RelativeScreenPosition aPos, RelativeScreenPosition aSize, UI.UIElements.UIElement aParent = null) : base(aPos, aSize, Color.White, "Chat", Color.Black, aParent)
        {
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();
            OptionManager.SetScreen(OptionManager.OptionScreen.Chat);
        }
    }
}
