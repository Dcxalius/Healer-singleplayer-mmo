using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers.States;
using Project_1.UI.UIElements.Buttons;

namespace Project_1.UI.StartMenu
{
    internal class LoadGameButton : Button
    {
        public LoadGameButton(UI.UIElements.UIElement aParent, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, aPos, aSize, Color.LightYellow, "Load", Color.Black)
        {
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();
            StateManager.RequestStateChange(StateManager.States.LoadingMenu);
        }
    }
}
