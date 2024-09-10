using Microsoft.Xna.Framework;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.PauseMenu
{
    internal class ExitGameButton : Button
    {
        public ExitGameButton(in Rectangle? aParentPos, Vector2 aPos, Vector2 aSize) : base(in aParentPos, aPos, aSize, Color.AliceBlue)
        {
            ButtonText = "Exit Game";
        }

        public override void HoldReleaseOnMe()
        {
            Game1.Instance.Exit();

            base.HoldReleaseOnMe();
        }
    }
}
