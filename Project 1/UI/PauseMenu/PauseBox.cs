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
        static Vector2 buttonStartingFromBottomPos = new Vector2(pauseSize.X / 2 - buttonSize.X / 2, pauseSize.Y - 0.05f);
        static Vector2 buttonOffset = new Vector2(0, buttonSize.Y * 1.5f);

        static int buttonIndex = 0;
        static int buttonIndexFromBottom = 0;

        static Vector2 GetStartPositionFromTop //TODO: Get this logic out to a menu class
        {
            get
            {
                return buttonStartingPos + buttonOffset * buttonIndex++;
            }
        }

        static Vector2 GetStartPositionFromBottom
        {
            get
            {
                return buttonStartingFromBottomPos - buttonOffset * buttonIndexFromBottom++;
            }
        }

        public PauseBox() : base (gfx, pausePos, pauseSize) 
        {
            children.Add(new ResumeButton(GetStartPositionFromTop, buttonSize));
            children.Add(new OptionMenuButton(GetStartPositionFromTop, buttonSize));
            //children.Add(new Button(GetStartPosition, buttonSize, Color.BurlyWood));
            //children.Add(new Button(GetStartPosition, buttonSize, Color.Green));
            children.Add(new ExitGameButton(GetStartPositionFromBottom, buttonSize));
            children.Add(new MainMenuButton(GetStartPositionFromBottom, buttonSize));
        }

    }
}
