using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers.States;
using Project_1.UI.UIElements.Buttons;

namespace Project_1.UI.StartMenu
{
    internal class LoadGameButton : Button
    {
        public LoadGameButton(RelativeScreenPosition aPos, RelativeScreenPosition aSize, UI.UIElements.UIElement aParent = null) : base(aPos, aSize, Color.LightYellow, "Load", Color.Black, aParent)
        {
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();
            StateManager.RequestStateChange(StateManager.States.LoadingMenu);
        }
    }
}
