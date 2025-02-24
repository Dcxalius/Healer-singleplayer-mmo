using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.PauseMenu
{
    internal class PauseBox : Box
    {

        static RelativeScreenPosition pauseSize = new RelativeScreenPosition(0.2f, 0.5f);
        static RelativeScreenPosition pausePos = new RelativeScreenPosition(0.5f - (pauseSize.X / 2), 0.5f - pauseSize.Y / 2);
        static UITexture staticGfx = new UITexture("WhiteBackground", Color.DarkGray);

        static RelativeScreenPosition buttonSize = new RelativeScreenPosition(pauseSize.X / 5 * 4, pauseSize.Y / 12);
        static RelativeScreenPosition buttonStartingPos = new RelativeScreenPosition(pauseSize.X / 2 - buttonSize.X / 2, 0.05f);
        static RelativeScreenPosition buttonStartingFromBottomPos = new RelativeScreenPosition(pauseSize.X / 2 - buttonSize.X / 2, pauseSize.Y - 0.05f - buttonSize.Y);
        static RelativeScreenPosition buttonOffset = new RelativeScreenPosition(0, buttonSize.Y * 1.5f);

        static int buttonIndex = 0;
        static int buttonIndexFromBottom = 0;

        static RelativeScreenPosition GetStartPositionFromTop //TODO: Get this logic out to a menu class
        {
            get
            {
                return buttonStartingPos + buttonOffset * buttonIndex++;
            }
        }

        static RelativeScreenPosition GetStartPositionFromBottom
        {
            get
            {
                return buttonStartingFromBottomPos - buttonOffset * buttonIndexFromBottom++;
            }
        }

        public PauseBox() : base (staticGfx, pausePos, pauseSize) 
        {
            AddChild(new ResumeButton(GetStartPositionFromTop, buttonSize));
            AddChild(new OptionMenuButton(GetStartPositionFromTop, buttonSize));
            AddChild(new SaveButton(GetStartPositionFromTop, buttonSize));
            //children.Add(new Button(GetStartPosition, buttonSize, Color.BurlyWood));
            //children.Add(new Button(GetStartPosition, buttonSize, Color.Green));
            AddChild(new ExitGameButton(GetStartPositionFromBottom, buttonSize));
            AddChild(new MainMenuButton(GetStartPositionFromBottom, buttonSize));
        }

    }
}
