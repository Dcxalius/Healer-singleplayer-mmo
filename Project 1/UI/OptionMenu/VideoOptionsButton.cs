using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.OptionMenu
{
    internal class VideoOptionsButton : Button
    {
        public VideoOptionsButton(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aPos, aSize, Color.White, "Video", Color.Black)
        {

        }

        public override void HoldReleaseOnMe()
        {
            base.HoldReleaseOnMe();

            OptionManager.SetScreen(OptionManager.OptionScreen.Video);
        }
    }
}
