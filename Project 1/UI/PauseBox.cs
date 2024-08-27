using Microsoft.Xna.Framework;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI
{
    internal class PauseBox : Box
    {

        static Vector2 pauseSize = new Vector2(0.2f, 0.5f);
        static Vector2 pausePos = new Vector2(0.5f - (pauseSize.X / 2), 0.5f - pauseSize.Y / 2);
        static GfxPath gfxPath = new GfxPath(GfxType.UI, "WhiteBackground");
        static UITexture gfx = new UITexture(gfxPath, Color.DarkGray);

        static Vector2 buttonSize = new Vector2(pauseSize.X / 5 * 4, pauseSize.Y / 12);
        static Vector2 buttonStartingPos = new Vector2(0.5f - (buttonSize.X / 2), 0.5f - buttonSize.Y / 2);
        static Vector2 buttonOffset = new Vector2(0, buttonSize.Y * 1.5f);

        public PauseBox() : base (gfx, pausePos, pauseSize) 
        {
            children.Add(new Button(buttonStartingPos, buttonSize, Color.Green));
            children.Add(new Button(buttonStartingPos + buttonOffset, buttonSize, Color.BurlyWood));
        }

    }
}
