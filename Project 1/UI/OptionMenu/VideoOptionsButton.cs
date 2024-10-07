using Microsoft.Xna.Framework;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.OptionMenu
{
    internal class VideoOptionsButton : Button
    {
        public VideoOptionsButton(Vector2 aPos, Vector2 aSize) : base(aPos, aSize, Color.White, "Video", Color.Black)
        {

        }

        public override void HoldReleaseOnMe()
        {
            base.HoldReleaseOnMe();

            OptionManager.SetScreen(OptionManager.OptionScreen.Video);
        }
    }
}
