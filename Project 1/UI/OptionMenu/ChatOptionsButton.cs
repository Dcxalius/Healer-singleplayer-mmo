using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.UI.UIElements.Buttons;

namespace Project_1.UI.OptionMenu
{
    internal class ChatOptionsButton : Button
    {
        public ChatOptionsButton(UI.UIElements.UIElement aParent, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, aPos, aSize, Color.White, "Chat", Color.Black)
        {
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();
            OptionManager.SetScreen(OptionManager.OptionScreen.Chat);
        }
    }
}
