using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers.States;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.PauseMenu
{
    internal class PauseBox : MenuBox
    {
        static RelativeScreenPosition pauseSize = new RelativeScreenPosition(0.2f, 0.5f);
        static UITexture staticGfx = new UITexture("WhiteBackground", Color.DarkGray);

        static RelativeScreenPosition pausePos = new RelativeScreenPosition(0.5f - (pauseSize.X / 2), 0.5f - pauseSize.Y / 2);


        public PauseBox() : base (staticGfx, pausePos, pauseSize) 
        {
            AddChild(new ResumeButton(GetStartPositionFromTop, ButtonSize));
            AddChild(new OptionMenuButton(GetStartPositionFromTop, ButtonSize));
            AddChild(new Button(new List<Action>() { new Action(() => StateManager.SetState(StateManager.States.MoveHUD)) }, GetStartPositionFromTop, ButtonSize, Color.WhiteSmoke, "Move HUD", Color.Black));
            
            AddChild(new SaveButton(GetStartPositionFromTop, ButtonSize));
            AddChild(new LoadButton(GetStartPositionFromTop, ButtonSize));



            AddChild(new ExitGameButton(GetStartPositionFromBottom, ButtonSize));
            AddChild(new MainMenuButton(GetStartPositionFromBottom, ButtonSize));
        }

    }
}
