using Microsoft.Xna.Framework;
using Project_1.Textures;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.PauseMenu
{
    internal class PauseBox : Box
    {

        static Vector2 pauseSize = new Vector2(0.2f, 0.5f);
        static Vector2 pausePos = new Vector2(0.5f - (pauseSize.X / 2), 0.5f - pauseSize.Y / 2);
        static UITexture gfx = new UITexture("WhiteBackground", Color.DarkGray);

        static Vector2 buttonSize = new Vector2(pauseSize.X / 5 * 4, pauseSize.Y / 12);
        static Vector2 buttonStartingPos = new Vector2(pauseSize.X / 2 - buttonSize.X / 2, 0.05f);
        static Vector2 buttonOffset = new Vector2(0, buttonSize.Y * 1.5f);

        static int buttonIndex = 0;

        static Vector2 GetStartPosition
        {
            get
            {
                return buttonStartingPos + buttonOffset * buttonIndex++;
            }
        }

        public PauseBox() : base (null, gfx, pausePos, pauseSize) 
        {
            children.Add(new MainMenuButton(Pos, GetStartPosition, buttonSize));
            children.Add(new OptionMenuButton(Pos, GetStartPosition, buttonSize));
            children.Add(new Button(Pos, GetStartPosition, buttonSize, Color.BurlyWood));
            children.Add(new Button(Pos, GetStartPosition, buttonSize, Color.Green));
            children.Add(new ExitGameButton(Pos, GetStartPosition, buttonSize));
        }

    }
}
