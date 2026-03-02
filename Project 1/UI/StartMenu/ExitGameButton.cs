using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.UI.UIElements.Buttons;

namespace Project_1.UI.StartMenu
{
    internal class ExitGameButton : Button
    {
        public ExitGameButton(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aPos, aSize, Color.LightGreen, "Exit", Color.Black)
        {
        }


        public override void ClickedOnAndReleasedOnMe()
        {
            Game1.Instance.Exit();

            base.ClickedOnAndReleasedOnMe();
        }
    }
}
