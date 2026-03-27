using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.UI.UIElements.Buttons;

namespace Project_1.UI.StartMenu
{
    internal class ExitGameButton : Button
    {
        public ExitGameButton(RelativeScreenPosition aPos, RelativeScreenPosition aSize, UI.UIElements.UIElement aParent = null) : base(aPos, aSize, Color.LightGreen, "Exit", Color.Black, aParent)
        {
        }


        public override void ClickedOnAndReleasedOnMe()
        {
            Game1.Instance.Exit();

            base.ClickedOnAndReleasedOnMe();
        }
    }
}
