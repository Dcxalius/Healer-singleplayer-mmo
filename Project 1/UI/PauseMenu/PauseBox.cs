using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers;
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
        static UITexture staticGfx;
        static bool gfxInitialized;

        static RelativeScreenPosition pausePos = new RelativeScreenPosition(0.5f - (pauseSize.X / 2), 0.5f - pauseSize.Y / 2);


        static UITexture EnsureGfx()
        {
            if (gfxInitialized) return staticGfx;
            ThreadAffinity.AssertMainThread();
            staticGfx = new UITexture("WhiteBackground", Color.DarkGray);
            gfxInitialized = true;
            return staticGfx;
        }

        public PauseBox() : base(null, EnsureGfx(), pausePos, pauseSize) 
        {
            AddChild(new ResumeButton(this, GetStartPositionFromTop, ButtonSize));
            AddChild(new OptionMenuButton(this, GetStartPositionFromTop, ButtonSize));
            AddChild(new Button(this, new List<Action>() { new Action(() => StateManager.RequestStateChange(StateManager.States.MoveHUD)) }, GetStartPositionFromTop, ButtonSize, Color.WhiteSmoke, "Move HUD", Color.Black));
            
            AddChild(new SaveButton(this, GetStartPositionFromTop, ButtonSize));
            AddChild(new LoadButton(this, GetStartPositionFromTop, ButtonSize));



            AddChild(new ExitGameButton(this, GetStartPositionFromBottom, ButtonSize));
            AddChild(new MainMenuButton(this, GetStartPositionFromBottom, ButtonSize));
        }

    }
}
