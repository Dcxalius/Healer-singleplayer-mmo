using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.UI.UIElements.Buttons;

namespace Project_1.UI.OptionMenu
{
    internal class DebugOptionsButton : Button
    {
        public DebugOptionsButton(UI.UIElements.UIElement aParent, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, aPos, aSize, Color.White, "Debug", Color.Black)
        {
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();

            OptionManager.SetScreen(OptionManager.OptionScreen.Debug);
        }
    }
}
